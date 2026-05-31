using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/plans")]
public sealed class PlansController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public PlansController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<MembershipPlanDto>>>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<IReadOnlyList<MembershipPlanDto>> result = await _platformService.GetPlansAsync(clientId, cancellationToken);
        return Ok(result);
    }
}
