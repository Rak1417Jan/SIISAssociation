CREATE PROCEDURE [dbo].[usp_Admin_VerifyMember]
    @ClientId INT,
    @MemberId INT,
    @Notes NVARCHAR(500) = NULL,
    @ChangedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

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

    IF (@oldStatus = 'VERIFIED')
    BEGIN
        -- idempotent
        RETURN;
    END

    DECLARE @yyyy NVARCHAR(4) = CONVERT(NVARCHAR(4), YEAR(GETDATE()));
    DECLARE @seq INT;

    SELECT @seq = ISNULL(MAX(CAST(RIGHT(MEMBERSHIP_ID, 4) AS INT)), 0) + 1
    FROM dbo.MEMBERS
    WHERE CLIENT_ID = @ClientId
      AND MEMBERSHIP_ID LIKE 'AMMS-' + @yyyy + '-%';

    DECLARE @membershipId NVARCHAR(30) = 'AMMS-' + @yyyy + '-' + RIGHT('0000' + CAST(@seq AS NVARCHAR(10)), 4);

    UPDATE dbo.MEMBERS
    SET MEMBERSHIP_ID = ISNULL(NULLIF(MEMBERSHIP_ID,''), @membershipId),
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ChangedBy
    WHERE MEMBER_ID = @MemberId
      AND CLIENT_ID = @ClientId;

    UPDATE dbo.MEMBER_APPLICATIONS
    SET STATUS = 'VERIFIED',
        REMARKS = @Notes,
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ChangedBy
    WHERE APPLICATION_ID = @applicationId
      AND CLIENT_ID = @ClientId;

    INSERT INTO dbo.MEMBER_APPLICATION_STATUS_HISTORY (APPLICATION_ID, OLD_STATUS, NEW_STATUS, CHANGED_BY, CHANGED_DATE, CREATED_BY)
    VALUES (@applicationId, @oldStatus, 'VERIFIED', @ChangedBy, GETDATE(), @ChangedBy);
END
