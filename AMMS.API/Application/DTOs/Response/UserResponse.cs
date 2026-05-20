using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class UserResponse
{
    public int Id { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Name { get; set; }
    public int? AssemblyId { get; set; }
    public int? MLAId { get; set; }
}
