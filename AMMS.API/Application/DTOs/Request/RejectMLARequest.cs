namespace MVEA.Application.DTOs.Request;

public class RejectMLARequest
{
    public int MLAId { get; set; }
    public string RejectionReason { get; set; } = string.Empty; // Mandatory
}
