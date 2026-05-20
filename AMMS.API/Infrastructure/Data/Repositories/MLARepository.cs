using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// MLA repository implementation using Dapper
/// </summary>
public class MLARepository : BaseRepository<MLA>, IMLARepository
{
    protected override string TableName => "MLAs";

    public MLARepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<MLA?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM MLAs WHERE UserId = @UserId AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<MLA>(
            new CommandDefinition(query, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<MLA?> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM MLAs WHERE AssemblyId = @AssemblyId AND IsDeleted = 0 AND Status IN (4, 6)"; // Approved or Public
        return await _connection.QueryFirstOrDefaultAsync<MLA>(
            new CommandDefinition(query, new { AssemblyId = assemblyId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MLA>> GetByStatusAsync(ProfileStatus status, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM MLAs WHERE Status = @Status AND IsDeleted = 0";
        return await _connection.QueryAsync<MLA>(
            new CommandDefinition(query, new { Status = (int)status }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsByAssemblyIdAsync(int assemblyId, int? excludeMLAId = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM MLAs WHERE AssemblyId = @AssemblyId AND IsDeleted = 0 AND Status IN (3, 4, 6)"; // UnderReview, Approved, Public
        
        if (excludeMLAId.HasValue)
        {
            query += " AND Id != @ExcludeMLAId";
        }

        var count = await _connection.QuerySingleAsync<int>(
            new CommandDefinition(query, new { AssemblyId = assemblyId, ExcludeMLAId = excludeMLAId }, _transaction, cancellationToken: cancellationToken));

        return count > 0;
    }

    protected override string GetInsertColumns()
    {
        return "UserId, AssemblyId, Name, Party, ProfilePictureUrl, CoverPhotoUrl, VisionDescription, TermStartDate, TermEndDate, Status, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@UserId, @AssemblyId, @Name, @Party, @ProfilePictureUrl, @CoverPhotoUrl, @VisionDescription, @TermStartDate, @TermEndDate, @Status, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "Name = @Name, Party = @Party, ProfilePictureUrl = @ProfilePictureUrl, CoverPhotoUrl = @CoverPhotoUrl, VisionDescription = @VisionDescription, TermStartDate = @TermStartDate, TermEndDate = @TermEndDate, Status = @Status, RejectionReason = @RejectionReason, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
