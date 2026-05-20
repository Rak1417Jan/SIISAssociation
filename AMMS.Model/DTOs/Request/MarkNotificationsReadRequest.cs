namespace MVEA.Model.DTOs.Request;

public sealed class MarkNotificationsReadRequest
{
    public IReadOnlyList<int>? NotificationIds { get; init; }
}
