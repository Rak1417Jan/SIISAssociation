using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Audit log repository implementation using Dapper
/// </summary>
public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    protected override string TableName => "AuditLogs";

    public AuditLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM AuditLogs WHERE EntityType = @EntityType AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<AuditLog>(
            new CommandDefinition(query, new { EntityType = entityType }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM AuditLogs WHERE UserId = @UserId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<AuditLog>(
            new CommandDefinition(query, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM AuditLogs WHERE EntityType = @EntityType AND EntityId = @EntityId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<AuditLog>(
            new CommandDefinition(query, new { EntityType = entityType, EntityId = entityId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM AuditLogs WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<AuditLog>(
            new CommandDefinition(query, new { StartDate = startDate, EndDate = endDate }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "EntityType, EntityId, Action, UserId, UserName, OldValues, NewValues, Description, IpAddress, UserAgent, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@EntityType, @EntityId, @Action, @UserId, @UserName, @OldValues, @NewValues, @Description, @IpAddress, @UserAgent, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "EntityType = @EntityType, EntityId = @EntityId, Action = @Action, UserId = @UserId, UserName = @UserName, OldValues = @OldValues, NewValues = @NewValues, Description = @Description, IpAddress = @IpAddress, UserAgent = @UserAgent, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
