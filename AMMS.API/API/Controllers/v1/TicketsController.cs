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
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Create a grievance ticket by voter
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> CreateTicket([FromBody] CreateTicketRequest request)
    {
        try
        {
            // TODO: Get voter ID from JWT claims
            int voterId = GetCurrentVoterId(); // Placeholder

            var result = await _ticketService.CreateTicketAsync(request, voterId);
            return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket");
            return StatusCode(500, new { message = "An error occurred while creating ticket" });
        }
    }

    /// <summary>
    /// Fetch ticket details and status
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TicketDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TicketDetailResponse>> GetTicket(int id)
    {
        try
        {
            // TODO: Get user ID and role from JWT claims
            int? userId = GetCurrentUserId();
            bool isVoter = IsVoter();

            var result = await _ticketService.GetTicketByIdAsync(id, userId, isVoter);
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
            _logger.LogError(ex, "Error retrieving ticket {TicketId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving ticket" });
        }
    }

    /// <summary>
    /// Update ticket status with remarks
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusRequest request)
    {
        try
        {
            // TODO: Get user ID from JWT claims
            int userId = GetCurrentUserId(); // Placeholder

            var result = await _ticketService.UpdateTicketStatusAsync(id, request, userId);
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
            _logger.LogError(ex, "Error updating ticket status {TicketId}", id);
            return StatusCode(500, new { message = "An error occurred while updating ticket status" });
        }
    }

    /// <summary>
    /// Generate grievance performance reports
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(TicketReportResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketReportResponse>> GetTicketReport(
        [FromQuery] int? assemblyId = null,
        [FromQuery] int? mlaId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _ticketService.GetTicketReportAsync(assemblyId, mlaId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ticket report");
            return StatusCode(500, new { message = "An error occurred while generating ticket report" });
        }
    }

    private int GetCurrentVoterId()
    {
        // TODO: Extract voter ID from JWT claims
        return 1; // Placeholder
    }

    private int GetCurrentUserId()
    {
        // TODO: Extract user ID from JWT claims
        return 1; // Placeholder
    }

    private bool IsVoter()
    {
        // TODO: Check user role from JWT claims
        return false; // Placeholder
    }
}
