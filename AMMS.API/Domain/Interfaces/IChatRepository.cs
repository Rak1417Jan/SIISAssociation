using MVEA.Domain.Entities;
using MVEA.Domain.Enums;

namespace MVEA.Domain.Interfaces;

public interface IChatRepository : IRepository<Chat>
{
    Task<Chat?> GetByVoterIdAndMLAIdAsync(int voterId, int? mlaId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chat>> GetConversationsByMLAIdAsync(int mlaId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chat>> GetConversationsByVoterIdAsync(int voterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chat>> GetConversationsByTypeAsync(ChatType type, int? mlaId = null, CancellationToken cancellationToken = default);
    Task UpdateLastMessageAsync(int chatId, DateTime lastMessageAt, CancellationToken cancellationToken = default);
}
