CREATE PROCEDURE [dbo].[usp_Admin_DeactivateStaff]
    @ClientId INT,
    @UserId INT,
    @ModifiedBy INT = NULL
AS
BEGIN
    UPDATE dbo.USERS
    SET IS_ACTIVE = 0,
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ModifiedBy
    WHERE USER_ID = @UserId
      AND CLIENT_ID = @ClientId;

    IF (@@ROWCOUNT = 0)
    BEGIN
        RAISERROR('Staff not found.', 16, 1);
        RETURN;
    END
END
