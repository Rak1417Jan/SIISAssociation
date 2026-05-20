using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IContentRepository : IRepository<ContentPost>
{
    Task<IEnumerable<ContentPost>> GetByMLAIdAsync(int mlaId, bool publishedOnly = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContentPost>> GetPublishedContentAsync(int? assemblyId = null, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<ContentPost?> GetPublishedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(int id, CancellationToken cancellationToken = default);
    Task IncrementLikeCountAsync(int id, CancellationToken cancellationToken = default);
    Task IncrementShareCountAsync(int id, CancellationToken cancellationToken = default);
}
