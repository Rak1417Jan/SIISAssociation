using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;
using System.Security.Claims;

namespace MVEA.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/roles")]
public sealed class AdminRolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public AdminRolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [Authorize(Policy = "MinRole:Admin")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<RoleRowResponse>>>> GetRoles(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        return Ok(await _rolesService.GetRolesAsync(clientId, cancellationToken));
    }

    /// <summary>Update permissions for a specific role.</summary>
    /// <param name="role">Role name for the current client (matches <c>ROLES.ROLE_NAME</c>).</param>
    [Authorize(Policy = "MinRole:SuperAdmin")]
    [HttpPut("{role}")]
    public async Task<ActionResult<ResponseModel<bool>>> UpdateRolePermissions(
        [FromRoute] string role,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        var userId = GetUserIdOrDefault();
        var result = await _rolesService.UpdatePermissionsAsync(clientId, role, request, userId, cancellationToken);
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
