using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IBroadcastService
{
    Task<ResponseModel<PagedResponse<BroadcastListItemResponse>>> GetBroadcastsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateAsync(int clientId, CreateBroadcastRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<BroadcastDetailResponse>> GetDetailAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> DeleteAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> SendAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> ScheduleAsync(int clientId, int broadcastId, DateTime scheduledAt, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> CancelAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<BroadcastStatsResponse>> GetStatsAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default);
}
