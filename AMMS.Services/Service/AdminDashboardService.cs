using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository, ILogger<AdminDashboardService> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public Task<ResponseModel<AdminDashboardResponse>> GetDashboardAsync(int clientId, CancellationToken cancellationToken = default)
    {
        return _adminDashboardRepository.GetDashboardAsync(clientId, cancellationToken);
    }

    public Task<ResponseModel<AdminAnalyticsResponse>> GetAnalyticsAsync(int clientId, int? year, CancellationToken cancellationToken = default)
    {
        if (year is < 2000 or > 2100)
        {
            return Task.FromResult(new ResponseModel<AdminAnalyticsResponse> { ErrorMessage = "Invalid year.", ErrorId = -1 });
        }

        return _adminDashboardRepository.GetAnalyticsAsync(clientId, year, cancellationToken);
    }

    public Task<ResponseModel<PagedResponse<PendingQueueItemResponse>>> GetPendingQueueAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        return _adminDashboardRepository.GetPendingQueueAsync(clientId, page, pageSize, cancellationToken);
    }
}

