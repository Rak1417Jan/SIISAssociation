using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Content repository implementation using Dapper
/// </summary>
public class ContentRepository : BaseRepository<ContentPost>, IContentRepository
{
    protected override string TableName => "ContentPosts";

    public ContentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<ContentPost>> GetByMLAIdAsync(int mlaId, bool publishedOnly = false, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM ContentPosts WHERE MLAId = @MLAId AND IsDeleted = 0";
        if (publishedOnly)
        {
            query += " AND IsPublished = 1";
        }
        query += " ORDER BY CreatedAt DESC";

        return await _connection.QueryAsync<ContentPost>(
            new CommandDefinition(query, new { MLAId = mlaId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ContentPost>> GetPublishedContentAsync(int? assemblyId = null, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT cp.* FROM ContentPosts cp
            INNER JOIN MLAs m ON cp.MLAId = m.Id
            WHERE cp.IsPublished = 1 AND cp.IsDeleted = 0";

        if (assemblyId.HasValue)
        {
            query += " AND m.AssemblyId = @AssemblyId";
        }

        query += " ORDER BY cp.PublishedAt DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

        return await _connection.QueryAsync<ContentPost>(
            new CommandDefinition(query, new { AssemblyId = assemblyId, Skip = skip, Take = take }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ContentPost?> GetPublishedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM ContentPosts WHERE Id = @Id AND IsPublished = 1 AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<ContentPost>(
            new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task IncrementViewCountAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "UPDATE ContentPosts SET ViewCount = ViewCount + 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task IncrementLikeCountAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "UPDATE ContentPosts SET LikeCount = LikeCount + 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task IncrementShareCountAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "UPDATE ContentPosts SET ShareCount = ShareCount + 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "MLAId, Title, Description, ContentType, MediaUrl, IsPublished, PublishedAt, ViewCount, LikeCount, ShareCount, ShareWhatsAppLink, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@MLAId, @Title, @Description, @ContentType, @MediaUrl, @IsPublished, @PublishedAt, @ViewCount, @LikeCount, @ShareCount, @ShareWhatsAppLink, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "Title = @Title, Description = @Description, ContentType = @ContentType, MediaUrl = @MediaUrl, IsPublished = @IsPublished, PublishedAt = @PublishedAt, ViewCount = @ViewCount, LikeCount = @LikeCount, ShareCount = @ShareCount, ShareWhatsAppLink = @ShareWhatsAppLink, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
