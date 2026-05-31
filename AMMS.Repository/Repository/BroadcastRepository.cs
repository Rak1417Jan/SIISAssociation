using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;
using System.Linq;

namespace MVEA.Repository.Repository;

public sealed class BroadcastRepository : IBroadcastRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<BroadcastRepository> _logger;

    public BroadcastRepository(ISqlConnectionFactory connectionFactory, ILogger<BroadcastRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private sealed class BroadcastListRow : BroadcastListItemResponse
    {
        public int Total { get; init; }
    }

    public async Task<ResponseModel<PagedResponse<BroadcastListItemResponse>>> GetBroadcastsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Admin_GetBroadcasts",
                new { ClientId = clientId, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            List<BroadcastListRow> rows = (await connection.QueryAsync<BroadcastListRow>(cmd)).ToList();
            int total = rows.FirstOrDefault()?.Total ?? 0;
            List<BroadcastListItemResponse> records = rows.Select(r => new BroadcastListItemResponse
            {
                BroadcastId = r.BroadcastId,
                Title = r.Title,
                Channel = r.Channel,
                SentAt = r.SentAt,
                ScheduledAt = r.ScheduledAt,
                RecipientCount = r.RecipientCount,
                DeliveredCount = r.DeliveredCount,
                FailedCount = r.FailedCount,
                CreatedDate = r.CreatedDate
            }).ToList();

            return new ResponseModel<PagedResponse<BroadcastListItemResponse>>
            {
                Data = new PagedResponse<BroadcastListItemResponse>
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
            _logger.LogError(ex, "GetBroadcastsAsync failed.");
            return new ResponseModel<PagedResponse<BroadcastListItemResponse>> { ErrorMessage = "Unable to load broadcasts.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> CreateAsync(int clientId, CreateBroadcastRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Broadcast_Create",
                new
                {
                    ClientId = clientId,
                    Title = request.Title,
                    Message = request.Message,
                    Channel = request.Channel,
                    TargetFilter = request.TargetFilterJson,
                    ScheduledAt = request.ScheduledAt,
                    CreatedBy = createdBy
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            int newId = await connection.QuerySingleAsync<int>(cmd);
            return new ResponseModel<int> { Data = newId };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<int> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync broadcast failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to create broadcast.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<BroadcastDetailResponse>> GetDetailAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Broadcast_GetDetail",
                new { ClientId = clientId, BroadcastId = broadcastId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            BroadcastDetailResponse? row = await connection.QueryFirstOrDefaultAsync<BroadcastDetailResponse>(cmd);
            return row == null
                ? new ResponseModel<BroadcastDetailResponse> { ErrorMessage = "Broadcast not found.", ErrorId = -1 }
                : new ResponseModel<BroadcastDetailResponse> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDetailAsync broadcast failed.");
            return new ResponseModel<BroadcastDetailResponse> { ErrorMessage = "Unable to load broadcast.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> SoftDeleteAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Broadcast_Delete",
                new { ClientId = clientId, BroadcastId = broadcastId, ModifiedBy = modifiedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            int affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SoftDeleteAsync broadcast failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to delete broadcast.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> ProcessDispatchAsync(int broadcastId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Broadcast_ProcessDispatch",
                new { BroadcastId = broadcastId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessDispatchAsync failed for {BroadcastId}.", broadcastId);
            return new ResponseModel<bool> { ErrorMessage = "Dispatch failed.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> ScheduleAsync(int clientId, int broadcastId, DateTime scheduledAt, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.BROADCASTS
                SET SCHEDULED_AT = @ScheduledAt, MODIFIED_DATE = SYSUTCDATETIME()
                WHERE BROADCAST_ID = @BroadcastId AND CLIENT_ID = @ClientId AND ISNULL(IS_DELETED, 0) = 0 AND SENT_AT IS NULL";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { BroadcastId = broadcastId, ClientId = clientId, ScheduledAt = scheduledAt }, cancellationToken: cancellationToken));

            return affected > 0
                ? new ResponseModel<bool> { Data = true }
                : new ResponseModel<bool> { ErrorMessage = "Broadcast not found or already sent.", ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScheduleAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to schedule broadcast.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> CancelAsync(int clientId, int broadcastId, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.BROADCASTS
                SET IS_DELETED = 1, MODIFIED_DATE = SYSUTCDATETIME(), MODIFIED_BY = @ModifiedBy
                WHERE BROADCAST_ID = @BroadcastId AND CLIENT_ID = @ClientId AND SENT_AT IS NULL";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { BroadcastId = broadcastId, ClientId = clientId, ModifiedBy = modifiedBy }, cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to cancel broadcast.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<BroadcastStatsResponse>> GetStatsAsync(int clientId, int broadcastId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT BROADCAST_ID AS BroadcastId, ISNULL(RECIPIENT_COUNT, 0) AS RecipientCount,
                       ISNULL(DELIVERED_COUNT, 0) AS DeliveredCount, ISNULL(FAILED_COUNT, 0) AS FailedCount,
                       SENT_AT AS SentAt, SCHEDULED_AT AS ScheduledAt
                FROM dbo.BROADCASTS
                WHERE BROADCAST_ID = @BroadcastId AND CLIENT_ID = @ClientId AND ISNULL(IS_DELETED, 0) = 0";

            BroadcastStatsResponse? row = await connection.QueryFirstOrDefaultAsync<BroadcastStatsResponse>(
                new CommandDefinition(sql, new { BroadcastId = broadcastId, ClientId = clientId }, cancellationToken: cancellationToken));

            return row == null
                ? new ResponseModel<BroadcastStatsResponse> { ErrorMessage = "Broadcast not found.", ErrorId = -1 }
                : new ResponseModel<BroadcastStatsResponse> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStatsAsync failed.");
            return new ResponseModel<BroadcastStatsResponse> { ErrorMessage = "Unable to load broadcast stats.", ErrorId = -1 };
        }
    }
}
