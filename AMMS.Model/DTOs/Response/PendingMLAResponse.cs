using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class PendingMLAResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AssemblyId { get; set; }
    public string AssemblyNumber { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Party { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? VisionDescription { get; set; }
    public DateTime? TermStartDate { get; set; }
    public DateTime? TermEndDate { get; set; }
    public ProfileStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
