using Dapper;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<AdminDashboardRepository> _logger;

    public AdminDashboardRepository(ISqlConnectionFactory connection, ILogger<AdminDashboardRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ResponseModel<AdminDashboardResponse>> GetDashboardAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_GetDashboard",
                new { ClientId = clientId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(cmd);

            var header = await multi.ReadFirstOrDefaultAsync<AdminDashboardResponse>();
            var points = (await multi.ReadAsync<DailyRegistrationPoint>()).ToList();

            if (header == null)
            {
                return new ResponseModel<AdminDashboardResponse> { ErrorMessage = "Dashboard data not found.", ErrorId = -1 };
            }

            return new ResponseModel<AdminDashboardResponse>
            {
                Data = new AdminDashboardResponse
                {
                    TotalMembers = header.TotalMembers,
                    ActiveMembers = header.ActiveMembers,
                    InactiveMembers = header.InactiveMembers,
                    PendingApplications = header.PendingApplications,
                    OnHoldApplications = header.OnHoldApplications,
                    RejectedApplications = header.RejectedApplications,
                    CurrentYearRevenue = header.CurrentYearRevenue,
                    Last7DaysRegistrations = points
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching admin dashboard.");
            return new ResponseModel<AdminDashboardResponse> { ErrorMessage = "Unable to fetch dashboard.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<AdminAnalyticsResponse>> GetAnalyticsAsync(int clientId, int? year, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_GetAnalytics",
                new { ClientId = clientId, Year = year },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(cmd);

            var growth = (await multi.ReadAsync<MembershipGrowthPoint>()).ToList();
            var revenue = (await multi.ReadAsync<MonthlyRevenuePoint>()).ToList();
            var plans = (await multi.ReadAsync<PlanBreakdownPoint>()).ToList();
            var comparison = await multi.ReadFirstOrDefaultAsync<YearComparison>() ?? new YearComparison();

            return new ResponseModel<AdminAnalyticsResponse>
            {
                Data = new AdminAnalyticsResponse
                {
                    MembershipGrowth = growth,
                    MonthlyRevenue = revenue,
                    PlanBreakdown = plans,
                    YearComparison = comparison
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching admin analytics.");
            return new ResponseModel<AdminAnalyticsResponse> { ErrorMessage = "Unable to fetch analytics.", ErrorId = -1 };
        }
    }

    private sealed class PendingQueueRow : PendingQueueItemResponse
    {
        public int Total { get; init; }
    }

    public async Task<ResponseModel<PagedResponse<PendingQueueItemResponse>>> GetPendingQueueAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_GetPendingQueue",
                new { ClientId = clientId, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = (await connection.QueryAsync<PendingQueueRow>(cmd)).ToList();

            var total = rows.FirstOrDefault()?.Total ?? 0;
            var records = rows.Select(r => new PendingQueueItemResponse
            {
                ApplicationId = r.ApplicationId,
                OwnerName = r.OwnerName,
                MobileNumber = r.MobileNumber,
                CreatedDate = r.CreatedDate,
                Status = r.Status,
                IsOnHoldOver7Days = r.IsOnHoldOver7Days
            }).ToList();

            return new ResponseModel<PagedResponse<PendingQueueItemResponse>>
            {
                Data = new PagedResponse<PendingQueueItemResponse>
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    Records = records
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching admin pending queue.");
            return new ResponseModel<PagedResponse<PendingQueueItemResponse>> { ErrorMessage = "Unable to fetch pending queue.", ErrorId = -1 };
        }
    }
}

