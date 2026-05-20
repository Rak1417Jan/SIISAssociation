using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/staff")]
public sealed class AdminStaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public AdminStaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<StaffListItemResponse>>>> GetStaff(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        return Ok(await _staffService.GetStaffAsync(clientId, cancellationToken));
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPost]
    public async Task<ActionResult<ResponseModel<int>>> CreateStaff([FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _staffService.CreateStaffAsync(clientId, request, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> UpdateStaff([FromRoute] int id, [FromBody] UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        if (userId == id)
        {
            return Ok(new ResponseModel<bool> { ErrorMessage = "Cannot modify own account via this endpoint.", ErrorId = -1 });
        }

        var result = await _staffService.UpdateStaffAsync(clientId, id, request, userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ResponseModel<bool>>> DeactivateStaff([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        if (userId == id)
        {
            return Ok(new ResponseModel<bool> { ErrorMessage = "Cannot deactivate own account.", ErrorId = -1 });
        }

        var result = await _staffService.DeactivateStaffAsync(clientId, id, userId, cancellationToken);
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
