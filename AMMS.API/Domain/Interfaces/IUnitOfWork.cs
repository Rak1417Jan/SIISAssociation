using System.Data;

namespace MVEA.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the database connection
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Gets the current database transaction (null if no transaction is active)
    /// </summary>
    IDbTransaction? Transaction { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
