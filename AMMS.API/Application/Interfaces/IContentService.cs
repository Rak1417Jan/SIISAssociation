using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IContentService
{
    Task<ContentResponse> CreateContentAsync(CreateContentRequest request, int mlaId, CancellationToken cancellationToken = default);
    Task<ContentResponse> GetContentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ContentResponse> UpdateContentAsync(int id, UpdateContentRequest request, int mlaId, CancellationToken cancellationToken = default);
    Task<bool> ApproveContentAsync(int id, int adminUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteContentAsync(int id, int mlaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContentFeedResponse>> GetContentFeedAsync(int? assemblyId = null, int page = 1, int pageSize = 20, int? voterId = null, CancellationToken cancellationToken = default);
}
