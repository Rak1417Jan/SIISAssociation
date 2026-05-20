namespace AMMS.API.Security;

public static class RoleMapping
{
    public static RoleLevel ToRoleLevel(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return 0;
        }

        var normalized = role.Trim().Replace("_", " ").Replace("-", " ").ToLowerInvariant();

        return normalized switch
        {
            "support" => RoleLevel.Support,
            "finance" => RoleLevel.Finance,
            "manager" => RoleLevel.Manager,
            "admin" => RoleLevel.Admin,
            "super admin" => RoleLevel.SuperAdmin,
            "superadmin" => RoleLevel.SuperAdmin,
            _ => 0
        };
    }
}

