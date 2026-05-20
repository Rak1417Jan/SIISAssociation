using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class TicketDetailResponse
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int VoterId { get; set; }
    public string VoterName { get; set; } = string.Empty;
    public int AssemblyId { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    public int? MLAId { get; set; }
    public string? MLAName { get; set; }
    public TicketCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int SLAHours { get; set; }
    public int RemainingHours { get; set; }
    public bool IsSLABreached { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolutionProofUrl { get; set; }
    public List<string> AttachmentUrls { get; set; } = new();
    public List<TicketCommentResponse> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TicketCommentResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
