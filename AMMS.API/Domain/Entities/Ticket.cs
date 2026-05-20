using MVEA.Domain.Enums;

namespace MVEA.Domain.Entities;

/// <summary>
/// Ticket/Grievance entity
/// </summary>
public class Ticket : BaseEntity
{
    public int VoterId { get; set; }
    public int AssemblyId { get; set; }
    public int? MLAId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int SLAHours { get; set; } = 120; // 5 days default
    public string? ResolutionNote { get; set; }
    public string? ResolutionProofUrl { get; set; }
}
