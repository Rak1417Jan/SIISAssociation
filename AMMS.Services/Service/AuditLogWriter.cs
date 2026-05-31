using MVEA.Model.DTOs.Platform;
using MVEA.Repository.IRepository;

namespace MVEA.Services.Service;

public sealed class AuditLogWriter
{
    private readonly IPlatformRepository _platformRepository;

    public AuditLogWriter(IPlatformRepository platformRepository)
    {
        _platformRepository = platformRepository;
    }

    public Task WriteAsync(WriteAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        return _platformRepository.WriteAuditLogAsync(request, cancellationToken);
    }
}
