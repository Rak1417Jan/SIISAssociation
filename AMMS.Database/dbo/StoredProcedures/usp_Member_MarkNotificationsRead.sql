CREATE PROCEDURE [dbo].[usp_Member_MarkNotificationsRead]
    @MemberId INT,
    @NotificationIds NVARCHAR (MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@NotificationIds IS NULL OR LEN(LTRIM(RTRIM(@NotificationIds))) = 0)
    BEGIN
        UPDATE dbo.NOTIFICATIONS
        SET IS_READ = 1,
            MODIFIED_DATE = SYSUTCDATETIME()
        WHERE MEMBER_ID = @MemberId AND IS_READ = 0;
        RETURN;
    END

    UPDATE n
    SET n.IS_READ = 1,
        n.MODIFIED_DATE = SYSUTCDATETIME()
    FROM dbo.NOTIFICATIONS n
    INNER JOIN STRING_SPLIT(@NotificationIds, ',') s ON TRY_CAST(LTRIM(RTRIM(s.[value])) AS INT) = n.NOTIFICATION_ID
    WHERE n.MEMBER_ID = @MemberId;
END
