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
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Schedule notifications using templates and channels
    /// </summary>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ScheduledNotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScheduledNotificationResponse>> ScheduleNotification([FromBody] ScheduleNotificationRequest request)
    {
        try
        {
            // TODO: Get MLA ID from JWT claims
            int mlaId = GetCurrentMLAId(); // Placeholder - implement from JWT claims

            var result = await _notificationService.ScheduleNotificationAsync(request, mlaId);
            return CreatedAtAction(nameof(GetDeliveryLogs), new { notificationId = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling notification");
            return StatusCode(500, new { message = "An error occurred while scheduling notification" });
        }
    }

    /// <summary>
    /// Fetch available notification templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IEnumerable<NotificationTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationTemplateResponse>>> GetNotificationTemplates()
    {
        try
        {
            var result = await _notificationService.GetNotificationTemplatesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification templates");
            return StatusCode(500, new { message = "An error occurred while retrieving notification templates" });
        }
    }

    /// <summary>
    /// View delivery and failure logs
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(IEnumerable<NotificationDeliveryLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationDeliveryLogResponse>>> GetDeliveryLogs(
        [FromQuery] int? notificationId = null,
        [FromQuery] int? voterId = null,
        [FromQuery] bool? isDelivered = null,
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

            var result = await _notificationService.GetDeliveryLogsAsync(
                notificationId: notificationId,
                voterId: voterId,
                isDelivered: isDelivered,
                startDate: startDate,
                endDate: endDate,
                page: page,
                pageSize: pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery logs");
            return StatusCode(500, new { message = "An error occurred while retrieving delivery logs" });
        }
    }

    private int GetCurrentMLAId()
    {
        // TODO: Extract MLA ID from JWT claims
        return 1; // Placeholder
    }
}
