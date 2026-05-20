using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<PendingMLAResponse>> GetPendingMLAProfilesAsync(CancellationToken cancellationToken = default);
    Task<MLAResponse> ApproveMLAProfileAsync(ApproveMLARequest request, int adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RejectMLAProfileAsync(RejectMLARequest request, int adminUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(
        string? entityType = null,
        int? entityId = null,
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
