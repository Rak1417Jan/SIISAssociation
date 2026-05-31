using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/settings")]
public sealed class SettingsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public SettingsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<ClientSettingsDto>>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<ClientSettingsDto> result = await _platformService.GetSettingsAsync(clientId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:SuperAdmin")]
    [HttpPut]
    public async Task<ActionResult<ResponseModel<bool>>> Update([FromBody] UpdateClientSettingsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.UpdateSettingsAsync(clientId, request, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:SuperAdmin")]
    [HttpPost("logo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ResponseModel<string>>> UploadLogo(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<string> result = await _platformService.UploadLogoAsync(clientId, file, cancellationToken);
        return Ok(result);
    }
}
