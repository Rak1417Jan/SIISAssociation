namespace MVEA.Model.DTOs.Response;

/// <summary>
/// Active session snapshot from JWT claims (and optional future DB enrichment).
/// </summary>
public sealed class SessionInfoResponse
{
    public int? UserId { get; init; }

    public int? ClientId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string? MobileNumber { get; init; }

    public int? MemberId { get; init; }

    public bool IsActive { get; init; } = true;
}
