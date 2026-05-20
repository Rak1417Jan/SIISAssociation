namespace MVEA.Model.DTOs.Request;

public class UpdateContentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; }
    public string? MediaUrl { get; set; }
}
