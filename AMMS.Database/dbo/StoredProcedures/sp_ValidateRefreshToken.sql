CREATE PROCEDURE [dbo].[sp_ValidateRefreshToken]
    @REFRESH_TOKEN NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@REFRESH_TOKEN IS NULL OR LEN(LTRIM(RTRIM(@REFRESH_TOKEN))) = 0)
        RETURN;

    SELECT
        u.USER_ID       AS UserId,
        u.CLIENT_ID     AS ClientId,
        u.USERNAME      AS Username,
        u.PASSWORD_HASH AS PasswordHash,
        u.PASSWORD_SALT AS PasswordSalt,
        FullName        = ISNULL(NULLIF(LTRIM(RTRIM(u.FULL_NAME)), N''), u.USERNAME),
        [Role]          = COALESCE(rx.ROLE_NAME, N'User')
    FROM dbo.USER_REFRESH_TOKENS AS rt
    INNER JOIN dbo.USERS AS u
        ON u.USER_ID = rt.USER_ID
       AND ISNULL(u.IS_ACTIVE, 1) = 1
    OUTER APPLY (
        SELECT TOP (1)
            r.ROLE_NAME
        FROM dbo.USER_ROLES AS ur
        INNER JOIN dbo.ROLES AS r
            ON r.ROLE_ID = ur.ROLE_ID
           AND r.CLIENT_ID = u.CLIENT_ID
        WHERE ur.USER_ID = u.USER_ID
        ORDER BY
            CASE LOWER(REPLACE(REPLACE(LTRIM(RTRIM(r.ROLE_NAME)), N'_', N' '), N'-', N' '))
                WHEN N'superadmin' THEN 100
                WHEN N'super admin' THEN 100
                WHEN N'admin' THEN 90
                WHEN N'manager' THEN 80
                WHEN N'finance' THEN 70
                WHEN N'support' THEN 60
                ELSE 50
            END DESC,
            r.ROLE_NAME ASC
    ) AS rx
    WHERE rt.REFRESH_TOKEN = LTRIM(RTRIM(@REFRESH_TOKEN))
      AND rt.EXPIRES_ON > SYSUTCDATETIME();
END
