namespace MVEA.Application.DTOs.Response;

public class AssemblyResponse
{
    public int Id { get; set; }
    public string AssemblyNumber { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? District { get; set; }
    public bool IsActive { get; set; }
}
