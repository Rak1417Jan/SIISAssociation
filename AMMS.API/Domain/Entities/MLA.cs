using MVEA.Domain.Enums;

namespace MVEA.Domain.Entities;

/// <summary>
/// MLA (Member of Legislative Assembly) entity
/// </summary>
public class MLA : BaseEntity
{
    public int UserId { get; set; }
    public int AssemblyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Party { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? VisionDescription { get; set; }
    public DateTime? TermStartDate { get; set; }
    public DateTime? TermEndDate { get; set; }
    public ProfileStatus Status { get; set; } = ProfileStatus.Draft;
    public string? RejectionReason { get; set; }
}
