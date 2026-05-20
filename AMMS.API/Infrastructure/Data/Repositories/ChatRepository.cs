using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Chat repository implementation using Dapper
/// </summary>
public class ChatRepository : BaseRepository<Chat>, IChatRepository
{
    protected override string TableName => "Chats";

    public ChatRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Chat?> GetByVoterIdAndMLAIdAsync(int voterId, int? mlaId = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Chats WHERE VoterId = @VoterId AND IsDeleted = 0";
        
        if (mlaId.HasValue)
        {
            query += " AND MLAId = @MLAId";
        }
        else
        {
            query += " AND MLAId IS NULL";
        }

        query += " AND IsActive = 1";

        return await _connection.QueryFirstOrDefaultAsync<Chat>(
            new CommandDefinition(query, new { VoterId = voterId, MLAId = mlaId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Chat>> GetConversationsByMLAIdAsync(int mlaId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Chats WHERE MLAId = @MLAId AND IsDeleted = 0";
        
        if (activeOnly)
        {
            query += " AND IsActive = 1";
        }

        query += " ORDER BY LastMessageAt DESC, CreatedAt DESC";

        return await _connection.QueryAsync<Chat>(
            new CommandDefinition(query, new { MLAId = mlaId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Chat>> GetConversationsByVoterIdAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Chats WHERE VoterId = @VoterId AND IsDeleted = 0 AND IsActive = 1 ORDER BY LastMessageAt DESC, CreatedAt DESC";
        return await _connection.QueryAsync<Chat>(
            new CommandDefinition(query, new { VoterId = voterId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Chat>> GetConversationsByTypeAsync(ChatType type, int? mlaId = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Chats WHERE Type = @Type AND IsDeleted = 0";
        
        if (mlaId.HasValue)
        {
            query += " AND MLAId = @MLAId";
        }

        query += " ORDER BY LastMessageAt DESC";

        return await _connection.QueryAsync<Chat>(
            new CommandDefinition(query, new { Type = (int)type, MLAId = mlaId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdateLastMessageAsync(int chatId, DateTime lastMessageAt, CancellationToken cancellationToken = default)
    {
        var query = "UPDATE Chats SET LastMessageAt = @LastMessageAt, UpdatedAt = @UpdatedAt WHERE Id = @ChatId";
        await _connection.ExecuteAsync(
            new CommandDefinition(query, new { ChatId = chatId, LastMessageAt = lastMessageAt, UpdatedAt = DateTime.UtcNow }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "VoterId, MLAId, MLATeamMemberId, Type, IsActive, LastMessageAt, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@VoterId, @MLAId, @MLATeamMemberId, @Type, @IsActive, @LastMessageAt, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "VoterId = @VoterId, MLAId = @MLAId, MLATeamMemberId = @MLATeamMemberId, Type = @Type, IsActive = @IsActive, LastMessageAt = @LastMessageAt, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
