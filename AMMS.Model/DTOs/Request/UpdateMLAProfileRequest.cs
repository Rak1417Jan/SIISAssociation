namespace MVEA.Model.DTOs.Request;

public class UpdateMLAProfileRequest
{
    public string? Name { get; set; }
    public string? Party { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? VisionDescription { get; set; }
    public DateTime? TermStartDate { get; set; }
    public DateTime? TermEndDate { get; set; }
}
