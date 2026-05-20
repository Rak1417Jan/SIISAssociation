using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Request;

public class CreateUserRequest
{
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public UserRole Role { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? AssemblyId { get; set; }
    public int? BoothId { get; set; }
    public string? SerialNumber { get; set; }
    public int? MLAId { get; set; } // For MLA Team Member
}
