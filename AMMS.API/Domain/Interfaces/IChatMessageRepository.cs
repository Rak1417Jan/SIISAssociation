using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetByChatIdAsync(int chatId, int? limit = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default);
    Task MarkMessagesAsReadAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int chatId, bool isFromVoter, CancellationToken cancellationToken = default);
}
