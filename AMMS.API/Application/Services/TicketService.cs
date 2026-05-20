using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Ticket service implementation with Unit of Work pattern
/// </summary>
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAttachmentRepository _ticketAttachmentRepository;
    private readonly ITicketCommentRepository _ticketCommentRepository;
    private readonly IVoterRepository? _voterRepository;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IMLARepository _mlaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        IUnitOfWork unitOfWork,
        ITicketRepository ticketRepository,
        ITicketAttachmentRepository ticketAttachmentRepository,
        ITicketCommentRepository ticketCommentRepository,
        IAssemblyRepository assemblyRepository,
        IMLARepository mlaRepository,
        IUserRepository userRepository,
        ILogger<TicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _ticketRepository = ticketRepository;
        _ticketAttachmentRepository = ticketAttachmentRepository;
        _ticketCommentRepository = ticketCommentRepository;
        _assemblyRepository = assemblyRepository;
        _mlaRepository = mlaRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int voterId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get voter details
            var voter = await _voterRepository?.GetByIdAsync(voterId, cancellationToken);
            if (voter == null)
            {
                throw new KeyNotFoundException($"Voter with ID {voterId} not found");
            }

            // Generate ticket number
            var ticketNumber = GenerateTicketNumber(voter.AssemblyId);

            // Create ticket
            var ticket = new Ticket
            {
                VoterId = voterId,
                AssemblyId = voter.AssemblyId,
                TicketNumber = ticketNumber,
                Category = request.Category,
                Title = request.Title,
                Description = request.Description,
                Status = TicketStatus.New,
                SLAHours = 120, // 5 days default
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdTicket = await _ticketRepository.AddAsync(ticket, cancellationToken);

            // Add attachments if provided
            if (request.AttachmentUrls != null && request.AttachmentUrls.Any())
            {
                foreach (var attachmentUrl in request.AttachmentUrls)
                {
                    var attachment = new TicketAttachment
                    {
                        TicketId = createdTicket.Id,
                        FileUrl = attachmentUrl,
                        FileName = System.IO.Path.GetFileName(attachmentUrl),
                        FileType = GetFileType(attachmentUrl),
                        FileSize = 0, // Would be set from actual file upload
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _ticketAttachmentRepository.AddAsync(attachment, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Ticket {TicketNumber} created by voter {VoterId}", ticketNumber, voterId);

            return await MapToTicketResponseAsync(createdTicket, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TicketDetailResponse> GetTicketByIdAsync(int id, int? userId = null, bool isVoter = false, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {id} not found");
        }

        // Access control: Voters can only see their own tickets
        if (isVoter && userId.HasValue && ticket.VoterId != userId.Value)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this ticket");
        }

        // Get related data
        var voter = await _voterRepository?.GetByIdAsync(ticket.VoterId, cancellationToken);
        var assembly = await _assemblyRepository.GetByIdAsync(ticket.AssemblyId, cancellationToken);
        var attachments = await _ticketAttachmentRepository.GetByTicketIdAsync(id, cancellationToken);
        var comments = await _ticketCommentRepository.GetByTicketIdAsync(id, !isVoter, cancellationToken); // Include internal for MLA/Admin

        string? mlaName = null;
        if (ticket.MLAId.HasValue)
        {
            var mla = await _mlaRepository.GetByIdAsync(ticket.MLAId.Value, cancellationToken);
            mlaName = mla?.Name;
        }

        // Calculate remaining SLA hours
        var elapsedHours = (DateTime.UtcNow - ticket.CreatedAt).TotalHours;
        var remainingHours = Math.Max(0, ticket.SLAHours - (int)elapsedHours);
        var isSLABreached = remainingHours == 0 && ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed;

        // Map comments
        var commentResponses = new List<TicketCommentResponse>();
        foreach (var comment in comments)
        {
            string? userName = null;
            if (comment.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(comment.UserId.Value, cancellationToken);
                userName = user?.MobileNumber;
            }

            commentResponses.Add(new TicketCommentResponse
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = userName,
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            });
        }

        return new TicketDetailResponse
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            VoterId = ticket.VoterId,
            VoterName = voter?.Name ?? string.Empty,
            AssemblyId = ticket.AssemblyId,
            AssemblyName = assembly?.AssemblyName ?? string.Empty,
            MLAId = ticket.MLAId,
            MLAName = mlaName,
            Category = ticket.Category,
            CategoryName = ticket.Category.ToString(),
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            StatusName = ticket.Status.ToString(),
            AssignedAt = ticket.AssignedAt,
            ResolvedAt = ticket.ResolvedAt,
            SLAHours = ticket.SLAHours,
            RemainingHours = remainingHours,
            IsSLABreached = isSLABreached,
            ResolutionNote = ticket.ResolutionNote,
            ResolutionProofUrl = ticket.ResolutionProofUrl,
            AttachmentUrls = attachments.Select(a => a.FileUrl).ToList(),
            Comments = commentResponses,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }

    public async Task<IEnumerable<TicketResponse>> GetTicketsByVoterAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetByVoterIdAsync(voterId, cancellationToken);
        var responses = new List<TicketResponse>();

        foreach (var ticket in tickets)
        {
            responses.Add(await MapToTicketResponseAsync(ticket, cancellationToken));
        }

        return responses;
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with ID {id} not found");
            }

            var oldStatus = ticket.Status;

            // Update status
            ticket.Status = request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            // Update status-specific fields
            if (request.Status == TicketStatus.Assigned || request.Status == TicketStatus.InProgress)
            {
                if (!ticket.AssignedAt.HasValue)
                {
                    ticket.AssignedAt = DateTime.UtcNow;
                }
            }

            if (request.Status == TicketStatus.Resolved)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                ticket.ResolutionNote = request.ResolutionNote;
                ticket.ResolutionProofUrl = request.ResolutionProofUrl;
            }

            _ticketRepository.Update(ticket);

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                var comment = new TicketComment
                {
                    TicketId = id,
                    UserId = userId,
                    Comment = request.Comment,
                    IsInternal = false, // Public comment
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _ticketCommentRepository.AddAsync(comment, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Ticket {TicketId} status updated from {OldStatus} to {NewStatus} by user {UserId}", 
                id, oldStatus, request.Status, userId);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TicketReportResponse> GetTicketReportAsync(int? assemblyId = null, int? mlaId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Ticket> tickets;

        if (assemblyId.HasValue)
        {
            tickets = await _ticketRepository.GetByAssemblyIdAsync(assemblyId.Value, cancellationToken);
        }
        else if (mlaId.HasValue)
        {
            // Get tickets for specific MLA
            var allTickets = await _ticketRepository.GetAllAsync(cancellationToken);
            tickets = allTickets.Where(t => t.MLAId == mlaId.Value);
        }
        else
        {
            tickets = await _ticketRepository.GetAllAsync(cancellationToken);
        }

        // Apply date filter
        if (startDate.HasValue || endDate.HasValue)
        {
            tickets = tickets.Where(t =>
                (!startDate.HasValue || t.CreatedAt >= startDate.Value) &&
                (!endDate.HasValue || t.CreatedAt <= endDate.Value));
        }

        var ticketList = tickets.ToList();

        // Calculate statistics
        var totalTickets = ticketList.Count;
        var newTickets = ticketList.Count(t => t.Status == TicketStatus.New);
        var inProgressTickets = ticketList.Count(t => t.Status == TicketStatus.InProgress);
        var resolvedTickets = ticketList.Count(t => t.Status == TicketStatus.Resolved);
        var closedTickets = ticketList.Count(t => t.Status == TicketStatus.Closed);

        // Calculate average resolution time
        var resolvedTicketsWithTime = ticketList
            .Where(t => t.Status == TicketStatus.Resolved && t.ResolvedAt.HasValue && t.CreatedAt != null)
            .ToList();

        double averageResolutionTimeHours = 0;
        if (resolvedTicketsWithTime.Any())
        {
            averageResolutionTimeHours = resolvedTicketsWithTime
                .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
        }

        // Calculate resolution rate
        var resolutionRate = totalTickets > 0 
            ? ((double)(resolvedTickets + closedTickets) / totalTickets) * 100 
            : 0;

        // Category statistics
        var categoryStats = ticketList
            .GroupBy(t => t.Category)
            .Select(g => new TicketCategoryStats
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                Count = g.Count(),
                AverageResolutionTimeHours = g.Where(t => t.ResolvedAt.HasValue)
                    .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        // TODO: Booth statistics (would need booth information from tickets)
        var boothStats = new List<TicketBoothStats>();

        return new TicketReportResponse
        {
            TotalTickets = totalTickets,
            NewTickets = newTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            ClosedTickets = closedTickets,
            AverageResolutionTimeHours = averageResolutionTimeHours,
            ResolutionRate = resolutionRate,
            CategoryStats = categoryStats,
            BoothStats = boothStats,
            ReportStartDate = startDate,
            ReportEndDate = endDate
        };
    }

    private async Task<TicketResponse> MapToTicketResponseAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        var elapsedHours = (DateTime.UtcNow - ticket.CreatedAt).TotalHours;
        var remainingHours = Math.Max(0, ticket.SLAHours - (int)elapsedHours);

        return new TicketResponse
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Category = ticket.Category,
            CategoryName = ticket.Category.ToString(),
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            StatusName = ticket.Status.ToString(),
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            ResolutionNote = ticket.ResolutionNote,
            SLAHours = ticket.SLAHours,
            RemainingHours = remainingHours,
            AttachmentUrls = new List<string>() // Would be populated from attachments if needed
        };
    }

    private string GenerateTicketNumber(int assemblyId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"TKT-{assemblyId}-{timestamp}-{random}";
    }

    private string GetFileType(string fileUrl)
    {
        var extension = System.IO.Path.GetExtension(fileUrl).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" => "image",
            ".mp4" or ".avi" or ".mov" => "video",
            _ => "unknown"
        };
    }
}
