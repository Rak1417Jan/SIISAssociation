using MVEA.Domain.Enums;

namespace MVEA.Domain.Entities;

/// <summary>
/// User entity - base class for all users in the system
/// </summary>
public class User : BaseEntity
{
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? DeviceInfo { get; set; }
}
