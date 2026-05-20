using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/members")]
public sealed class AdminMembersController : ControllerBase
{
    private readonly IAdminMembersService _adminMembersService;

    public AdminMembersController(IAdminMembersService adminMembersService)
    {
        _adminMembersService = adminMembersService;
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<PagedResponse<AdminMemberListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<PagedResponse<AdminMemberListItemResponse>>>> GetMembers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? firmId = null,
        [FromQuery] int? planId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var result = await _adminMembersService.GetMembersAsync(clientId, page, pageSize, status, firmId, planId, search, dateFrom, dateTo, sortBy, sortOrder, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResponseModel<AdminMemberDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<AdminMemberDetailResponse>>> GetMemberDetail([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var result = await _adminMembersService.GetMemberDetailAsync(clientId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpPut("{id:int}/verify")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<bool>>> VerifyMember([FromRoute] int id, [FromBody] VerifyMemberRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var changedBy = GetUserIdOrDefault();
        var result = await _adminMembersService.VerifyMemberAsync(clientId, id, request, changedBy, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpPut("{id:int}/hold")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<bool>>> HoldMember([FromRoute] int id, [FromBody] HoldMemberRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var changedBy = GetUserIdOrDefault();
        var result = await _adminMembersService.HoldMemberAsync(clientId, id, request, changedBy, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpPut("{id:int}/reject")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<bool>>> RejectMember([FromRoute] int id, [FromBody] RejectMemberRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var changedBy = GetUserIdOrDefault();
        var result = await _adminMembersService.RejectMemberAsync(clientId, id, request, changedBy, cancellationToken);
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

