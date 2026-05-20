namespace MVEA.Application.DTOs.Request;

public class AddFamilyMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? MobileNumber { get; set; }
    public bool HasConsent { get; set; }
}
