using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Content service implementation with Unit of Work pattern
/// </summary>
public class ContentService : IContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContentRepository _contentRepository;
    private readonly IMLARepository _mlaRepository;
    private readonly IPostEngagementRepository? _postEngagementRepository;
    private readonly ILogger<ContentService> _logger;

    public ContentService(
        IUnitOfWork unitOfWork,
        IContentRepository contentRepository,
        IMLARepository mlaRepository,
        ILogger<ContentService> logger)
    {
        _unitOfWork = unitOfWork;
        _contentRepository = contentRepository;
        _mlaRepository = mlaRepository;
        _logger = logger;
    }

    public async Task<ContentResponse> CreateContentAsync(CreateContentRequest request, int mlaId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Verify MLA exists
            var mla = await _mlaRepository.GetByIdAsync(mlaId, cancellationToken);
            if (mla == null)
            {
                throw new KeyNotFoundException($"MLA with ID {mlaId} not found");
            }

            // Create content post
            var contentPost = new ContentPost
            {
                MLAId = mlaId,
                Title = request.Title,
                Description = request.Description,
                ContentType = request.ContentType,
                MediaUrl = request.MediaUrl,
                IsPublished = false, // Initially not published, needs approval
                ViewCount = 0,
                LikeCount = 0,
                ShareCount = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdContent = await _contentRepository.AddAsync(contentPost, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Content post created for MLA {MLAId}", mlaId);

            return await MapToContentResponseAsync(createdContent, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ContentResponse> GetContentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null)
        {
            throw new KeyNotFoundException($"Content with ID {id} not found");
        }

        return await MapToContentResponseAsync(content, cancellationToken);
    }

    public async Task<ContentResponse> UpdateContentAsync(int id, UpdateContentRequest request, int mlaId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
            if (content == null)
            {
                throw new KeyNotFoundException($"Content with ID {id} not found");
            }

            // Verify ownership
            if (content.MLAId != mlaId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this content");
            }

            // Can only edit if not published
            if (content.IsPublished)
            {
                throw new InvalidOperationException("Cannot edit published content. Please create a new post.");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Title)) content.Title = request.Title;
            if (request.Description != null) content.Description = request.Description;
            if (!string.IsNullOrEmpty(request.ContentType)) content.ContentType = request.ContentType;
            if (request.MediaUrl != null) content.MediaUrl = request.MediaUrl;

            content.UpdatedAt = DateTime.UtcNow;
            _contentRepository.Update(content);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return await MapToContentResponseAsync(content, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> ApproveContentAsync(int id, int adminUserId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
            if (content == null)
            {
                throw new KeyNotFoundException($"Content with ID {id} not found");
            }

            if (content.IsPublished)
            {
                throw new InvalidOperationException("Content is already published");
            }

            // Approve and publish content
            content.IsPublished = true;
            content.PublishedAt = DateTime.UtcNow;
            content.UpdatedAt = DateTime.UtcNow;

            // Generate WhatsApp share link
            content.ShareWhatsAppLink = $"https://wa.me/?text={Uri.EscapeDataString(content.Title)} - {Uri.EscapeDataString(content.Description ?? "")}";

            _contentRepository.Update(content);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Content {ContentId} approved and published by admin {AdminUserId}", id, adminUserId);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteContentAsync(int id, int mlaId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
            if (content == null)
            {
                throw new KeyNotFoundException($"Content with ID {id} not found");
            }

            // Verify ownership (MLA can delete their own content, Admin can delete any)
            // For now, we'll allow MLA to delete their own content
            // TODO: Add admin role check

            if (content.MLAId != mlaId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this content");
            }

            _contentRepository.SoftDelete(content);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Content {ContentId} soft deleted by MLA {MLAId}", id, mlaId);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<ContentFeedResponse>> GetContentFeedAsync(int? assemblyId = null, int page = 1, int pageSize = 20, int? voterId = null, CancellationToken cancellationToken = default)
    {
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        int skip = (page - 1) * pageSize;

        // Get published content
        var contentPosts = await _contentRepository.GetPublishedContentAsync(assemblyId, skip, pageSize, cancellationToken);

        var feedItems = new List<ContentFeedResponse>();

        foreach (var content in contentPosts)
        {
            // Get MLA details
            var mla = await _mlaRepository.GetByIdAsync(content.MLAId, cancellationToken);

            // Check if voter has liked this post (if voterId provided)
            bool hasLiked = false;
            if (voterId.HasValue)
            {
                // TODO: Check PostEngagement table for like status
                // hasLiked = await _postEngagementRepository.HasLikedAsync(content.Id, voterId.Value, cancellationToken);
            }

            feedItems.Add(new ContentFeedResponse
            {
                Id = content.Id,
                MLAId = content.MLAId,
                MLAName = mla?.Name ?? string.Empty,
                MLAParty = mla?.Party ?? string.Empty,
                MLAProfilePictureUrl = mla?.ProfilePictureUrl,
                Title = content.Title,
                Description = content.Description,
                ContentType = content.ContentType,
                MediaUrl = content.MediaUrl,
                PublishedAt = content.PublishedAt,
                ViewCount = content.ViewCount,
                LikeCount = content.LikeCount,
                ShareCount = content.ShareCount,
                ShareWhatsAppLink = content.ShareWhatsAppLink,
                HasLiked = hasLiked
            });
        }

        return feedItems;
    }

    private async Task<ContentResponse> MapToContentResponseAsync(ContentPost content, CancellationToken cancellationToken = default)
    {
        var mla = await _mlaRepository.GetByIdAsync(content.MLAId, cancellationToken);

        return new ContentResponse
        {
            Id = content.Id,
            MLAId = content.MLAId,
            MLAName = mla?.Name ?? string.Empty,
            Title = content.Title,
            Description = content.Description,
            ContentType = content.ContentType,
            MediaUrl = content.MediaUrl,
            IsPublished = content.IsPublished,
            PublishedAt = content.PublishedAt,
            ViewCount = content.ViewCount,
            LikeCount = content.LikeCount,
            ShareCount = content.ShareCount,
            ShareWhatsAppLink = content.ShareWhatsAppLink,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }
}
