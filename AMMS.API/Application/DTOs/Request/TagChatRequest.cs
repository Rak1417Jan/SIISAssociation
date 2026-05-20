using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class TagChatRequest
{
    public int ConversationId { get; set; }
    public ChatType Type { get; set; } // Complaint, Feedback, Request, General
}
