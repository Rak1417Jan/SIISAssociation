using Dapper;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class CompanyTypeRepository : ICompanyTypeRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<CompanyTypeRepository> _logger;

    public CompanyTypeRepository(ISqlConnectionFactory connection, ILogger<CompanyTypeRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ResponseModel<IReadOnlyList<CompanyTypeResponse>>> GetCompanyTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition("usp_GetCompanyTypes", commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            var rows = (await connection.QueryAsync<CompanyTypeResponse>(cmd)).ToList();
            return new ResponseModel<IReadOnlyList<CompanyTypeResponse>> { Data = rows };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching company types.");
            return new ResponseModel<IReadOnlyList<CompanyTypeResponse>> { ErrorMessage = "Unable to fetch company types.", ErrorId = -1 };
        }
    }
}
