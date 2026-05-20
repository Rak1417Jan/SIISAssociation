namespace MVEA.Model.DTOs.Response;

public sealed class BroadcastDetailResponse
{
    public int BroadcastId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string? TargetFilter { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public DateTime? SentAt { get; init; }
    public int? CreatedBy { get; init; }
    public int RecipientCount { get; init; }
    public int DeliveredCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime CreatedDate { get; init; }
}
