using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Ticket attachment repository implementation using Dapper
/// </summary>
public class TicketAttachmentRepository : BaseRepository<TicketAttachment>, ITicketAttachmentRepository
{
    protected override string TableName => "TicketAttachments";

    public TicketAttachmentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TicketAttachment>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM TicketAttachments WHERE TicketId = @TicketId AND IsDeleted = 0 ORDER BY CreatedAt ASC";
        return await _connection.QueryAsync<TicketAttachment>(
            new CommandDefinition(query, new { TicketId = ticketId }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "TicketId, FileUrl, FileName, FileType, FileSize, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@TicketId, @FileUrl, @FileName, @FileType, @FileSize, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "TicketId = @TicketId, FileUrl = @FileUrl, FileName = @FileName, FileType = @FileType, FileSize = @FileSize, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
