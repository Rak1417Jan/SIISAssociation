namespace MVEA.Domain.Entities;

/// <summary>
/// Comment on ticket for status updates
/// </summary>
public class TicketComment : BaseEntity
{
    public int TicketId { get; set; }
    public int? UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
