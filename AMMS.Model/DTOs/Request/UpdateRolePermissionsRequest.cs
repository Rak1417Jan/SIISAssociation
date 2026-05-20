namespace MVEA.Model.DTOs.Request;

/// <summary>Replace all permissions assigned to the role with the given permission identifiers.</summary>
public sealed class UpdateRolePermissionsRequest
{
    public IReadOnlyList<int> PermissionIds { get; init; } = Array.Empty<int>();
}
