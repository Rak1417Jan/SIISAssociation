using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Member;

[ApiController]
[Route("api/v1/member/notifications")]
[Authorize(Roles = "Member")]
public sealed class MemberNotificationsController : ControllerBase
{
    private readonly IMemberNotificationsService _memberNotificationsService;

    public MemberNotificationsController(IMemberNotificationsService memberNotificationsService)
    {
        _memberNotificationsService = memberNotificationsService;
    }

    [HttpGet]
    public async Task<ActionResult<ResponseModel<MemberNotificationsResponse>>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<MemberNotificationsResponse> result = await _memberNotificationsService.GetAsync(memberId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("read")]
    public async Task<ActionResult<ResponseModel<bool>>> MarkRead([FromBody] MarkNotificationsReadRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _memberNotificationsService.MarkReadAsync(memberId, request, cancellationToken);
        return Ok(result);
    }

    private bool TryGetMemberId(out int memberId)
    {
        string? value = User.FindFirstValue("MemberId");
        return int.TryParse(value, out memberId);
    }
}
