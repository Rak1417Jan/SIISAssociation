using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Enums;

namespace MVEA.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "SystemAdmin")] // Only System Admin can access
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Fetch all MLA profiles pending approval
    /// </summary>
    [HttpGet("mla/pending")]
    [ProducesResponseType(typeof(IEnumerable<PendingMLAResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PendingMLAResponse>>> GetPendingMLAProfiles()
    {
        try
        {
            var result = await _adminService.GetPendingMLAProfilesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending MLA profiles");
            return StatusCode(500, new { message = "An error occurred while retrieving pending MLA profiles" });
        }
    }

    /// <summary>
    /// Approve MLA profile and make it public
    /// </summary>
    [HttpPost("mla/approve")]
    [ProducesResponseType(typeof(MLAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MLAResponse>> ApproveMLAProfile([FromBody] ApproveMLARequest request)
    {
        try
        {
            // TODO: Get admin user ID from JWT claims
            int adminUserId = GetCurrentUserId(); // Placeholder - implement from JWT claims

            var result = await _adminService.ApproveMLAProfileAsync(request, adminUserId);
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
            _logger.LogError(ex, "Error approving MLA profile {MLAId}", request.MLAId);
            return StatusCode(500, new { message = "An error occurred while approving MLA profile" });
        }
    }

    /// <summary>
    /// Reject MLA profile with mandatory reason
    /// </summary>
    [HttpPost("mla/reject")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> RejectMLAProfile([FromBody] RejectMLARequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest(new { message = "Rejection reason is mandatory" });
            }

            // TODO: Get admin user ID from JWT claims
            int adminUserId = GetCurrentUserId(); // Placeholder - implement from JWT claims

            var result = await _adminService.RejectMLAProfileAsync(request, adminUserId);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting MLA profile {MLAId}", request.MLAId);
            return StatusCode(500, new { message = "An error occurred while rejecting MLA profile" });
        }
    }

    /// <summary>
    /// View system audit logs
    /// </summary>
    [HttpGet("audit/logs")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditLogResponse>>> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var result = await _adminService.GetAuditLogsAsync(
                entityType: entityType,
                entityId: entityId,
                userId: userId,
                startDate: startDate,
                endDate: endDate,
                page: page,
                pageSize: pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new { message = "An error occurred while retrieving audit logs" });
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
