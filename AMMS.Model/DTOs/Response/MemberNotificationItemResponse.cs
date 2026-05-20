namespace MVEA.Model.DTOs.Response;

public sealed class MemberNotificationItemResponse
{
    public int NotificationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? LinkTo { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
