using Microsoft.Extensions.Logging;

namespace MVEA.Services.Messaging;

public sealed class LoggingOutboundNotifier : IOutboundNotifier
{
    private readonly ILogger<LoggingOutboundNotifier> _logger;

    public LoggingOutboundNotifier(ILogger<LoggingOutboundNotifier> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendWhatsAppAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp stub to {Recipient}: {Message}", recipient, message);
        return Task.FromResult(true);
    }

    public Task<bool> SendSmsAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SMS stub to {Recipient}: {Message}", recipient, message);
        return Task.FromResult(true);
    }
}
