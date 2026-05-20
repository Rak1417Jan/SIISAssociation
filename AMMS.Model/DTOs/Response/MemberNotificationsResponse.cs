namespace MVEA.Model.DTOs.Response;

public sealed class MemberNotificationsResponse
{
    public int UnreadCount { get; init; }
    public IReadOnlyList<MemberNotificationItemResponse> Notifications { get; init; } = Array.Empty<MemberNotificationItemResponse>();
}
