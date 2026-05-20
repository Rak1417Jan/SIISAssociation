namespace MVEA.Domain.Entities;

/// <summary>
/// Polling booth entity
/// </summary>
public class Booth : BaseEntity
{
    public int AssemblyId { get; set; }
    public string BoothNumber { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Address { get; set; }
}
