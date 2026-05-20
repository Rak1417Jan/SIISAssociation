using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class MLAResponse
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
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
