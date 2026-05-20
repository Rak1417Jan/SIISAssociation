using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;

namespace MVEA.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ILogger<ContentController> _logger;

    public ContentController(IContentService contentService, ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new post (text, image, video)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentResponse>> CreateContent([FromBody] CreateContentRequest request)
    {
        try
        {
            // TODO: Get MLA ID from JWT claims
            int mlaId = GetCurrentMLAId(); // Placeholder - implement from JWT claims

            var result = await _contentService.CreateContentAsync(request, mlaId);
            return CreatedAtAction(nameof(GetContent), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content");
            return StatusCode(500, new { message = "An error occurred while creating content" });
        }
    }

    /// <summary>
    /// Fetch content feed for voters
    /// </summary>
    [HttpGet("feed")]
    [ProducesResponseType(typeof(IEnumerable<ContentFeedResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContentFeedResponse>>> GetContentFeed(
        [FromQuery] int? assemblyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // TODO: Get voter ID from JWT claims (optional - for like status)
            int? voterId = GetCurrentVoterId(); // Placeholder

            var result = await _contentService.GetContentFeedAsync(assemblyId, page, pageSize, voterId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving content feed");
            return StatusCode(500, new { message = "An error occurred while retrieving content feed" });
        }
    }

    /// <summary>
    /// Edit content before publishing
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentResponse>> UpdateContent(int id, [FromBody] UpdateContentRequest request)
    {
        try
        {
            // TODO: Get MLA ID from JWT claims
            int mlaId = GetCurrentMLAId(); // Placeholder

            var result = await _contentService.UpdateContentAsync(id, request, mlaId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating content {ContentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating content" });
        }
    }

    /// <summary>
    /// Approve content for public visibility
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "SystemAdmin,MLATeamMember")] // Only admin and MLA team can approve
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ApproveContent(int id)
    {
        try
        {
            // TODO: Get admin/user ID from JWT claims
            int adminUserId = GetCurrentUserId(); // Placeholder

            var result = await _contentService.ApproveContentAsync(id, adminUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving content {ContentId}", id);
            return StatusCode(500, new { message = "An error occurred while approving content" });
        }
    }

    /// <summary>
    /// Soft delete content
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> DeleteContent(int id)
    {
        try
        {
            // TODO: Get MLA ID from JWT claims
            int mlaId = GetCurrentMLAId(); // Placeholder

            var result = await _contentService.DeleteContentAsync(id, mlaId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content {ContentId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting content" });
        }
    }

    /// <summary>
    /// Get content by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentResponse>> GetContent(int id)
    {
        try
        {
            var result = await _contentService.GetContentByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving content {ContentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving content" });
        }
    }

    private int GetCurrentMLAId()
    {
        // TODO: Extract MLA ID from JWT claims
        return 1; // Placeholder
    }

    private int? GetCurrentVoterId()
    {
        // TODO: Extract voter ID from JWT claims
        return null; // Placeholder
    }

    private int GetCurrentUserId()
    {
        // TODO: Extract user ID from JWT claims
        return 1; // Placeholder
    }
}
