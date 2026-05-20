using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MVEA.Services.Messaging;

public interface IStaffPasswordResetNotifier
{
    Task NotifyResetIssuedAsync(string email, string plaintextToken, int clientId, CancellationToken cancellationToken);
}

/// <summary>
/// Development-oriented notifier: logs the reset token. Replace with SMTP or a queue in production.
/// </summary>
public sealed class LoggingStaffPasswordResetNotifier : IStaffPasswordResetNotifier
{
    private readonly ILogger<LoggingStaffPasswordResetNotifier> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public LoggingStaffPasswordResetNotifier(
        ILogger<LoggingStaffPasswordResetNotifier> logger,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public Task NotifyResetIssuedAsync(string email, string plaintextToken, int clientId, CancellationToken cancellationToken)
    {
        string? baseUrl = _configuration["App:PasswordResetPublicUrl"]?.TrimEnd('/');
        if (_hostEnvironment.IsDevelopment())
        {
            _logger.LogInformation(
                "Staff password reset for {Email} (client {ClientId}). Base URL: {BaseUrl}. Token (dev): {Token}",
                email,
                clientId,
                string.IsNullOrEmpty(baseUrl) ? "(not configured)" : baseUrl,
                plaintextToken);
        }
        else
        {
            _logger.LogInformation(
                "Staff password reset flow completed for {Email} (client {ClientId}). Deliver token via your email provider.",
                email,
                clientId);
        }

        return Task.CompletedTask;
    }
}
