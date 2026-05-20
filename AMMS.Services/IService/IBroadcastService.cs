using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IBroadcastService
{
    Task<ResponseModel<PagedResponse<BroadcastListItemResponse>>> GetBroadcastsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateAsync(int clientId, CreateBroadcastRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<BroadcastDetailResponse>> GetDetailAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> DeleteAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default);
}
