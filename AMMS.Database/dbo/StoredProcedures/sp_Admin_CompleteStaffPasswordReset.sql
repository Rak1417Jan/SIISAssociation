CREATE PROCEDURE [dbo].[sp_Admin_CompleteStaffPasswordReset]
    @TokenHash VARBINARY(32),
    @PasswordHash VARBINARY(500),
    @PasswordSalt VARBINARY(256),
    @ModifiedBy INT = NULL
AS
BEGIN
    DECLARE @ResetId INT;
    DECLARE @UserId INT;

    SELECT TOP (1)
        @ResetId = r.RESET_ID,
        @UserId = r.USER_ID
    FROM dbo.STAFF_PASSWORD_RESET AS r
    WHERE r.TOKEN_HASH = @TokenHash
      AND r.USED_AT IS NULL
      AND r.EXPIRES_AT > SYSUTCDATETIME()
    ORDER BY r.RESET_ID DESC;

    IF @ResetId IS NULL OR @UserId IS NULL
    BEGIN
        RAISERROR('Invalid or expired reset token.', 16, 1);
        RETURN;
    END

    UPDATE dbo.USERS
    SET PASSWORD_HASH = @PasswordHash,
        PASSWORD_SALT = @PasswordSalt,
        MUST_CHANGE_PASSWORD = 0,
        IS_FIRST_LOGIN = 0,
        MODIFIED_DATE = SYSUTCDATETIME(),
        MODIFIED_BY = @ModifiedBy
    WHERE USER_ID = @UserId
      AND ISNULL(IS_ACTIVE, 1) = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('User not found or inactive.', 16, 1);
        RETURN;
    END

    UPDATE dbo.STAFF_PASSWORD_RESET
    SET USED_AT = SYSUTCDATETIME()
    WHERE RESET_ID = @ResetId;
END
