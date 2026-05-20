namespace MVEA.Model.DTOs.Response;

public sealed class AdminMemberDetailResponse
{
    public int MemberId { get; init; }
    public int? ApplicationId { get; init; }
    public string MembershipId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public DateTime? AnniversaryDate { get; init; }
    public bool IsActive { get; init; }
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string ApplicationStatus { get; init; } = string.Empty;
    public string ApplicationRemarks { get; init; } = string.Empty;
}

