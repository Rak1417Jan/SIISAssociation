using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Family member repository implementation using Dapper
/// </summary>
public class FamilyMemberRepository : BaseRepository<FamilyMember>, IFamilyMemberRepository
{
    protected override string TableName => "FamilyMembers";

    public FamilyMemberRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<FamilyMember>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM FamilyMembers WHERE VoterId = @VoterId AND IsDeleted = 0 ORDER BY CreatedAt ASC";
        return await _connection.QueryAsync<FamilyMember>(
            new CommandDefinition(query, new { VoterId = voterId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<FamilyMember?> GetByVoterIdAndNameAsync(int voterId, string name, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM FamilyMembers WHERE VoterId = @VoterId AND Name = @Name AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<FamilyMember>(
            new CommandDefinition(query, new { VoterId = voterId, Name = name }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "VoterId, Name, DateOfBirth, MobileNumber, HasConsent, ConsentDate, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@VoterId, @Name, @DateOfBirth, @MobileNumber, @HasConsent, @ConsentDate, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "VoterId = @VoterId, Name = @Name, DateOfBirth = @DateOfBirth, MobileNumber = @MobileNumber, HasConsent = @HasConsent, ConsentDate = @ConsentDate, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
