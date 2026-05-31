using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/grievances")]
public sealed class GrievancesController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public GrievancesController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ResponseModel<int>>> Submit([FromBody] SubmitGrievanceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<int> result = await _platformService.SubmitGrievanceAsync(clientId, memberId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GrievanceListItemDto>>>> MyList(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<IReadOnlyList<GrievanceListItemDto>> result = await _platformService.GetMyGrievancesAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResponse<GrievanceListItemDto>>>> AdminList(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<PagedResponse<GrievanceListItemDto>> result = await _platformService.GetGrievancesAsync(clientId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> Update([FromRoute] int id, [FromBody] UpdateGrievanceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.UpdateGrievanceAsync(clientId, id, request, cancellationToken);
        return Ok(result);
    }
}
