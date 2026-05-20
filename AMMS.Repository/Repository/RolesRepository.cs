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

public sealed class RolesRepository : IRolesRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<RolesRepository> _logger;

    public RolesRepository(ISqlConnectionFactory connection, ILogger<RolesRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ResponseModel<IReadOnlyList<RoleRowResponse>>> GetRolesAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition("usp_Admin_GetRoles", new { ClientId = clientId }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            var rows = (await connection.QueryAsync<RoleRowResponse>(cmd)).ToList();
            return new ResponseModel<IReadOnlyList<RoleRowResponse>> { Data = rows };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching roles.");
            return new ResponseModel<IReadOnlyList<RoleRowResponse>> { ErrorMessage = "Unable to fetch roles.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdatePermissionsAsync(int clientId, string roleName, UpdateRolePermissionsRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var permissionIdsJson = JsonSerializer.Serialize(request?.PermissionIds ?? Array.Empty<int>());
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_UpdateRolePermissions",
                new { ClientId = clientId, RoleName = roleName, PermissionIdsJson = permissionIdsJson, ModifiedBy = modifiedBy },
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
            _logger.LogError(ex, "Error while updating role permissions.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update role permissions.", ErrorId = -1 };
        }
    }
}

