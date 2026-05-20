using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IPostEngagementRepository : IRepository<PostEngagement>
{
    Task<bool> HasLikedAsync(int contentPostId, int voterId, CancellationToken cancellationToken = default);
    Task<PostEngagement?> GetEngagementAsync(int contentPostId, int voterId, string engagementType, CancellationToken cancellationToken = default);
    Task<IEnumerable<PostEngagement>> GetByContentPostIdAsync(int contentPostId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PostEngagement>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default);
    Task<int> GetLikeCountAsync(int contentPostId, CancellationToken cancellationToken = default);
    Task<int> GetShareCountAsync(int contentPostId, CancellationToken cancellationToken = default);
}
