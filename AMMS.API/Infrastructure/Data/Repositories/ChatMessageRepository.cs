using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Chat message repository implementation using Dapper
/// </summary>
public class ChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
{
    protected override string TableName => "ChatMessages";

    public ChatMessageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<ChatMessage>> GetByChatIdAsync(int chatId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM ChatMessages WHERE ChatId = @ChatId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        
        if (limit.HasValue && limit.Value > 0)
        {
            query = "SELECT TOP (@Limit) * FROM ChatMessages WHERE ChatId = @ChatId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        }

        return await _connection.QueryAsync<ChatMessage>(
            new CommandDefinition(query, new { ChatId = chatId, Limit = limit }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM ChatMessages WHERE ChatId = @ChatId AND IsFromVoter = @IsFromVoter AND IsRead = 0 AND IsDeleted = 0 ORDER BY CreatedAt ASC";
        return await _connection.QueryAsync<ChatMessage>(
            new CommandDefinition(query, new { ChatId = chatId, IsFromVoter = isFromVoter }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task MarkMessagesAsReadAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default)
    {
        var query = "UPDATE ChatMessages SET IsRead = 1, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt WHERE ChatId = @ChatId AND IsFromVoter = @IsFromVoter AND IsRead = 0";
        await _connection.ExecuteAsync(
            new CommandDefinition(query, new { ChatId = chatId, IsFromVoter = isFromVoter, ReadAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<int> GetUnreadCountAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM ChatMessages WHERE ChatId = @ChatId AND IsFromVoter = @IsFromVoter AND IsRead = 0 AND IsDeleted = 0";
        return await _connection.QuerySingleAsync<int>(
            new CommandDefinition(query, new { ChatId = chatId, IsFromVoter = isFromVoter }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "ChatId, SenderUserId, IsFromVoter, Message, IsTemplateResponse, TemplateId, IsRead, ReadAt, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@ChatId, @SenderUserId, @IsFromVoter, @Message, @IsTemplateResponse, @TemplateId, @IsRead, @ReadAt, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "ChatId = @ChatId, SenderUserId = @SenderUserId, IsFromVoter = @IsFromVoter, Message = @Message, IsTemplateResponse = @IsTemplateResponse, TemplateId = @TemplateId, IsRead = @IsRead, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
