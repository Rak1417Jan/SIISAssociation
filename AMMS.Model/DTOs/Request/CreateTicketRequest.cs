using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Request;

public class CreateTicketRequest
{
    public TicketCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? AttachmentUrls { get; set; }
}
