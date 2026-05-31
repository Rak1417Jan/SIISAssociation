using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/digital-id")]
public sealed class DigitalIdController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public DigitalIdController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<DigitalIdResponse>>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<DigitalIdResponse> result = await _platformService.GetDigitalIdAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("generate")]
    public async Task<ActionResult<ResponseModel<DigitalIdResponse>>> Generate(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<DigitalIdResponse> result = await _platformService.GenerateDigitalIdAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("download")]
    public async Task<IActionResult> Download(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<byte[]> result = await _platformService.DownloadDigitalIdAsync(clientId, memberId, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return Ok(result);
        }

        return File(result.Data, "text/plain", "digital-id.txt");
    }

    [AllowAnonymous]
    [HttpGet("verify/{membershipId}")]
    public async Task<ActionResult<ResponseModel<DigitalIdVerifyResponse>>> Verify([FromRoute] string membershipId, CancellationToken cancellationToken)
    {
        ResponseModel<DigitalIdVerifyResponse> result = await _platformService.VerifyDigitalIdPublicAsync(membershipId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("share")]
    public async Task<ActionResult<ResponseModel<bool>>> Share([FromBody] ShareDigitalIdRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.ShareDigitalIdAsync(clientId, memberId, request, cancellationToken);
        return Ok(result);
    }
}
