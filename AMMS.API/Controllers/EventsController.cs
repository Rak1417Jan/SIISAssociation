using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/events")]
public sealed class EventsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public EventsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EventListItemDto>>>> GetList(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<IReadOnlyList<EventListItemDto>> result = await _platformService.GetEventsAsync(clientId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseModel<EventDetailDto>>> GetDetail([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        TryGetMemberId(out int memberId);
        ResponseModel<EventDetailDto> result = await _platformService.GetEventByIdAsync(clientId, id, memberId > 0 ? memberId : null, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost]
    public async Task<ActionResult<ResponseModel<int>>> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<int> result = await _platformService.CreateEventAsync(clientId, request, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> Update([FromRoute] int id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.UpdateEventAsync(clientId, id, request, cancellationToken);
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

        ResponseModel<bool> result = await _platformService.DeleteEventAsync(clientId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:int}/register")]
    public async Task<ActionResult<ResponseModel<bool>>> Register([FromRoute] int id, [FromBody] EventRsvpRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.RsvpEventAsync(id, memberId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:int}/register")]
    public async Task<ActionResult<ResponseModel<bool>>> CancelRegistration([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.CancelEventRsvpAsync(id, memberId, cancellationToken);
        return Ok(result);
    }
}
