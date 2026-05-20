using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Ticket repository implementation using Dapper
/// </summary>
public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    protected override string TableName => "Tickets";

    public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<Ticket>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Tickets WHERE VoterId = @VoterId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Ticket>(
            new CommandDefinition(query, new { VoterId = voterId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Tickets WHERE Status = @Status AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Ticket>(
            new CommandDefinition(query, new { Status = (int)status }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Ticket>> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Tickets WHERE AssemblyId = @AssemblyId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Ticket>(
            new CommandDefinition(query, new { AssemblyId = assemblyId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Tickets WHERE TicketNumber = @TicketNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Ticket>(
            new CommandDefinition(query, new { TicketNumber = ticketNumber }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "VoterId, AssemblyId, MLAId, TicketNumber, Category, Title, Description, Status, AssignedAt, ResolvedAt, SLAHours, ResolutionNote, ResolutionProofUrl, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@VoterId, @AssemblyId, @MLAId, @TicketNumber, @Category, @Title, @Description, @Status, @AssignedAt, @ResolvedAt, @SLAHours, @ResolutionNote, @ResolutionProofUrl, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "VoterId = @VoterId, AssemblyId = @AssemblyId, MLAId = @MLAId, TicketNumber = @TicketNumber, Category = @Category, Title = @Title, Description = @Description, Status = @Status, AssignedAt = @AssignedAt, ResolvedAt = @ResolvedAt, SLAHours = @SLAHours, ResolutionNote = @ResolutionNote, ResolutionProofUrl = @ResolutionProofUrl, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
