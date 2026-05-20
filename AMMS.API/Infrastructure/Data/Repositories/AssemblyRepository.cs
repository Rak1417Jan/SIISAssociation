using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Assembly repository implementation using Dapper
/// </summary>
public class AssemblyRepository : BaseRepository<Assembly>, IAssemblyRepository
{
    protected override string TableName => "Assemblies";

    public AssemblyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Assembly?> GetByAssemblyNumberAsync(string assemblyNumber, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Assemblies WHERE AssemblyNumber = @AssemblyNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Assembly>(
            new CommandDefinition(query, new { AssemblyNumber = assemblyNumber }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Assembly>> GetActiveAssembliesAsync(CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Assemblies WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY AssemblyNumber";
        return await _connection.QueryAsync<Assembly>(
            new CommandDefinition(query, transaction: _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "AssemblyNumber, AssemblyName, State, District, IsActive, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@AssemblyNumber, @AssemblyName, @State, @District, @IsActive, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "AssemblyNumber = @AssemblyNumber, AssemblyName = @AssemblyName, State = @State, District = @District, IsActive = @IsActive, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
