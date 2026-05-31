using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/audit-logs")]
public sealed class AuditLogsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public AuditLogsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResponse<AuditLogListItemDto>>>> GetList(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] int? staffId,
        [FromQuery] string? actionType,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        AuditLogFilterRequest filter = new AuditLogFilterRequest
        {
            Page = page,
            PageSize = pageSize,
            StaffId = staffId,
            ActionType = actionType,
            EntityType = entityType,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        ResponseModel<PagedResponse<AuditLogListItemDto>> result = await _platformService.GetAuditLogsAsync(clientId, filter, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] int? staffId,
        [FromQuery] string? actionType,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        AuditLogFilterRequest filter = new AuditLogFilterRequest
        {
            StaffId = staffId,
            ActionType = actionType,
            EntityType = entityType,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        ResponseModel<byte[]> result = await _platformService.ExportAuditLogsAsync(clientId, filter, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return Ok(result);
        }

        return File(result.Data, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
