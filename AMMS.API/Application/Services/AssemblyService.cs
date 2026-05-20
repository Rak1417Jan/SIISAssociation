using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Assembly service implementation with Unit of Work pattern
/// </summary>
public class AssemblyService : IAssemblyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly ILogger<AssemblyService> _logger;

    public AssemblyService(
        IUnitOfWork unitOfWork,
        IAssemblyRepository assemblyRepository,
        ILogger<AssemblyService> logger)
    {
        _unitOfWork = unitOfWork;
        _assemblyRepository = assemblyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AssemblyResponse>> GetAllAssembliesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        IEnumerable<Domain.Entities.Assembly> assemblies;

        if (activeOnly)
        {
            assemblies = await _assemblyRepository.GetActiveAssembliesAsync(cancellationToken);
        }
        else
        {
            assemblies = await _assemblyRepository.GetAllAsync(cancellationToken);
        }

        return assemblies.Select(a => new AssemblyResponse
        {
            Id = a.Id,
            AssemblyNumber = a.AssemblyNumber,
            AssemblyName = a.AssemblyName,
            State = a.State,
            District = a.District,
            IsActive = a.IsActive
        });
    }

    public async Task<AssemblyResponse?> GetAssemblyByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var assembly = await _assemblyRepository.GetByIdAsync(id, cancellationToken);
        if (assembly == null)
        {
            return null;
        }

        return new AssemblyResponse
        {
            Id = assembly.Id,
            AssemblyNumber = assembly.AssemblyNumber,
            AssemblyName = assembly.AssemblyName,
            State = assembly.State,
            District = assembly.District,
            IsActive = assembly.IsActive
        };
    }
}
