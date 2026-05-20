namespace MVEA.Domain.Entities;

/// <summary>
/// Family member of a voter
/// </summary>
public class FamilyMember : BaseEntity
{
    public int VoterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? MobileNumber { get; set; }
    public bool HasConsent { get; set; }
    public DateTime? ConsentDate { get; set; }
}
