using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// MLA service implementation with Unit of Work pattern
/// </summary>
public class MLAService : IMLAService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMLARepository _mlaRepository;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly ILogger<MLAService> _logger;

    public MLAService(
        IUnitOfWork unitOfWork,
        IMLARepository mlaRepository,
        IAssemblyRepository assemblyRepository,
        ILogger<MLAService> logger)
    {
        _unitOfWork = unitOfWork;
        _mlaRepository = mlaRepository;
        _assemblyRepository = assemblyRepository;
        _logger = logger;
    }

    public async Task<MLAResponse> CreateOrSubmitMLAProfileAsync(CreateMLAProfileRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if assembly exists
            var assembly = await _assemblyRepository.GetByIdAsync(request.AssemblyId, cancellationToken);
            if (assembly == null)
            {
                throw new KeyNotFoundException($"Assembly with ID {request.AssemblyId} not found");
            }

            // Check if MLA profile already exists for this user
            var existingMLA = await _mlaRepository.GetByUserIdAsync(userId, cancellationToken);

            if (existingMLA != null)
            {
                // Update existing profile and submit for approval
                existingMLA.AssemblyId = request.AssemblyId;
                existingMLA.Name = request.Name;
                existingMLA.Party = request.Party;
                existingMLA.ProfilePictureUrl = request.ProfilePictureUrl;
                existingMLA.CoverPhotoUrl = request.CoverPhotoUrl;
                existingMLA.VisionDescription = request.VisionDescription;
                existingMLA.TermStartDate = request.TermStartDate;
                existingMLA.TermEndDate = request.TermEndDate;

                // If status is Draft, change to Submitted for approval
                if (existingMLA.Status == ProfileStatus.Draft)
                {
                    existingMLA.Status = ProfileStatus.Submitted;
                }

                existingMLA.UpdatedAt = DateTime.UtcNow;
                _mlaRepository.Update(existingMLA);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return await MapToMLAResponseAsync(existingMLA, cancellationToken);
            }
            else
            {
                // Check if another MLA already exists for this assembly (prevent duplicates)
                var existingAssemblyMLA = await _mlaRepository.ExistsByAssemblyIdAsync(request.AssemblyId, null, cancellationToken);
                if (existingAssemblyMLA)
                {
                    throw new InvalidOperationException($"An MLA profile already exists for Assembly ID {request.AssemblyId}");
                }

                // Create new MLA profile
                var newMLA = new MLA
                {
                    UserId = userId,
                    AssemblyId = request.AssemblyId,
                    Name = request.Name,
                    Party = request.Party,
                    ProfilePictureUrl = request.ProfilePictureUrl,
                    CoverPhotoUrl = request.CoverPhotoUrl,
                    VisionDescription = request.VisionDescription,
                    TermStartDate = request.TermStartDate,
                    TermEndDate = request.TermEndDate,
                    Status = ProfileStatus.Submitted, // Submitted for approval
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var createdMLA = await _mlaRepository.AddAsync(newMLA, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("MLA profile created for user {UserId} with Assembly ID {AssemblyId}", userId, request.AssemblyId);

                return await MapToMLAResponseAsync(createdMLA, cancellationToken);
            }
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<MLAResponse> GetMLAProfileAsync(int userId, bool includePrivateDetails = false, CancellationToken cancellationToken = default)
    {
        var mla = await _mlaRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mla == null)
        {
            throw new KeyNotFoundException($"MLA profile not found for user ID {userId}");
        }

        // If requesting private details, check if user has permission
        // For now, we'll return all details. In production, check user role/permissions

        return await MapToMLAResponseAsync(mla, cancellationToken);
    }

    public async Task<MLAResponse> UpdateMLAProfileAsync(int userId, UpdateMLAProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var mla = await _mlaRepository.GetByUserIdAsync(userId, cancellationToken);
            if (mla == null)
            {
                throw new KeyNotFoundException($"MLA profile not found for user ID {userId}");
            }

            // Can only update if status is Draft or Submitted (not if already approved/public)
            if (mla.Status == ProfileStatus.Approved || mla.Status == ProfileStatus.Public)
            {
                // If already approved, need to resubmit for approval for changes
                if (!string.IsNullOrEmpty(request.Name)) mla.Name = request.Name;
                if (request.Party != null) mla.Party = request.Party;
                if (request.ProfilePictureUrl != null) mla.ProfilePictureUrl = request.ProfilePictureUrl;
                if (request.CoverPhotoUrl != null) mla.CoverPhotoUrl = request.CoverPhotoUrl;
                if (request.VisionDescription != null) mla.VisionDescription = request.VisionDescription;
                if (request.TermStartDate.HasValue) mla.TermStartDate = request.TermStartDate;
                if (request.TermEndDate.HasValue) mla.TermEndDate = request.TermEndDate;

                // Resubmit for approval if already approved/public
                if (mla.Status == ProfileStatus.Public || mla.Status == ProfileStatus.Approved)
                {
                    mla.Status = ProfileStatus.Submitted;
                }
            }
            else
            {
                // Update fields
                if (!string.IsNullOrEmpty(request.Name)) mla.Name = request.Name;
                if (request.Party != null) mla.Party = request.Party;
                if (request.ProfilePictureUrl != null) mla.ProfilePictureUrl = request.ProfilePictureUrl;
                if (request.CoverPhotoUrl != null) mla.CoverPhotoUrl = request.CoverPhotoUrl;
                if (request.VisionDescription != null) mla.VisionDescription = request.VisionDescription;
                if (request.TermStartDate.HasValue) mla.TermStartDate = request.TermStartDate;
                if (request.TermEndDate.HasValue) mla.TermEndDate = request.TermEndDate;
            }

            mla.UpdatedAt = DateTime.UtcNow;
            _mlaRepository.Update(mla);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return await MapToMLAResponseAsync(mla, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<MLAResponse> MapToMLAResponseAsync(MLA mla, CancellationToken cancellationToken)
    {
        // Get assembly details
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
            RejectionReason = mla.RejectionReason,
            CreatedAt = mla.CreatedAt,
            UpdatedAt = mla.UpdatedAt
        };
    }
}
