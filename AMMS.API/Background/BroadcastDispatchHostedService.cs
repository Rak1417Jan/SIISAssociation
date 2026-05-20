using Microsoft.Extensions.DependencyInjection;
using MVEA.Repository.IRepository;
using System.Threading.Channels;

namespace AMMS.API.Background;

public sealed class BroadcastDispatchHostedService : BackgroundService
{
    private readonly ChannelReader<int> _reader;
    private readonly IServiceProvider _services;
    private readonly ILogger<BroadcastDispatchHostedService> _logger;

    public BroadcastDispatchHostedService(
        ChannelReader<int> reader,
        IServiceProvider services,
        ILogger<BroadcastDispatchHostedService> logger)
    {
        _reader = reader;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (int broadcastId in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = _services.CreateScope();
                IBroadcastRepository repository = scope.ServiceProvider.GetRequiredService<IBroadcastRepository>();
                await repository.ProcessDispatchAsync(broadcastId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Broadcast dispatch failed for {BroadcastId}.", broadcastId);
            }
        }
    }
}
