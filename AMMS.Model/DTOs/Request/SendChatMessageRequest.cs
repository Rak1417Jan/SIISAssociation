namespace MVEA.Model.DTOs.Request;

public class SendChatMessageRequest
{
    public int ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; } // Optional template ID for MLA team responses
}
