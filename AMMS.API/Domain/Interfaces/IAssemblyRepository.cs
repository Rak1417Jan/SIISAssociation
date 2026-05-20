using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IAssemblyRepository : IRepository<Assembly>
{
    Task<Assembly?> GetByAssemblyNumberAsync(string assemblyNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assembly>> GetActiveAssembliesAsync(CancellationToken cancellationToken = default);
}
