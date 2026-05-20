namespace MVEA.Application.DTOs.Request;

public class CreateContentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentType { get; set; } = string.Empty; // text, photo, video
    public string? MediaUrl { get; set; }
}
