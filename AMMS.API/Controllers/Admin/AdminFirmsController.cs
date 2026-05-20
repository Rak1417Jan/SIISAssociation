using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/firms")]
public sealed class AdminFirmsController : ControllerBase
{
    private readonly IAdminFirmsService _adminFirmsService;

    public AdminFirmsController(IAdminFirmsService adminFirmsService)
    {
        _adminFirmsService = adminFirmsService;
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResponse<FirmListItemResponse>>>> GetFirms([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        return Ok(await _adminFirmsService.GetFirmsAsync(clientId, page, pageSize, search, cancellationToken));
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseModel<FirmDetailResponse>>> GetFirm([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        return Ok(await _adminFirmsService.GetFirmDetailAsync(clientId, id, cancellationToken));
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost]
    public async Task<ActionResult<ResponseModel<int>>> CreateFirm([FromBody] CreateFirmRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _adminFirmsService.CreateFirmAsync(clientId, request, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> UpdateFirm([FromRoute] int id, [FromBody] UpdateFirmRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _adminFirmsService.UpdateFirmAsync(clientId, id, request, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> DeleteFirm([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _adminFirmsService.SoftDeleteFirmAsync(clientId, id, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<ResponseModel<bool>>> LinkMember([FromRoute] int id, [FromBody] LinkFirmMemberRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _adminFirmsService.LinkMemberAsync(clientId, id, request, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpDelete("{id:int}/members/{mid:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> UnlinkMember([FromRoute] int id, [FromRoute] int mid, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _adminFirmsService.UnlinkMemberAsync(clientId, id, mid, userId, cancellationToken);
        return Ok(result);
    }

    private int GetUserIdOrDefault()
    {
        var value = User.FindFirstValue("UserId");
        return int.TryParse(value, out var id) ? id : 1;
    }

    private bool TryGetClientId(out int clientId)
    {
        var value = User.FindFirstValue("ClientId");
        return int.TryParse(value, out clientId) && clientId > 0;
    }
}
