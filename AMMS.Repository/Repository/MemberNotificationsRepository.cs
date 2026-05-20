using Dapper;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;
using System.Linq;

namespace MVEA.Repository.Repository;

public sealed class MemberNotificationsRepository : IMemberNotificationsRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<MemberNotificationsRepository> _logger;

    public MemberNotificationsRepository(ISqlConnectionFactory connectionFactory, ILogger<MemberNotificationsRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<ResponseModel<MemberNotificationsResponse>> GetNotificationsAsync(int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Member_GetNotifications",
                new { MemberId = memberId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(cmd);
            List<MemberNotificationItemResponse> items = (await multi.ReadAsync<MemberNotificationItemResponse>()).ToList();
            int unread = await multi.ReadSingleAsync<int>();

            return new ResponseModel<MemberNotificationsResponse>
            {
                Data = new MemberNotificationsResponse
                {
                    Notifications = items,
                    UnreadCount = unread
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNotificationsAsync failed.");
            return new ResponseModel<MemberNotificationsResponse> { ErrorMessage = "Unable to load notifications.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> MarkReadAsync(int memberId, IReadOnlyList<int>? notificationIds, CancellationToken cancellationToken = default)
    {
        try
        {
            string? idsCsv = null;
            if (notificationIds != null && notificationIds.Count > 0)
            {
                idsCsv = string.Join(",", notificationIds);
            }

            IDbConnection connection = _connectionFactory.GetConnection();
            CommandDefinition cmd = new CommandDefinition(
                "usp_Member_MarkNotificationsRead",
                new { MemberId = memberId, NotificationIds = idsCsv },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MarkReadAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to mark notifications read.", ErrorId = -1 };
        }
    }
}
