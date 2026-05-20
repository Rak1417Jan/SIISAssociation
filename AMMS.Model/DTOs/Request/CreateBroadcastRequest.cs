namespace MVEA.Model.DTOs.Request;

public sealed class CreateBroadcastRequest
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string? TargetFilterJson { get; init; }
    public DateTime? ScheduledAt { get; init; }
}
