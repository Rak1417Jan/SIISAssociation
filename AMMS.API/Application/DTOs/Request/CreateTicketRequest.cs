using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class CreateTicketRequest
{
    public TicketCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? AttachmentUrls { get; set; }
}
