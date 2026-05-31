using AMMS.API.Security;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MVEA.API.Controllers;

public abstract class AmmsControllerBase : ControllerBase
{
    protected bool TryGetClientId(out int clientId)
    {
        string? value = User.FindFirstValue("ClientId");
        return int.TryParse(value, out clientId) && clientId > 0;
    }

    protected int GetUserId()
    {
        string? value = User.FindFirstValue("UserId");
        return int.TryParse(value, out int id) ? id : 0;
    }

    protected bool TryGetMemberId(out int memberId)
    {
        string? value = User.FindFirstValue("MemberId");
        return int.TryParse(value, out memberId) && memberId > 0;
    }

    protected bool IsAdminRole()
    {
        string? role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        RoleLevel level = RoleMapping.ToRoleLevel(role ?? string.Empty);
        return level >= RoleLevel.Admin;
    }
}
