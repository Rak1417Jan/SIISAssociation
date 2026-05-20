CREATE PROCEDURE [dbo].[usp_TokenDenylist_IsDenied]
    @UserId INT,
    @Jti NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IsDenied = CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.TOKEN_DENYLIST WHERE USER_ID = @UserId AND (JTI = @Jti OR JTI = '*')
    ) THEN 1 ELSE 0 END AS bit);
END

