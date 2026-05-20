using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class NotificationDeliveryLogResponse
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public string NotificationTitle { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public string NotificationTypeName { get; set; } = string.Empty;
    public int? VoterId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientMobile { get; set; } = string.Empty;
    public string? DeliveredMessage { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryError { get; set; }
    public string? ExternalMessageId { get; set; }
    public string DeliveryChannel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
