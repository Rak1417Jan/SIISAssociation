using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/broadcasts")]
public sealed class BroadcastsV1Controller : AmmsControllerBase
{
    private readonly IBroadcastService _broadcastService;

    public BroadcastsV1Controller(IBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResponse<BroadcastListItemResponse>>>> GetList(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<PagedResponse<BroadcastListItemResponse>> result = await _broadcastService.GetBroadcastsAsync(clientId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost]
    public async Task<ActionResult<ResponseModel<int>>> Create([FromBody] CreateBroadcastRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<int> result = await _broadcastService.CreateAsync(clientId, request, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseModel<BroadcastDetailResponse>>> GetDetail([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<BroadcastDetailResponse> result = await _broadcastService.GetDetailAsync(clientId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost("{id:int}/send")]
    public async Task<ActionResult<ResponseModel<bool>>> Send([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _broadcastService.SendAsync(clientId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost("{id:int}/schedule")]
    public async Task<ActionResult<ResponseModel<bool>>> Schedule([FromRoute] int id, [FromBody] ScheduleBroadcastRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _broadcastService.ScheduleAsync(clientId, id, request.ScheduledAt, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ResponseModel<bool>>> Cancel([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _broadcastService.CancelAsync(clientId, id, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<ResponseModel<BroadcastStatsResponse>>> Stats([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<BroadcastStatsResponse> result = await _broadcastService.GetStatsAsync(clientId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _broadcastService.DeleteAsync(clientId, id, GetUserId(), cancellationToken);
        return Ok(result);
    }
}
