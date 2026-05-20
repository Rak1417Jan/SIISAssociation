using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.IRepository;

public interface IMemberNotificationsRepository
{
    Task<ResponseModel<MemberNotificationsResponse>> GetNotificationsAsync(int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> MarkReadAsync(int memberId, IReadOnlyList<int>? notificationIds, CancellationToken cancellationToken = default);
}
