using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IFamilyMemberRepository : IRepository<FamilyMember>
{
    Task<IEnumerable<FamilyMember>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default);
    Task<FamilyMember?> GetByVoterIdAndNameAsync(int voterId, string name, CancellationToken cancellationToken = default);
}
