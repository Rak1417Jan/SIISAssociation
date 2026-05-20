namespace MVEA.Model.DTOs.Response;

public class BroadcastListItemResponse
{
    public int BroadcastId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public DateTime? SentAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int RecipientCount { get; init; }
    public int DeliveredCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime CreatedDate { get; init; }
}
