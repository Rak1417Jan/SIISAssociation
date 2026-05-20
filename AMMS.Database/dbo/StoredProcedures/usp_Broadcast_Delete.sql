CREATE PROCEDURE [dbo].[usp_Broadcast_Delete]
    @ClientId INT,
    @BroadcastId INT,
    @ModifiedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.BROADCASTS
    SET IS_DELETED = 1,
        MODIFIED_DATE = SYSUTCDATETIME(),
        MODIFIED_BY = @ModifiedBy
    WHERE BROADCAST_ID = @BroadcastId
      AND CLIENT_ID = @ClientId
      AND IS_DELETED = 0;

    IF (@@ROWCOUNT = 0)
    BEGIN
        RAISERROR('Broadcast not found.', 16, 1);
        RETURN;
    END
END
