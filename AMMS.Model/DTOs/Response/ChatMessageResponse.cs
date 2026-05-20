namespace MVEA.Model.DTOs.Response;

public class ChatMessageResponse
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int? SenderUserId { get; set; }
    public string? SenderName { get; set; }
    public bool IsFromVoter { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsTemplateResponse { get; set; }
    public string? TemplateId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
