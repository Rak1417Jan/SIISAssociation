using Dapper;
using Microsoft.Extensions.Logging;

using MVEA.Comman;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.Interfaces;

using System.Data;


namespace MVEA.Repository.Repositories;

/// <summary>
/// User repository implementation using Dapper
/// </summary>
public class MasterRepository : IMasterRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<MasterRepository> _logger;

    public MasterRepository(ISqlConnectionFactory connection, ILogger<MasterRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }


   
    public async Task<ResponseModel<IList<MasterResponse>>> GetMasterAsync(MasterRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogWarning("GetMasterAsync called with null request.");
            return new ResponseModel<IList<MasterResponse>>() { ErrorMessage = "Request cannot be null.", ErrorId = -1 };
        }

        var connection = _connection.GetConnection();
        if (connection == null)
        {
            _logger.LogError("Database connection is not available in GetMasterAsync.");
            return new ResponseModel<IList<MasterResponse>>() { ErrorMessage = "Database connection is not available.", ErrorId = -1 };
        }

        try
        {
            var spParams = new
            {
                ParentId = request.ParentId == 0 ? (int?)null : request.ParentId,
                MasterId = request.Id == 0 ? (int?)null : request.Id
            };

            var cmd = new CommandDefinition(
                "sp_GetMasterRecords",
                spParams,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var results = (await connection.QueryAsync<MasterResponse>(cmd)).ToList();

            return new ResponseModel<IList<MasterResponse>>()
            {
                Data = results,
                TotalItems = results.Count
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetMasterAsync cancelled.");
            return new ResponseModel<IList<MasterResponse>>() { ErrorMessage = "GetMasterAsync cancelled.", ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching master records via stored procedure.");
            return new ResponseModel<IList<MasterResponse>>() { ErrorMessage = "Error while fetching master records.", ErrorId = -1 };
        }
    }

    
}
