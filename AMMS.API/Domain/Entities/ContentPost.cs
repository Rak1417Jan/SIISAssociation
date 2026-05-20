namespace MVEA.Domain.Entities;

/// <summary>
/// Content post by MLA (photos, videos, updates)
/// </summary>
public class ContentPost : BaseEntity
{
    public int MLAId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentType { get; set; } = string.Empty; // text, photo, video
    public string? MediaUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public string? ShareWhatsAppLink { get; set; }
}
