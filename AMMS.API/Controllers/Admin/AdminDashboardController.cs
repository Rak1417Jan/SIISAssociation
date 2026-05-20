using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin")]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ResponseModel<AdminDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<AdminDashboardResponse>>> GetDashboard(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var result = await _adminDashboardService.GetDashboardAsync(clientId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(ResponseModel<AdminAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<AdminAnalyticsResponse>>> GetAnalytics([FromQuery] int? year, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var result = await _adminDashboardService.GetAnalyticsAsync(clientId, year, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet("pending-queue")]
    [ProducesResponseType(typeof(ResponseModel<PagedResponse<PendingQueueItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<PagedResponse<PendingQueueItemResponse>>>> GetPendingQueue([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var result = await _adminDashboardService.GetPendingQueueAsync(clientId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    private bool TryGetClientId(out int clientId)
    {
        var value = User.FindFirstValue("ClientId");
        return int.TryParse(value, out clientId) && clientId > 0;
    }
}

