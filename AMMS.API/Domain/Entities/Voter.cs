namespace MVEA.Domain.Entities;

/// <summary>
/// Voter entity
/// </summary>
public class Voter : BaseEntity
{
    public int UserId { get; set; }
    public int AssemblyId { get; set; }
    public int BoothId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? FatherName { get; set; }
    public string? Address { get; set; }
}
