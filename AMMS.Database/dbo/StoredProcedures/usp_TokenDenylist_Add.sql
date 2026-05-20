CREATE PROCEDURE [dbo].[usp_TokenDenylist_Add]
    @UserId INT,
    @Jti NVARCHAR(100),
    @Reason NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TOKEN_DENYLIST (USER_ID, JTI, REASON)
    VALUES (@UserId, @Jti, @Reason);
END

