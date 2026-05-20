CREATE PROCEDURE [dbo].[sp_ClearUserRefreshTokens]
    @USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF (@USER_ID IS NULL OR @USER_ID < 1)
    BEGIN
        RAISERROR(N'User is required.', 16, 1);
        RETURN;
    END

    DELETE FROM dbo.USER_REFRESH_TOKENS
    WHERE USER_ID = @USER_ID;
END
