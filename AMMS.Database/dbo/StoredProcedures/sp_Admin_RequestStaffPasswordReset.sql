CREATE PROCEDURE [dbo].[sp_Admin_RequestStaffPasswordReset]
    @ClientId INT,
    @Email NVARCHAR(150) = NULL,
    @Username NVARCHAR(100) = NULL,
    @TokenHash VARBINARY(32),
    @ExpiresAt DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@ClientId IS NULL OR @ClientId < 1)
    BEGIN
        RAISERROR('Client is required.', 16, 1);
        RETURN;
    END

    IF ((@Email IS NULL OR LEN(LTRIM(RTRIM(@Email))) = 0)
        AND (@Username IS NULL OR LEN(LTRIM(RTRIM(@Username))) = 0))
    BEGIN
        RAISERROR('Email or username is required.', 16, 1);
        RETURN;
    END

    DECLARE @UserId INT;

    SELECT @UserId = u.USER_ID
    FROM dbo.USERS AS u
    WHERE u.CLIENT_ID = @ClientId
      AND ISNULL(u.IS_ACTIVE, 1) = 1
      AND (
          (@Email IS NOT NULL AND LEN(LTRIM(RTRIM(@Email))) > 0 AND u.EMAIL = LTRIM(RTRIM(@Email)))
          OR (@Username IS NOT NULL AND LEN(LTRIM(RTRIM(@Username))) > 0 AND u.USERNAME = LTRIM(RTRIM(@Username)))
      );

    IF @UserId IS NULL
    BEGIN
        SELECT CAST(0 AS bit) AS Issued, CAST(NULL AS NVARCHAR(150)) AS Email;
        RETURN;
    END

    UPDATE dbo.STAFF_PASSWORD_RESET
    SET USED_AT = SYSUTCDATETIME()
    WHERE USER_ID = @UserId
      AND USED_AT IS NULL;

    INSERT INTO dbo.STAFF_PASSWORD_RESET (USER_ID, CLIENT_ID, TOKEN_HASH, EXPIRES_AT)
    VALUES (@UserId, @ClientId, @TokenHash, @ExpiresAt);

    SELECT CAST(1 AS bit) AS Issued, u.EMAIL AS Email
    FROM dbo.USERS AS u
    WHERE u.USER_ID = @UserId;
END
