using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AMMS.API.Security;

public sealed class MinRoleRequirementHandler : AuthorizationHandler<MinRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinRoleRequirement requirement)
    {
        var role = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role") ?? string.Empty;
        var userLevel = RoleMapping.ToRoleLevel(role);

        if (userLevel >= requirement.MinimumRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

