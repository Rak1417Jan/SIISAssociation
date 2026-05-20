namespace MVEA.Application.DTOs.Request;

public class VerifyVoterRequest
{
    public int AssemblyId { get; set; }
    public int BoothId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
}
