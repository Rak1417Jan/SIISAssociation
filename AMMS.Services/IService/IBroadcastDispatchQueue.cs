namespace MVEA.Services.IService;

public interface IBroadcastDispatchQueue
{
    ValueTask EnqueueAsync(int broadcastId, CancellationToken cancellationToken = default);
}
