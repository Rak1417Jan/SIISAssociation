using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Request;

public class TagChatRequest
{
    public int ConversationId { get; set; }
    public ChatType Type { get; set; } // Complaint, Feedback, Request, General
}
