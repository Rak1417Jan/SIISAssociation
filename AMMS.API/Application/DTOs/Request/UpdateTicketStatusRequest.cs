using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class UpdateTicketStatusRequest
{
    public TicketStatus Status { get; set; }
    public string? Comment { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolutionProofUrl { get; set; }
}
