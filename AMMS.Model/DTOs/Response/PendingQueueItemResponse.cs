namespace MVEA.Model.DTOs.Response;

public class PendingQueueItemResponse
{
    public int ApplicationId { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsOnHoldOver7Days { get; init; }
}

