using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Base repository implementation using Dapper
/// </summary>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IDbConnection _connection;
    protected readonly IDbTransaction? _transaction;
    protected abstract string TableName { get; }

    protected BaseRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _connection = unitOfWork.Connection;
        _transaction = unitOfWork.Transaction;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = $"SELECT * FROM {TableName} WHERE IsDeleted = 0";
        return await _connection.QueryAsync<T>(
            new CommandDefinition(query, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public virtual async Task<T?> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        var columns = GetInsertColumns();
        var values = GetInsertValues();
        var query = $"INSERT INTO {TableName} ({columns}) OUTPUT INSERTED.* VALUES ({values})";

        var result = await _connection.QuerySingleAsync<T>(
            new CommandDefinition(query, entity, _transaction, cancellationToken: cancellationToken));

        return result;
    }

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var setClause = GetUpdateSetClause();
        var query = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";

        _connection.Execute(query, entity, _transaction);
    }

    public virtual void Delete(T entity)
    {
        var query = $"DELETE FROM {TableName} WHERE Id = @Id";
        _connection.Execute(query, new { Id = entity.Id }, _transaction);
    }

    public virtual void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        var query = $"UPDATE {TableName} SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        _connection.Execute(query, new { Id = entity.Id, UpdatedAt = entity.UpdatedAt }, _transaction);
    }

    protected abstract string GetInsertColumns();
    protected abstract string GetInsertValues();
    protected abstract string GetUpdateSetClause();
}
