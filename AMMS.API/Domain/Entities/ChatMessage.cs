namespace MVEA.Domain.Entities;

/// <summary>
/// Individual chat message
/// </summary>
public class ChatMessage : BaseEntity
{
    public int ChatId { get; set; }
    public int? SenderUserId { get; set; }
    public bool IsFromVoter { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsTemplateResponse { get; set; }
    public string? TemplateId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
