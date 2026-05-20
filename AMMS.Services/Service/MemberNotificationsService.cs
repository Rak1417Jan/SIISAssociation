using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class MemberNotificationsService : IMemberNotificationsService
{
    private readonly IMemberNotificationsRepository _repository;

    public MemberNotificationsService(IMemberNotificationsRepository repository)
    {
        _repository = repository;
    }

    public Task<ResponseModel<MemberNotificationsResponse>> GetAsync(int memberId, CancellationToken cancellationToken = default)
    {
        return _repository.GetNotificationsAsync(memberId, cancellationToken);
    }

    public Task<ResponseModel<bool>> MarkReadAsync(int memberId, MarkNotificationsReadRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.MarkReadAsync(memberId, request.NotificationIds, cancellationToken);
    }
}
