using Dapper;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class TokenDenylistRepository : ITokenDenylistRepository
{
    private readonly ISqlConnectionFactory _connection;

    public TokenDenylistRepository(ISqlConnectionFactory connection)
    {
        _connection = connection;
    }

    public async Task<bool> IsDeniedAsync(int userId, string jti, CancellationToken cancellationToken = default)
    {
        var connection = _connection.GetConnection();
        var cmd = new CommandDefinition(
            "usp_TokenDenylist_IsDenied",
            new { UserId = userId, Jti = jti },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var isDenied = await connection.QuerySingleAsync<bool>(cmd);
        return isDenied;
    }

    public async Task AddAsync(int userId, string jti, string? reason, CancellationToken cancellationToken = default)
    {
        var connection = _connection.GetConnection();
        var cmd = new CommandDefinition(
            "usp_TokenDenylist_Add",
            new { UserId = userId, Jti = jti, Reason = reason },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(cmd);
    }
}

