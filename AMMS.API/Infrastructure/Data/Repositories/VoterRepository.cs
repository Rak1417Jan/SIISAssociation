using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Voter repository implementation using Dapper
/// </summary>
public class VoterRepository : BaseRepository<Voter>, IVoterRepository
{
    protected override string TableName => "Voters";

    public VoterRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Voter?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Voters WHERE UserId = @UserId AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Voter>(
            new CommandDefinition(query, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Voter?> GetBySerialNumberAsync(int assemblyId, int boothId, string serialNumber, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Voters WHERE AssemblyId = @AssemblyId AND BoothId = @BoothId AND SerialNumber = @SerialNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Voter>(
            new CommandDefinition(query, new { AssemblyId = assemblyId, BoothId = boothId, SerialNumber = serialNumber }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Voter>> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Voters WHERE AssemblyId = @AssemblyId AND IsDeleted = 0 ORDER BY SerialNumber";
        return await _connection.QueryAsync<Voter>(
            new CommandDefinition(query, new { AssemblyId = assemblyId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Voter>> GetByBoothIdAsync(int boothId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Voters WHERE BoothId = @BoothId AND IsDeleted = 0 ORDER BY SerialNumber";
        return await _connection.QueryAsync<Voter>(
            new CommandDefinition(query, new { BoothId = boothId }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "UserId, AssemblyId, BoothId, SerialNumber, Name, DateOfBirth, FatherName, Address, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@UserId, @AssemblyId, @BoothId, @SerialNumber, @Name, @DateOfBirth, @FatherName, @Address, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "UserId = @UserId, AssemblyId = @AssemblyId, BoothId = @BoothId, SerialNumber = @SerialNumber, Name = @Name, DateOfBirth = @DateOfBirth, FatherName = @FatherName, Address = @Address, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
