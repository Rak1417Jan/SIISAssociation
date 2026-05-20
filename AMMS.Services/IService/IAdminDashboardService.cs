using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IAdminDashboardService
{
    Task<ResponseModel<AdminDashboardResponse>> GetDashboardAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<AdminAnalyticsResponse>> GetAnalyticsAsync(int clientId, int? year, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<PendingQueueItemResponse>>> GetPendingQueueAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
}

