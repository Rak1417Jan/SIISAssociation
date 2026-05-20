using Microsoft.AspNetCore.Authorization;

namespace AMMS.API.Security;

public sealed class MinRoleRequirement : IAuthorizationRequirement
{
    public MinRoleRequirement(RoleLevel minimumRole)
    {
        MinimumRole = minimumRole;
    }

    public RoleLevel MinimumRole { get; }
}

