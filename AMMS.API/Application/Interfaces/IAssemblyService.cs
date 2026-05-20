using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IAssemblyService
{
    Task<IEnumerable<AssemblyResponse>> GetAllAssembliesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<AssemblyResponse?> GetAssemblyByIdAsync(int id, CancellationToken cancellationToken = default);
}
