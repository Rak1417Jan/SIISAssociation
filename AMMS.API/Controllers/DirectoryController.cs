using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/directory")]
[Authorize]
public sealed class DirectoryController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public DirectoryController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet("members")]
    public async Task<ActionResult<ResponseModel<PagedResponse<DirectoryMemberDto>>>> GetMembers(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        bool includeContact = IsAdminRole();
        ResponseModel<PagedResponse<DirectoryMemberDto>> result = await _platformService.GetDirectoryMembersAsync(clientId, page, pageSize, search, includeContact, cancellationToken);
        return Ok(result);
    }

    [HttpGet("members/{id:int}")]
    public async Task<ActionResult<ResponseModel<DirectoryMemberDto>>> GetMember([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        bool includeContact = IsAdminRole();
        ResponseModel<DirectoryMemberDto> result = await _platformService.GetDirectoryMemberAsync(clientId, id, includeContact, cancellationToken);
        return Ok(result);
    }

    [HttpGet("industries")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<IndustryDto>>>> GetIndustries(CancellationToken cancellationToken)
    {
        ResponseModel<IReadOnlyList<IndustryDto>> result = await _platformService.GetIndustriesAsync(cancellationToken);
        return Ok(result);
    }
}
