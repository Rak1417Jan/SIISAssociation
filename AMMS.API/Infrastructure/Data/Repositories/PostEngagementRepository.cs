using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Post engagement repository implementation using Dapper
/// </summary>
public class PostEngagementRepository : BaseRepository<PostEngagement>, IPostEngagementRepository
{
    protected override string TableName => "PostEngagements";

    public PostEngagementRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<bool> HasLikedAsync(int contentPostId, int voterId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM PostEngagements WHERE ContentPostId = @ContentPostId AND VoterId = @VoterId AND EngagementType = 'like' AND IsDeleted = 0";
        var count = await _connection.QuerySingleAsync<int>(
            new CommandDefinition(query, new { ContentPostId = contentPostId, VoterId = voterId }, _transaction, cancellationToken: cancellationToken));
        
        return count > 0;
    }

    public async Task<PostEngagement?> GetEngagementAsync(int contentPostId, int voterId, string engagementType, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM PostEngagements WHERE ContentPostId = @ContentPostId AND VoterId = @VoterId AND EngagementType = @EngagementType AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<PostEngagement>(
            new CommandDefinition(query, new { ContentPostId = contentPostId, VoterId = voterId, EngagementType = engagementType }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<PostEngagement>> GetByContentPostIdAsync(int contentPostId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM PostEngagements WHERE ContentPostId = @ContentPostId AND IsDeleted = 0 ORDER BY EngagedAt DESC";
        return await _connection.QueryAsync<PostEngagement>(
            new CommandDefinition(query, new { ContentPostId = contentPostId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<PostEngagement>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM PostEngagements WHERE VoterId = @VoterId AND IsDeleted = 0 ORDER BY EngagedAt DESC";
        return await _connection.QueryAsync<PostEngagement>(
            new CommandDefinition(query, new { VoterId = voterId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<int> GetLikeCountAsync(int contentPostId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM PostEngagements WHERE ContentPostId = @ContentPostId AND EngagementType = 'like' AND IsDeleted = 0";
        return await _connection.QuerySingleAsync<int>(
            new CommandDefinition(query, new { ContentPostId = contentPostId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<int> GetShareCountAsync(int contentPostId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM PostEngagements WHERE ContentPostId = @ContentPostId AND EngagementType = 'share' AND IsDeleted = 0";
        return await _connection.QuerySingleAsync<int>(
            new CommandDefinition(query, new { ContentPostId = contentPostId }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "ContentPostId, VoterId, EngagementType, EngagedAt, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@ContentPostId, @VoterId, @EngagementType, @EngagedAt, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "ContentPostId = @ContentPostId, VoterId = @VoterId, EngagementType = @EngagementType, EngagedAt = @EngagedAt, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
