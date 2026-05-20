using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatConversationResponse>> GetConversationsAsync(int userId, bool isVoter, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessageResponse>> GetChatHistoryAsync(int conversationId, int userId, bool isVoter, int? limit = null, CancellationToken cancellationToken = default);
    Task<ChatMessageResponse> SendMessageAsync(SendChatMessageRequest request, int senderUserId, bool isFromVoter, CancellationToken cancellationToken = default);
    Task<bool> TagChatAsync(TagChatRequest request, CancellationToken cancellationToken = default);
}
