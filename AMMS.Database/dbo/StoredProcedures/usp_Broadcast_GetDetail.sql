CREATE PROCEDURE [dbo].[usp_Broadcast_GetDetail]
    @ClientId INT,
    @BroadcastId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.BROADCAST_ID AS BroadcastId,
        b.TITLE AS Title,
        b.MESSAGE AS Message,
        b.CHANNEL AS Channel,
        b.TARGET_FILTER AS TargetFilter,
        b.SCHEDULED_AT AS ScheduledAt,
        b.SENT_AT AS SentAt,
        b.CREATED_BY AS CreatedBy,
        b.RECIPIENT_COUNT AS RecipientCount,
        b.DELIVERED_COUNT AS DeliveredCount,
        b.FAILED_COUNT AS FailedCount,
        b.CREATED_DATE AS CreatedDate
    FROM dbo.BROADCASTS b
    WHERE b.BROADCAST_ID = @BroadcastId
      AND b.CLIENT_ID = @ClientId
      AND b.IS_DELETED = 0;
END
