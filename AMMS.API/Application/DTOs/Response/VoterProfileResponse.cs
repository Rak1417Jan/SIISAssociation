namespace MVEA.Application.DTOs.Response;

public class VoterProfileResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int AssemblyId { get; set; }
    public string AssemblyNumber { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public int BoothId { get; set; }
    public string BoothNumber { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? FatherName { get; set; }
    public string? Address { get; set; }
    public List<FamilyMemberResponse> FamilyMembers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class FamilyMemberResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? MobileNumber { get; set; }
    public bool HasConsent { get; set; }
    public DateTime? ConsentDate { get; set; }
}
