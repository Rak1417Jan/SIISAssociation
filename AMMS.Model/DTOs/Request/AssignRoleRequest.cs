using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Request;

public class AssignRoleRequest
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
}
