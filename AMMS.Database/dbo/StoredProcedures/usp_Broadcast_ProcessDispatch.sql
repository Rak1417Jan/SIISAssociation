CREATE PROCEDURE [dbo].[usp_Broadcast_ProcessDispatch]
    @BroadcastId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @scheduledAt DATETIME2 (7);
    DECLARE @sentAt DATETIME2 (7);
    DECLARE @isDeleted BIT;

    SELECT @scheduledAt = b.SCHEDULED_AT, @sentAt = b.SENT_AT, @isDeleted = b.IS_DELETED
    FROM dbo.BROADCASTS b
    WHERE b.BROADCAST_ID = @BroadcastId;

    IF (@isDeleted IS NULL)
    BEGIN
        RAISERROR('Broadcast not found.', 16, 1);
        RETURN;
    END

    IF (@isDeleted = 1)
    BEGIN
        RETURN;
    END

    IF (@sentAt IS NOT NULL)
    BEGIN
        RETURN;
    END

    IF (@scheduledAt IS NOT NULL AND @scheduledAt > SYSUTCDATETIME())
    BEGIN
        RETURN;
    END

    INSERT INTO dbo.NOTIFICATIONS (MEMBER_ID, TITLE, MESSAGE, [TYPE], CATEGORY, LINK_TO, IS_READ, CREATED_BY, CREATED_DATE)
    SELECT
        m.MEMBER_ID,
        b.TITLE,
        b.MESSAGE,
        N'BROADCAST',
        N'BROADCAST',
        NULL,
        0,
        b.CREATED_BY,
        SYSUTCDATETIME()
    FROM dbo.BROADCASTS b
    INNER JOIN dbo.MEMBERS m ON m.IS_ACTIVE = 1 AND m.CLIENT_ID = b.CLIENT_ID
    WHERE b.BROADCAST_ID = @BroadcastId;

    DECLARE @cnt INT = @@ROWCOUNT;

    UPDATE dbo.BROADCASTS
    SET SENT_AT = SYSUTCDATETIME(),
        RECIPIENT_COUNT = @cnt,
        DELIVERED_COUNT = @cnt,
        FAILED_COUNT = 0,
        MODIFIED_DATE = SYSUTCDATETIME()
    WHERE BROADCAST_ID = @BroadcastId;
END
