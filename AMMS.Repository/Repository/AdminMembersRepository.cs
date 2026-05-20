using Dapper;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class AdminMembersRepository : IAdminMembersRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<AdminMembersRepository> _logger;

    public AdminMembersRepository(ISqlConnectionFactory connection, ILogger<AdminMembersRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    private sealed class MemberListRow : AdminMemberListItemResponse
    {
        public int Total { get; init; }
    }

    public async Task<ResponseModel<PagedResponse<AdminMemberListItemResponse>>> GetMembersAsync(
        int clientId,
        int page,
        int pageSize,
        string? search,
        int? firmId,
        int? planId,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_GetMembers",
                new
                {
                    ClientId = clientId,
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    FirmId = firmId,
                    PlanId = planId,
                    Status = status,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = (await connection.QueryAsync<MemberListRow>(cmd)).ToList();
            var total = rows.FirstOrDefault()?.Total ?? 0;

            var records = rows.Select(r => new AdminMemberListItemResponse
            {
                MemberId = r.MemberId,
                MembershipId = r.MembershipId,
                OwnerName = r.OwnerName,
                MobileNumber = r.MobileNumber,
                Email = r.Email,
                CompanyId = r.CompanyId,
                CompanyName = r.CompanyName,
                IsActive = r.IsActive,
                CreatedDate = r.CreatedDate
            }).ToList();

            return new ResponseModel<PagedResponse<AdminMemberListItemResponse>>
            {
                Data = new PagedResponse<AdminMemberListItemResponse>
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
            _logger.LogError(ex, "Error while fetching members list.");
            return new ResponseModel<PagedResponse<AdminMemberListItemResponse>> { ErrorMessage = "Unable to fetch members.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<AdminMemberDetailResponse>> GetMemberDetailAsync(int clientId, int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_GetMemberDetail",
                new { ClientId = clientId, MemberId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var row = await connection.QueryFirstOrDefaultAsync<AdminMemberDetailResponse>(cmd);
            if (row == null)
            {
                return new ResponseModel<AdminMemberDetailResponse> { ErrorMessage = "Member not found.", ErrorId = -1 };
            }

            return new ResponseModel<AdminMemberDetailResponse> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching member detail.");
            return new ResponseModel<AdminMemberDetailResponse> { ErrorMessage = "Unable to fetch member detail.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> VerifyMemberAsync(int clientId, int id, string notes, int changedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_VerifyMember",
                new { ClientId = clientId, MemberId = id, Notes = notes, ChangedBy = changedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while verifying member.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to verify member.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> HoldMemberAsync(int clientId, int id, string reason, int changedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_HoldMember",
                new { ClientId = clientId, MemberId = id, Reason = reason, ChangedBy = changedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while holding member.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to hold member.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> RejectMemberAsync(int clientId, int id, string feedback, int changedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();

            var cmd = new CommandDefinition(
                "usp_Admin_RejectMember",
                new { ClientId = clientId, MemberId = id, Feedback = feedback, ChangedBy = changedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while rejecting member.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to reject member.", ErrorId = -1 };
        }
    }
}

