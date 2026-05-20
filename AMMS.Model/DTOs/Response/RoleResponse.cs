using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class RoleResponse
{
    public int Id { get; set; }
    public UserRole Role { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
