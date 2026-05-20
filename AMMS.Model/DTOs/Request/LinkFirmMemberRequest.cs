namespace MVEA.Model.DTOs.Request;

public sealed class LinkFirmMemberRequest
{
    public int MemberId { get; init; }
    public string RoleInFirm { get; init; } = string.Empty;
}

