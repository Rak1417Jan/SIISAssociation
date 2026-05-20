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
public class MLAController : ControllerBase
{
    private readonly IMLAService _mlaService;
    private readonly ILogger<MLAController> _logger;

    public MLAController(IMLAService mlaService, ILogger<MLAController> logger)
    {
        _mlaService = mlaService;
        _logger = logger;
    }

    /// <summary>
    /// Create or submit MLA profile for approval
    /// </summary>
    [HttpPost("profile")]
    [ProducesResponseType(typeof(MLAResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MLAResponse>> CreateOrSubmitMLAProfile([FromBody] CreateMLAProfileRequest request)
    {
        try
        {
            // TODO: Get userId from JWT claims
            int userId = GetCurrentUserId(); // Placeholder - implement from JWT claims

            var result = await _mlaService.CreateOrSubmitMLAProfileAsync(request, userId);
            return CreatedAtAction(nameof(GetMLAProfile), new { }, result);
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
            _logger.LogError(ex, "Error creating/submitting MLA profile");
            return StatusCode(500, new { message = "An error occurred while creating/submitting MLA profile" });
        }
    }

    /// <summary>
    /// Fetch MLA public and private profile details
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(MLAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MLAResponse>> GetMLAProfile([FromQuery] bool includePrivateDetails = false)
    {
        try
        {
            // TODO: Get userId from JWT claims
            int userId = GetCurrentUserId(); // Placeholder - implement from JWT claims

            var result = await _mlaService.GetMLAProfileAsync(userId, includePrivateDetails);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving MLA profile");
            return StatusCode(500, new { message = "An error occurred while retrieving MLA profile" });
        }
    }

    /// <summary>
    /// Update MLA profile information
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(MLAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MLAResponse>> UpdateMLAProfile([FromBody] UpdateMLAProfileRequest request)
    {
        try
        {
            // TODO: Get userId from JWT claims
            int userId = GetCurrentUserId(); // Placeholder - implement from JWT claims

            var result = await _mlaService.UpdateMLAProfileAsync(userId, request);
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
            _logger.LogError(ex, "Error updating MLA profile");
            return StatusCode(500, new { message = "An error occurred while updating MLA profile" });
        }
    }

    private int GetCurrentUserId()
    {
        // TODO: Extract userId from JWT claims
        // Example: var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // return int.Parse(userIdClaim ?? "0");
        return 1; // Placeholder
    }
}
