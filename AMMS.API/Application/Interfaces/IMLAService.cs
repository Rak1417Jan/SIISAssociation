using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IMLAService
{
    Task<MLAResponse> CreateOrSubmitMLAProfileAsync(CreateMLAProfileRequest request, int userId, CancellationToken cancellationToken = default);
    Task<MLAResponse> GetMLAProfileAsync(int userId, bool includePrivateDetails = false, CancellationToken cancellationToken = default);
    Task<MLAResponse> UpdateMLAProfileAsync(int userId, UpdateMLAProfileRequest request, CancellationToken cancellationToken = default);
}
