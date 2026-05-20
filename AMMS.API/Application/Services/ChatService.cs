using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Chat service implementation with Unit of Work pattern
/// </summary>
public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatRepository _chatRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IVoterRepository? _voterRepository;
    private readonly IMLARepository _mlaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IUnitOfWork unitOfWork,
        IChatRepository chatRepository,
        IChatMessageRepository chatMessageRepository,
        IMLARepository mlaRepository,
        IUserRepository userRepository,
        ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _chatRepository = chatRepository;
        _chatMessageRepository = chatMessageRepository;
        _mlaRepository = mlaRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ChatConversationResponse>> GetConversationsAsync(int userId, bool isVoter, CancellationToken cancellationToken = default)
    {
        IEnumerable<Chat> conversations;

        if (isVoter)
        {
            // Get voter conversations
            conversations = await _chatRepository.GetConversationsByVoterIdAsync(userId, cancellationToken);
        }
        else
        {
            // Get MLA/Team member conversations
            // TODO: Get MLA ID from user
            // For now, assuming userId maps to MLAId or we need to resolve it
            var mla = await _mlaRepository.GetByUserIdAsync(userId, cancellationToken);
            if (mla == null)
            {
                return Enumerable.Empty<ChatConversationResponse>();
            }

            conversations = await _chatRepository.GetConversationsByMLAIdAsync(mla.Id, true, cancellationToken);
        }

        var responses = new List<ChatConversationResponse>();

        foreach (var chat in conversations)
        {
            // Get voter details
            var voter = await _voterRepository?.GetByIdAsync(chat.VoterId, cancellationToken);
            var voterUser = await _userRepository.GetByIdAsync(chat.VoterId, cancellationToken);

            // Get MLA details if applicable
            string? mlaName = null;
            if (chat.MLAId.HasValue)
            {
                var mla = await _mlaRepository.GetByIdAsync(chat.MLAId.Value, cancellationToken);
                mlaName = mla?.Name;
            }

            // Get last message
            var lastMessages = await _chatMessageRepository.GetByChatIdAsync(chat.Id, 1, cancellationToken);
            var lastMessage = lastMessages.FirstOrDefault();

            // Get unread count
            int unreadCount = 0;
            if (!isVoter)
            {
                unreadCount = await _chatMessageRepository.GetUnreadCountAsync(chat.Id, true, cancellationToken);
            }
            else
            {
                unreadCount = await _chatMessageRepository.GetUnreadCountAsync(chat.Id, false, cancellationToken);
            }

            responses.Add(new ChatConversationResponse
            {
                Id = chat.Id,
                VoterId = chat.VoterId,
                VoterName = voter?.Name ?? voterUser?.MobileNumber ?? string.Empty,
                VoterMobile = voterUser?.MobileNumber,
                MLAId = chat.MLAId,
                MLAName = mlaName,
                MLATeamMemberId = chat.MLATeamMemberId,
                Type = chat.Type,
                TypeName = chat.Type.ToString(),
                IsActive = chat.IsActive,
                LastMessageAt = chat.LastMessageAt,
                LastMessage = lastMessage?.Message,
                HasUnreadMessages = unreadCount > 0,
                UnreadCount = unreadCount,
                CreatedAt = chat.CreatedAt
            });
        }

        return responses;
    }

    public async Task<IEnumerable<ChatMessageResponse>> GetChatHistoryAsync(int conversationId, int userId, bool isVoter, int? limit = null, CancellationToken cancellationToken = default)
    {
        // Verify chat exists and user has access
        var chat = await _chatRepository.GetByIdAsync(conversationId, cancellationToken);
        if (chat == null)
        {
            throw new KeyNotFoundException($"Chat conversation with ID {conversationId} not found");
        }

        // Verify access
        if (isVoter && chat.VoterId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this conversation");
        }

        // Mark messages as read when retrieving history
        await _chatMessageRepository.MarkMessagesAsReadAsync(conversationId, !isVoter, cancellationToken);

        // Get messages
        var messages = await _chatMessageRepository.GetByChatIdAsync(conversationId, limit, cancellationToken);

        var responses = new List<ChatMessageResponse>();

        foreach (var message in messages.OrderBy(m => m.CreatedAt))
        {
            string? senderName = null;
            if (message.SenderUserId.HasValue)
            {
                var sender = await _userRepository.GetByIdAsync(message.SenderUserId.Value, cancellationToken);
                senderName = sender?.MobileNumber;
            }

            responses.Add(new ChatMessageResponse
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderUserId = message.SenderUserId,
                SenderName = senderName,
                IsFromVoter = message.IsFromVoter,
                Message = message.Message,
                IsTemplateResponse = message.IsTemplateResponse,
                TemplateId = message.TemplateId,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                CreatedAt = message.CreatedAt
            });
        }

        return responses;
    }

    public async Task<ChatMessageResponse> SendMessageAsync(SendChatMessageRequest request, int senderUserId, bool isFromVoter, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Verify chat exists
            var chat = await _chatRepository.GetByIdAsync(request.ConversationId, cancellationToken);
            if (chat == null)
            {
                throw new KeyNotFoundException($"Chat conversation with ID {request.ConversationId} not found");
            }

            // Verify chat is active
            if (!chat.IsActive)
            {
                throw new InvalidOperationException("This chat conversation is no longer active");
            }

            // Create message
            var message = new ChatMessage
            {
                ChatId = request.ConversationId,
                SenderUserId = senderUserId,
                IsFromVoter = isFromVoter,
                Message = request.Message,
                IsTemplateResponse = !string.IsNullOrEmpty(request.TemplateId),
                TemplateId = request.TemplateId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdMessage = await _chatMessageRepository.AddAsync(message, cancellationToken);

            // Update chat last message timestamp
            await _chatRepository.UpdateLastMessageAsync(request.ConversationId, DateTime.UtcNow, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Message sent in chat {ChatId} by user {UserId}", request.ConversationId, senderUserId);

            // Get sender name
            var sender = await _userRepository.GetByIdAsync(senderUserId, cancellationToken);

            return new ChatMessageResponse
            {
                Id = createdMessage.Id,
                ChatId = createdMessage.ChatId,
                SenderUserId = createdMessage.SenderUserId,
                SenderName = sender?.MobileNumber,
                IsFromVoter = createdMessage.IsFromVoter,
                Message = createdMessage.Message,
                IsTemplateResponse = createdMessage.IsTemplateResponse,
                TemplateId = createdMessage.TemplateId,
                IsRead = createdMessage.IsRead,
                ReadAt = createdMessage.ReadAt,
                CreatedAt = createdMessage.CreatedAt
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> TagChatAsync(TagChatRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var chat = await _chatRepository.GetByIdAsync(request.ConversationId, cancellationToken);
            if (chat == null)
            {
                throw new KeyNotFoundException($"Chat conversation with ID {request.ConversationId} not found");
            }

            // Update chat type
            chat.Type = request.Type;
            chat.UpdatedAt = DateTime.UtcNow;
            _chatRepository.Update(chat);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Chat {ChatId} tagged as {Type}", request.ConversationId, request.Type);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
