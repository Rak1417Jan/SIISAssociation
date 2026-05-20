namespace MVEA.Model.DTOs.Response;

public class ContentFeedResponse
{
    public int Id { get; set; }
    public int MLAId { get; set; }
    public string MLAName { get; set; } = string.Empty;
    public string MLAParty { get; set; } = string.Empty;
    public string? MLAProfilePictureUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public string? ShareWhatsAppLink { get; set; }
    public bool HasLiked { get; set; } // If current user has liked this post
}
