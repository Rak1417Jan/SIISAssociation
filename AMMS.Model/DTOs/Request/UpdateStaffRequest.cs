namespace MVEA.Model.DTOs.Request;

public sealed class UpdateStaffRequest
{
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public string? MobileNo { get; init; }
    /// <summary>When set, replaces all roles for the user. Each id must be <c>ROLES.ROLE_ID</c> for the current client.</summary>
    public IReadOnlyList<int>? RoleIds { get; init; }
    public bool? IsActive { get; init; }
}
