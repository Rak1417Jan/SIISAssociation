namespace MVEA.Domain.Entities;

/// <summary>
/// Attachment for ticket (photos/videos)
/// </summary>
public class TicketAttachment : BaseEntity
{
    public int TicketId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
