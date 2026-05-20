using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(int role, CancellationToken cancellationToken = default);
}
