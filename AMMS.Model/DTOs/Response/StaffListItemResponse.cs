namespace MVEA.Model.DTOs.Response;

public sealed class StaffListItemResponse
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string MobileNo { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    /// <summary>Comma-separated role names for all assigned roles.</summary>
    public string RoleNames { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool MustChangePassword { get; init; }
    public DateTime CreatedDate { get; init; }
}
