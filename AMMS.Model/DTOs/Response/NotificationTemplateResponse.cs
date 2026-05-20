using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class NotificationTemplateResponse
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Placeholders { get; set; } = new(); // e.g., {VoterName}, {Age}, {BoothNo}
}
