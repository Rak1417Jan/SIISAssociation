CREATE PROCEDURE [dbo].[usp_Admin_GetRoles]
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        roleId = r.ROLE_ID,
        roleName = r.ROLE_NAME,
        description = ISNULL(r.DESCRIPTION, ''),
        permissions = COALESCE(agg.JsonPerm, ISNULL(NULLIF(LTRIM(RTRIM(r.PERMISSIONS)), N''), N'[]'))
    FROM dbo.ROLES r
    OUTER APPLY (
        SELECT N'[' + STRING_AGG(N'"' + REPLACE(p.PERMISSION_CODE, N'"', N'\"') + N'"', N',')
            WITHIN GROUP (ORDER BY p.PERMISSION_CODE) + N']' AS JsonPerm
        FROM dbo.ROLE_PERMISSION rp
        INNER JOIN dbo.PERMISSIONS p ON p.PERMISSION_ID = rp.PERMISSION_ID
        WHERE rp.ROLE_ID = r.ROLE_ID
    ) agg
    WHERE r.CLIENT_ID = @ClientId
    ORDER BY r.ROLE_ID ASC;
END
