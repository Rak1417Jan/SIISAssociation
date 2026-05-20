CREATE PROCEDURE [dbo].[usp_Admin_GetBroadcasts]
    @ClientId INT,
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page < 1) SET @Page = 1;
    IF (@PageSize < 1) SET @PageSize = 20;
    IF (@PageSize > 100) SET @PageSize = 100;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    ;WITH Brd AS
    (
        SELECT
            b.BROADCAST_ID AS BroadcastId,
            b.TITLE AS Title,
            b.CHANNEL AS Channel,
            b.SENT_AT AS SentAt,
            b.SCHEDULED_AT AS ScheduledAt,
            b.RECIPIENT_COUNT AS RecipientCount,
            b.DELIVERED_COUNT AS DeliveredCount,
            b.FAILED_COUNT AS FailedCount,
            b.CREATED_DATE AS CreatedDate
        FROM dbo.BROADCASTS b
        WHERE b.CLIENT_ID = @ClientId
          AND b.IS_DELETED = 0
    )
    SELECT
        x.*,
        Total = (SELECT COUNT(1) FROM Brd)
    FROM Brd x
    ORDER BY x.CreatedDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
