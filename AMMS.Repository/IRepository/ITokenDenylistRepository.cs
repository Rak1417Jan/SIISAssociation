namespace MVEA.Repository.IRepository;

public interface ITokenDenylistRepository
{
    Task<bool> IsDeniedAsync(int userId, string jti, CancellationToken cancellationToken = default);
    Task AddAsync(int userId, string jti, string? reason, CancellationToken cancellationToken = default);
}

