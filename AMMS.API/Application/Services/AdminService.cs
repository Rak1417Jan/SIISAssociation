using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Admin service implementation with Unit of Work pattern
/// </summary>
public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMLARepository _mlaRepository;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUnitOfWork unitOfWork,
        IMLARepository mlaRepository,
        IAssemblyRepository assemblyRepository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<AdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _mlaRepository = mlaRepository;
        _assemblyRepository = assemblyRepository;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PendingMLAResponse>> GetPendingMLAProfilesAsync(CancellationToken cancellationToken = default)
    {
        // Get all MLA profiles with status Submitted or UnderReview
        var submittedMLAs = await _mlaRepository.GetByStatusAsync(ProfileStatus.Submitted, cancellationToken);
        var underReviewMLAs = await _mlaRepository.GetByStatusAsync(ProfileStatus.UnderReview, cancellationToken);

        var allPendingMLAs = submittedMLAs.Concat(underReviewMLAs);

        var responses = new List<PendingMLAResponse>();

        foreach (var mla in allPendingMLAs)
        {
            // Get user details
            var user = await _userRepository.GetByIdAsync(mla.UserId, cancellationToken);
            
            // Get assembly details
            var assembly = await _assemblyRepository.GetByIdAsync(mla.AssemblyId, cancellationToken);

            responses.Add(new PendingMLAResponse
            {
                Id = mla.Id,
                UserId = mla.UserId,
                AssemblyId = mla.AssemblyId,
                AssemblyNumber = assembly?.AssemblyNumber ?? string.Empty,
                AssemblyName = assembly?.AssemblyName ?? string.Empty,
                Name = mla.Name,
                Party = mla.Party,
                ProfilePictureUrl = mla.ProfilePictureUrl,
                CoverPhotoUrl = mla.CoverPhotoUrl,
                VisionDescription = mla.VisionDescription,
                TermStartDate = mla.TermStartDate,
                TermEndDate = mla.TermEndDate,
                Status = mla.Status,
                StatusName = mla.Status.ToString(),
                MobileNumber = user?.MobileNumber ?? string.Empty,
                Email = user?.Email,
                CreatedAt = mla.CreatedAt,
                SubmittedAt = mla.Status == ProfileStatus.Submitted || mla.Status == ProfileStatus.UnderReview 
                    ? mla.CreatedAt 
                    : null
            });
        }

        return responses.OrderByDescending(r => r.CreatedAt);
    }

    public async Task<MLAResponse> ApproveMLAProfileAsync(ApproveMLARequest request, int adminUserId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var mla = await _mlaRepository.GetByIdAsync(request.MLAId, cancellationToken);
            if (mla == null)
            {
                throw new KeyNotFoundException($"MLA profile with ID {request.MLAId} not found");
            }

            if (mla.Status != ProfileStatus.Submitted && mla.Status != ProfileStatus.UnderReview)
            {
                throw new InvalidOperationException($"MLA profile with ID {request.MLAId} is not in pending status. Current status: {mla.Status}");
            }

            // Get old values for audit
            var oldStatus = mla.Status;

            // Update status to Approved and then Public
            mla.Status = ProfileStatus.Approved;
            mla.UpdatedAt = DateTime.UtcNow;
            _mlaRepository.Update(mla);

            // Create audit log
            var adminUser = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
            var auditLog = new AuditLog
            {
                EntityType = "MLA",
                EntityId = mla.Id,
                Action = "Approve",
                UserId = adminUserId,
                UserName = adminUser?.MobileNumber,
                OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = ProfileStatus.Approved }),
                Description = $"MLA profile approved by admin. Notes: {request.AdminNotes ?? "N/A"}",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);

            // Update status to Public after approval
            mla.Status = ProfileStatus.Public;
            _mlaRepository.Update(mla);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("MLA profile {MLAId} approved by admin {AdminUserId}", request.MLAId, adminUserId);

            // Map to response
            var assembly = await _assemblyRepository.GetByIdAsync(mla.AssemblyId, cancellationToken);
            return new MLAResponse
            {
                Id = mla.Id,
                UserId = mla.UserId,
                AssemblyId = mla.AssemblyId,
                AssemblyNumber = assembly?.AssemblyNumber ?? string.Empty,
                AssemblyName = assembly?.AssemblyName ?? string.Empty,
                Name = mla.Name,
                Party = mla.Party,
                ProfilePictureUrl = mla.ProfilePictureUrl,
                CoverPhotoUrl = mla.CoverPhotoUrl,
                VisionDescription = mla.VisionDescription,
                TermStartDate = mla.TermStartDate,
                TermEndDate = mla.TermEndDate,
                Status = mla.Status,
                StatusName = mla.Status.ToString(),
                CreatedAt = mla.CreatedAt,
                UpdatedAt = mla.UpdatedAt
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> RejectMLAProfileAsync(RejectMLARequest request, int adminUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            throw new ArgumentException("Rejection reason is mandatory", nameof(request.RejectionReason));
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var mla = await _mlaRepository.GetByIdAsync(request.MLAId, cancellationToken);
            if (mla == null)
            {
                throw new KeyNotFoundException($"MLA profile with ID {request.MLAId} not found");
            }

            if (mla.Status != ProfileStatus.Submitted && mla.Status != ProfileStatus.UnderReview)
            {
                throw new InvalidOperationException($"MLA profile with ID {request.MLAId} is not in pending status. Current status: {mla.Status}");
            }

            // Get old values for audit
            var oldStatus = mla.Status;

            // Update status to Rejected
            mla.Status = ProfileStatus.Rejected;
            mla.RejectionReason = request.RejectionReason;
            mla.UpdatedAt = DateTime.UtcNow;
            _mlaRepository.Update(mla);

            // Create audit log
            var adminUser = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
            var auditLog = new AuditLog
            {
                EntityType = "MLA",
                EntityId = mla.Id,
                Action = "Reject",
                UserId = adminUserId,
                UserName = adminUser?.MobileNumber,
                OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = ProfileStatus.Rejected, RejectionReason = request.RejectionReason }),
                Description = $"MLA profile rejected by admin. Reason: {request.RejectionReason}",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("MLA profile {MLAId} rejected by admin {AdminUserId}", request.MLAId, adminUserId);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(
        string? entityType = null,
        int? entityId = null,
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<AuditLog> auditLogs;

        if (!string.IsNullOrEmpty(entityType) && entityId.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByEntityAsync(entityType, entityId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(entityType))
        {
            auditLogs = await _auditLogRepository.GetByEntityTypeAsync(entityType, cancellationToken);
        }
        else if (userId.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByDateRangeAsync(startDate.Value, endDate.Value, cancellationToken);
        }
        else
        {
            auditLogs = await _auditLogRepository.GetAllAsync(cancellationToken);
        }

        // Apply pagination
        var paginatedLogs = auditLogs
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return paginatedLogs.Select(log => new AuditLogResponse
        {
            Id = log.Id,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Action = log.Action,
            UserId = log.UserId,
            UserName = log.UserName,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Description = log.Description,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        });
    }
}
