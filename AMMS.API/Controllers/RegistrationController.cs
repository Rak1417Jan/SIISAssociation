using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/registration")]
[Authorize]
public sealed class RegistrationController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public RegistrationController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<ResponseModel<StartRegistrationResponse>>> Start([FromBody] StartRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<StartRegistrationResponse> result = await _platformService.StartRegistrationAsync(clientId, request, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{applicationId:int}/step")]
    public async Task<ActionResult<ResponseModel<bool>>> Step([FromRoute] int applicationId, [FromBody] RegistrationStepRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.SaveRegistrationStepAsync(clientId, applicationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{applicationId:int}/submit")]
    public async Task<ActionResult<ResponseModel<bool>>> Submit([FromRoute] int applicationId, [FromBody] SubmitRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.SubmitRegistrationAsync(clientId, applicationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{applicationId:int}/status")]
    public async Task<ActionResult<ResponseModel<RegistrationStatusResponse>>> Status([FromRoute] int applicationId, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<RegistrationStatusResponse> result = await _platformService.GetRegistrationStatusAsync(clientId, applicationId, cancellationToken);
        return Ok(result);
    }
}
