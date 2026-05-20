namespace MVEA.Application.DTOs.Request;

public class CreateMLAProfileRequest
{
    public int AssemblyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Party { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? VisionDescription { get; set; }
    public DateTime? TermStartDate { get; set; }
    public DateTime? TermEndDate { get; set; }
}
