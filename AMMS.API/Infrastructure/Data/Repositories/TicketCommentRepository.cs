using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Ticket comment repository implementation using Dapper
/// </summary>
public class TicketCommentRepository : BaseRepository<TicketComment>, ITicketCommentRepository
{
    protected override string TableName => "TicketComments";

    public TicketCommentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TicketComment>> GetByTicketIdAsync(int ticketId, bool includeInternal = false, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM TicketComments WHERE TicketId = @TicketId AND IsDeleted = 0";
        
        if (!includeInternal)
        {
            query += " AND IsInternal = 0";
        }

        query += " ORDER BY CreatedAt ASC";

        return await _connection.QueryAsync<TicketComment>(
            new CommandDefinition(query, new { TicketId = ticketId }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "TicketId, UserId, Comment, IsInternal, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@TicketId, @UserId, @Comment, @IsInternal, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "TicketId = @TicketId, UserId = @UserId, Comment = @Comment, IsInternal = @IsInternal, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
