CREATE PROCEDURE [dbo].[usp_Member_GetNotifications]
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (50)
        n.NOTIFICATION_ID AS NotificationId,
        n.TITLE AS Title,
        n.MESSAGE AS Message,
        n.CATEGORY AS Category,
        n.LINK_TO AS LinkTo,
        n.IS_READ AS IsRead,
        n.CREATED_DATE AS CreatedAt
    FROM dbo.NOTIFICATIONS n
    WHERE n.MEMBER_ID = @MemberId
    ORDER BY n.CREATED_DATE DESC;

    SELECT UnreadCount = COUNT(1)
    FROM dbo.NOTIFICATIONS n
    WHERE n.MEMBER_ID = @MemberId AND n.IS_READ = 0;
END
