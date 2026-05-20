CREATE PROCEDURE [dbo].[usp_Admin_RejectMember]
    @ClientId INT,
    @MemberId INT,
    @Feedback NVARCHAR(500),
    @ChangedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Feedback IS NULL OR LEN(LTRIM(RTRIM(@Feedback))) = 0)
    BEGIN
        RAISERROR('feedback is mandatory.', 16, 1);
        RETURN;
    END

    DECLARE @applicationId INT;
    DECLARE @oldStatus NVARCHAR(50);

    SELECT @applicationId = m.APPLICATION_ID
    FROM dbo.MEMBERS m
    WHERE m.MEMBER_ID = @MemberId
      AND m.CLIENT_ID = @ClientId;

    IF (@applicationId IS NULL)
    BEGIN
        RAISERROR('Member application not found.', 16, 1);
        RETURN;
    END

    SELECT @oldStatus = a.STATUS
    FROM dbo.MEMBER_APPLICATIONS a
    WHERE a.APPLICATION_ID = @applicationId
      AND a.CLIENT_ID = @ClientId;

    IF (@oldStatus IS NULL)
    BEGIN
        RAISERROR('Member application not found.', 16, 1);
        RETURN;
    END

    UPDATE dbo.MEMBER_APPLICATIONS
    SET STATUS = 'REJECTED',
        REMARKS = @Feedback,
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ChangedBy
    WHERE APPLICATION_ID = @applicationId
      AND CLIENT_ID = @ClientId;

    INSERT INTO dbo.MEMBER_APPLICATION_STATUS_HISTORY (APPLICATION_ID, OLD_STATUS, NEW_STATUS, CHANGED_BY, CHANGED_DATE, CREATED_BY)
    VALUES (@applicationId, @oldStatus, 'REJECTED', @ChangedBy, GETDATE(), @ChangedBy);
END
