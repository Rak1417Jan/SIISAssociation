using MVEA.Domain.Enums;

namespace MVEA.Domain.Entities;

/// <summary>
/// Notification entity for scheduled messages
/// </summary>
public class Notification : BaseEntity
{
    public int MLAId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DeliveryChannel DeliveryChannel { get; set; } = DeliveryChannel.InApp;
}
