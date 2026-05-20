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
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Fetch list of chat conversations
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(typeof(IEnumerable<ChatConversationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ChatConversationResponse>>> GetConversations()
    {
        try
        {
            // TODO: Get user ID and role from JWT claims
            int userId = GetCurrentUserId(); // Placeholder
            bool isVoter = IsVoter(); // Placeholder - check from role claims

            var result = await _chatService.GetConversationsAsync(userId, isVoter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat conversations");
            return StatusCode(500, new { message = "An error occurred while retrieving chat conversations" });
        }
    }

    /// <summary>
    /// Retrieve chat message history
    /// </summary>
    [HttpGet("history/{conversationId}")]
    [ProducesResponseType(typeof(IEnumerable<ChatMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetChatHistory(
        int conversationId,
        [FromQuery] int? limit = null)
    {
        try
        {
            // TODO: Get user ID and role from JWT claims
            int userId = GetCurrentUserId(); // Placeholder
            bool isVoter = IsVoter(); // Placeholder

            var result = await _chatService.GetChatHistoryAsync(conversationId, userId, isVoter, limit);
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
            _logger.LogError(ex, "Error retrieving chat history for conversation {ConversationId}", conversationId);
            return StatusCode(500, new { message = "An error occurred while retrieving chat history" });
        }
    }

    /// <summary>
    /// Send message in one-to-one chat
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ChatMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatMessageResponse>> SendMessage([FromBody] SendChatMessageRequest request)
    {
        try
        {
            // TODO: Get user ID and role from JWT claims
            int userId = GetCurrentUserId(); // Placeholder
            bool isVoter = IsVoter(); // Placeholder

            var result = await _chatService.SendMessageAsync(request, userId, isVoter);
            return CreatedAtAction(nameof(GetChatHistory), new { conversationId = request.ConversationId }, result);
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
            _logger.LogError(ex, "Error sending chat message");
            return StatusCode(500, new { message = "An error occurred while sending message" });
        }
    }

    /// <summary>
    /// Tag chat with category (Complaint, Feedback, Request)
    /// </summary>
    [HttpPut("tag")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> TagChat([FromBody] TagChatRequest request)
    {
        try
        {
            var result = await _chatService.TagChatAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tagging chat {ConversationId}", request.ConversationId);
            return StatusCode(500, new { message = "An error occurred while tagging chat" });
        }
    }

    private int GetCurrentUserId()
    {
        // TODO: Extract user ID from JWT claims
        return 1; // Placeholder
    }

    private bool IsVoter()
    {
        // TODO: Check user role from JWT claims
        // Example: User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Voter"
        return false; // Placeholder
    }
}
