CREATE PROCEDURE [dbo].[usp_Admin_GetStaff]
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.USER_ID AS UserId,
        u.USERNAME AS Username,
        u.EMAIL AS Email,
        LTRIM(RTRIM(ISNULL(u.FULL_NAME, N''))) AS FullName,
        LTRIM(RTRIM(ISNULL(u.MOBILE_NO, N''))) AS MobileNo,
        CAST(ISNULL(rx.ROLE_ID, 0) AS int) AS RoleId,
        ISNULL(rx.ROLE_NAME, N'') AS RoleName,
        LTRIM(RTRIM(ISNULL(rn.RoleNames, N''))) AS RoleNames,
        CAST(ISNULL(u.IS_ACTIVE, 0) AS bit) AS IsActive,
        CAST(ISNULL(u.MUST_CHANGE_PASSWORD, 0) AS bit) AS MustChangePassword,
        ISNULL(u.CREATED_DATE, GETDATE()) AS CreatedDate
    FROM dbo.USERS u
    OUTER APPLY (
        SELECT TOP (1)
            ur.ROLE_ID,
            r.ROLE_NAME
        FROM dbo.USER_ROLES ur
        INNER JOIN dbo.ROLES r ON r.ROLE_ID = ur.ROLE_ID AND r.CLIENT_ID = u.CLIENT_ID
        WHERE ur.USER_ID = u.USER_ID
        ORDER BY ur.ROLE_ID ASC
    ) AS rx
    OUTER APPLY (
        SELECT STRING_AGG(r2.ROLE_NAME, N', ') WITHIN GROUP (ORDER BY r2.ROLE_NAME) AS RoleNames
        FROM dbo.USER_ROLES ur2
        INNER JOIN dbo.ROLES r2 ON r2.ROLE_ID = ur2.ROLE_ID AND r2.CLIENT_ID = u.CLIENT_ID
        WHERE ur2.USER_ID = u.USER_ID
    ) AS rn
    WHERE u.CLIENT_ID = @ClientId
    ORDER BY u.USER_ID DESC;
END
