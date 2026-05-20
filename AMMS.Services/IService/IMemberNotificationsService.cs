using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IMemberNotificationsService
{
    Task<ResponseModel<MemberNotificationsResponse>> GetAsync(int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> MarkReadAsync(int memberId, MarkNotificationsReadRequest request, CancellationToken cancellationToken = default);
}
