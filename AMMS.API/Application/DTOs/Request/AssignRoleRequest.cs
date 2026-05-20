using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class AssignRoleRequest
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
}
