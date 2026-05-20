namespace MVEA.Model.DTOs.Request;



public sealed class CreateStaffRequest

{

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? FullName { get; init; }

    public string? MobileNo { get; init; }

    /// <summary>At least one id; each must be <c>ROLES.ROLE_ID</c> for the current client (via <c>USER_ROLES</c>).</summary>
    public IReadOnlyList<int> RoleIds { get; init; } = Array.Empty<int>();



    /// <summary>Optional. Same PBKDF2 storage as admin login. If omitted, a temporary password is generated server-side (not returned).</summary>

    public string? Password { get; init; }

}

