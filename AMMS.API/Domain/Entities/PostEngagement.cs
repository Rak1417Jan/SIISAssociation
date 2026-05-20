namespace MVEA.Domain.Entities;

/// <summary>
/// User engagement with posts (likes, shares)
/// </summary>
public class PostEngagement : BaseEntity
{
    public int ContentPostId { get; set; }
    public int VoterId { get; set; }
    public string EngagementType { get; set; } = string.Empty; // like, share
    public DateTime EngagedAt { get; set; }
}
