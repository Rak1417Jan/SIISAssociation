namespace MVEA.Model.DTOs.Response;

public class AdminMemberListItemResponse
{
    public int MemberId { get; init; }
    public string MembershipId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
}

