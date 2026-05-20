using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class TicketResponse
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
    public int SLAHours { get; set; }
    public int RemainingHours { get; set; }
    public List<string> AttachmentUrls { get; set; } = new();
}
