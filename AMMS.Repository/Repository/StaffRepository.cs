using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;
using System.Text.Json;

namespace MVEA.Repository.Repository;

public sealed class StaffRepository : IStaffRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<StaffRepository> _logger;

    public StaffRepository(ISqlConnectionFactory connection, ILogger<StaffRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ResponseModel<IReadOnlyList<StaffListItemResponse>>> GetStaffAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition("usp_Admin_GetStaff", new { ClientId = clientId }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            var rows = (await connection.QueryAsync<StaffListItemResponse>(cmd)).ToList();
            return new ResponseModel<IReadOnlyList<StaffListItemResponse>> { Data = rows };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching staff.");
            return new ResponseModel<IReadOnlyList<StaffListItemResponse>> { ErrorMessage = "Unable to fetch staff.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> CreateStaffAsync(int clientId, CreateStaffRequest request, byte[] passwordHash, byte[] passwordSalt, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_CreateStaff",
                new
                {
                    ClientId = clientId,
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    RoleIdsJson = JsonSerializer.Serialize(request.RoleIds ?? Array.Empty<int>()),
                    FullName = request.FullName,
                    MobileNo = request.MobileNo,
                    CreatedBy = createdBy
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var newId = await connection.QuerySingleAsync<int>(cmd);
            return new ResponseModel<int> { Data = newId };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<int> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating staff.");
            return new ResponseModel<int> { ErrorMessage = "Unable to create staff.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateStaffAsync(int clientId, int id, UpdateStaffRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_UpdateStaff",
                new
                {
                    ClientId = clientId,
                    UserId = id,
                    Email = request.Email,
                    FullName = request.FullName,
                    MobileNo = request.MobileNo,
                    RoleIdsJson = request.RoleIds == null ? null : JsonSerializer.Serialize(request.RoleIds),
                    IsActive = request.IsActive,
                    ModifiedBy = modifiedBy
                },
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
            _logger.LogError(ex, "Error while updating staff.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update staff.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> DeactivateStaffAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_DeactivateStaff",
                new { ClientId = clientId, UserId = id, ModifiedBy = modifiedBy },
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
            _logger.LogError(ex, "Error while deactivating staff.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to deactivate staff.", ErrorId = -1 };
        }
    }
}

