using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IVoterRepository : IRepository<Voter>
{
    Task<Voter?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Voter?> GetBySerialNumberAsync(int assemblyId, int boothId, string serialNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Voter>> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Voter>> GetByBoothIdAsync(int boothId, CancellationToken cancellationToken = default);
}
