using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/broadcasts")]
public sealed class AdminBroadcastsController : ControllerBase
{
    private readonly IBroadcastService _broadcastService;

    public AdminBroadcastsController(IBroadcastService broadcastService)
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

        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
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

        int userId = GetUserIdOrDefault();
        ResponseModel<int> result = await _broadcastService.CreateAsync(clientId, request, userId, cancellationToken);
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
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        int userId = GetUserIdOrDefault();
        ResponseModel<bool> result = await _broadcastService.DeleteAsync(clientId, id, userId, cancellationToken);
        return Ok(result);
    }

    private int GetUserIdOrDefault()
    {
        string? value = User.FindFirstValue("UserId");
        return int.TryParse(value, out int id) ? id : 1;
    }

    private bool TryGetClientId(out int clientId)
    {
        string? value = User.FindFirstValue("ClientId");
        return int.TryParse(value, out clientId) && clientId > 0;
    }
}
