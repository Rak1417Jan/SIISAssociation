using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class ScheduleNotificationRequest
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DeliveryChannel DeliveryChannel { get; set; } = DeliveryChannel.InApp;
    public int? AssemblyId { get; set; }
    public int? BoothId { get; set; }
}
