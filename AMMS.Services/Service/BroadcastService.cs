using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class BroadcastService : IBroadcastService
{
    private readonly IBroadcastRepository _broadcastRepository;
    private readonly IBroadcastDispatchQueue _dispatchQueue;

    public BroadcastService(IBroadcastRepository broadcastRepository, IBroadcastDispatchQueue dispatchQueue)
    {
        _broadcastRepository = broadcastRepository;
        _dispatchQueue = dispatchQueue;
    }

    public Task<ResponseModel<PagedResponse<BroadcastListItemResponse>>> GetBroadcastsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        return _broadcastRepository.GetBroadcastsAsync(clientId, page, pageSize, cancellationToken);
    }

    public async Task<ResponseModel<int>> CreateAsync(int clientId, CreateBroadcastRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.Channel))
        {
            return new ResponseModel<int> { ErrorMessage = "title, message, and channel are required.", ErrorId = -1 };
        }

        ResponseModel<int> created = await _broadcastRepository.CreateAsync(clientId, request, createdBy, cancellationToken);
        if (!created.Success)
        {
            return created;
        }

        int broadcastId = created.Data;
        bool dispatchNow = request.ScheduledAt == null || request.ScheduledAt <= DateTime.UtcNow;
        if (dispatchNow && broadcastId > 0)
        {
            await _dispatchQueue.EnqueueAsync(broadcastId, cancellationToken);
        }

        return created;
    }

    public Task<ResponseModel<BroadcastDetailResponse>> GetDetailAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default)
    {
        return _broadcastRepository.GetDetailAsync(clientId, broadcastId, cancellationToken);
    }

    public Task<ResponseModel<bool>> DeleteAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default)
    {
        return _broadcastRepository.SoftDeleteAsync(clientId, broadcastId, modifiedBy, cancellationToken);
    }

    public async Task<ResponseModel<bool>> SendAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default)
    {
        ResponseModel<BroadcastDetailResponse> detail = await _broadcastRepository.GetDetailAsync(clientId, broadcastId, cancellationToken);
        if (!detail.Success)
        {
            return new ResponseModel<bool> { ErrorMessage = detail.ErrorMessage, ErrorId = detail.ErrorId };
        }

        await _dispatchQueue.EnqueueAsync(broadcastId, cancellationToken);
        return new ResponseModel<bool> { Data = true };
    }

    public Task<ResponseModel<bool>> ScheduleAsync(int clientId, int broadcastId, DateTime scheduledAt, CancellationToken cancellationToken = default)
    {
        if (scheduledAt <= DateTime.UtcNow)
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "Scheduled time must be in the future.", ErrorId = -1 });
        }

        return _broadcastRepository.ScheduleAsync(clientId, broadcastId, scheduledAt, cancellationToken);
    }

    public Task<ResponseModel<bool>> CancelAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default)
        => _broadcastRepository.CancelAsync(clientId, broadcastId, modifiedBy, cancellationToken);

    public Task<ResponseModel<BroadcastStatsResponse>> GetStatsAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default)
        => _broadcastRepository.GetStatsAsync(clientId, broadcastId, cancellationToken);
}
