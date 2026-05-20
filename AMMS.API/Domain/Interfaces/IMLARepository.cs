using MVEA.Domain.Entities;
using MVEA.Domain.Enums;

namespace MVEA.Domain.Interfaces;

public interface IMLARepository : IRepository<MLA>
{
    Task<MLA?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<MLA?> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MLA>> GetByStatusAsync(ProfileStatus status, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAssemblyIdAsync(int assemblyId, int? excludeMLAId = null, CancellationToken cancellationToken = default);
}
