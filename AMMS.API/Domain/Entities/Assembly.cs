namespace MVEA.Domain.Entities;

/// <summary>
/// Assembly Constituency (AC) entity
/// </summary>
public class Assembly : BaseEntity
{
    public string AssemblyNumber { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? District { get; set; }
    public bool IsActive { get; set; }
}
