using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IBoothRepository : IRepository<Booth>
{
    Task<IEnumerable<Booth>> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default);
}
