using MVEA.Services.IService;
using System.Threading.Channels;

namespace AMMS.API.Background;

public sealed class BroadcastDispatchQueue : IBroadcastDispatchQueue
{
    private readonly ChannelWriter<int> _writer;

    public BroadcastDispatchQueue(ChannelWriter<int> writer)
    {
        _writer = writer;
    }

    public ValueTask EnqueueAsync(int broadcastId, CancellationToken cancellationToken = default)
    {
        return _writer.WriteAsync(broadcastId, cancellationToken);
    }
}
