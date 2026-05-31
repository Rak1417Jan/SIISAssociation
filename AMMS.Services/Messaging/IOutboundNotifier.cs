namespace MVEA.Services.Messaging;

public interface IOutboundNotifier
{
    Task<bool> SendWhatsAppAsync(string recipient, string message, CancellationToken cancellationToken = default);
    Task<bool> SendSmsAsync(string recipient, string message, CancellationToken cancellationToken = default);
}
