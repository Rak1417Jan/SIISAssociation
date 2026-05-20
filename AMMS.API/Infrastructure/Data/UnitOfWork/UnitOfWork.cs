using System.Data;
using Dapper;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data;

namespace MVEA.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing transactions across repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _context;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWork(DapperContext context)
    {
        _context = context;
    }

    public IDbConnection Connection => _context.Connection;
    public IDbTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _transaction = Connection.BeginTransaction();
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In Dapper, SaveChanges is typically a no-op as operations execute immediately
        // This method is kept for interface compatibility and future extensions
        await Task.CompletedTask;
        return 0;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            _transaction.Commit();
            await Task.CompletedTask;
        }
        catch
        {
            _transaction.Rollback();
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback");
        }

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
        await Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
