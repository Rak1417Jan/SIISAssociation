using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class ScheduledNotificationResponse
{
    public int Id { get; set; }
    public int MLAId { get; set; }
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DeliveryChannel DeliveryChannel { get; set; }
    public string DeliveryChannelName { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
