namespace MVEA.Model.DTOs.Response;

public sealed class RoleRowResponse
{
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Permissions { get; init; } = string.Empty;
}

