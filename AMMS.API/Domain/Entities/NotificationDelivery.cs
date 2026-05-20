namespace MVEA.Domain.Entities;

/// <summary>
/// Notification delivery log
/// </summary>
public class NotificationDelivery : BaseEntity
{
    public int NotificationId { get; set; }
    public int? VoterId { get; set; }
    public string RecipientMobile { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? DeliveredMessage { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryError { get; set; }
    public string? ExternalMessageId { get; set; }
}
