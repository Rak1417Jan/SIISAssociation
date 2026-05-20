namespace MVEA.Application.DTOs.Response;

public class VoterVerificationResponse
{
    public bool IsVerified { get; set; }
    public int? VoterId { get; set; }
    public string? VoterName { get; set; }
    public string? Message { get; set; }
}
