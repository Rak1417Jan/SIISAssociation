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
public class VotersController : ControllerBase
{
    private readonly IVoterService _voterService;
    private readonly ILogger<VotersController> _logger;

    public VotersController(IVoterService voterService, ILogger<VotersController> logger)
    {
        _voterService = voterService;
        _logger = logger;
    }

    /// <summary>
    /// Verify voter using Assembly, Booth, and Serial Number
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VoterVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VoterVerificationResponse>> VerifyVoter([FromBody] VerifyVoterRequest request)
    {
        try
        {
            var result = await _voterService.VerifyVoterAsync(request);
            
            if (!result.IsVerified)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying voter");
            return StatusCode(500, new { message = "An error occurred while verifying voter" });
        }
    }

    /// <summary>
    /// Fetch voter profile and family details
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(VoterProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VoterProfileResponse>> GetVoterProfile()
    {
        try
        {
            // TODO: Get voter ID from JWT claims
            int voterId = GetCurrentVoterId(); // Placeholder

            var result = await _voterService.GetVoterProfileAsync(voterId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voter profile");
            return StatusCode(500, new { message = "An error occurred while retrieving voter profile" });
        }
    }

    /// <summary>
    /// Add or update family member details with consent
    /// </summary>
    [HttpPost("family")]
    [ProducesResponseType(typeof(FamilyMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyMemberResponse>> AddFamilyMember([FromBody] AddFamilyMemberRequest request)
    {
        try
        {
            if (!request.HasConsent)
            {
                return BadRequest(new { message = "Consent is mandatory for adding family members" });
            }

            // TODO: Get voter ID from JWT claims
            int voterId = GetCurrentVoterId(); // Placeholder

            var result = await _voterService.AddFamilyMemberAsync(request, voterId);
            return CreatedAtAction(nameof(GetVoterProfile), new { }, result);
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
            _logger.LogError(ex, "Error adding family member");
            return StatusCode(500, new { message = "An error occurred while adding family member" });
        }
    }

    private int GetCurrentVoterId()
    {
        // TODO: Extract voter ID from JWT claims
        return 1; // Placeholder
    }
}
