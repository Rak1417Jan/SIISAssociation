using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/referrals")]
public sealed class ReferralsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public ReferralsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize]
    [HttpGet("my-code")]
    public async Task<ActionResult<ResponseModel<ReferralCodeResponse>>> GetMyCode(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<ReferralCodeResponse> result = await _platformService.GetMyReferralCodeAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("track")]
    public async Task<ActionResult<ResponseModel<int>>> Track([FromBody] TrackReferralRequest request, CancellationToken cancellationToken)
    {
        int clientId = 1;
        if (TryGetClientId(out int claimClientId))
        {
            clientId = claimClientId;
        }

        ResponseModel<int> result = await _platformService.TrackReferralAsync(clientId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<ActionResult<ResponseModel<ReferralStatsDto>>> Stats(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<ReferralStatsDto> result = await _platformService.GetReferralStatsAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResponse<ReferralListItemDto>>>> AdminList(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<PagedResponse<ReferralListItemDto>> result = await _platformService.GetReferralsAsync(clientId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
