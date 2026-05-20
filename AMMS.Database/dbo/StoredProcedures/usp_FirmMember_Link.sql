CREATE PROCEDURE [dbo].[usp_FirmMember_Link]
    @ClientId INT,
    @FirmId INT,
    @MemberId INT,
    @RoleInFirm NVARCHAR(50),
    @LinkedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.COMPANY_MASTER WHERE COMPANY_ID = @FirmId AND CLIENT_ID = @ClientId AND IS_ACTIVE = 1)
    BEGIN
        RAISERROR('Firm not found or inactive.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.MEMBERS WHERE MEMBER_ID = @MemberId AND CLIENT_ID = @ClientId AND IS_ACTIVE = 1)
    BEGIN
        RAISERROR('Member not found or inactive.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.FIRM_MEMBERS WHERE COMPANY_ID = @FirmId AND MEMBER_ID = @MemberId AND IS_ACTIVE = 1)
    BEGIN
        RAISERROR('Member already linked to firm.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.FIRM_MEMBERS (COMPANY_ID, MEMBER_ID, ROLE_IN_FIRM, IS_ACTIVE, LINKED_BY, LINKED_AT, CREATED_BY)
    VALUES (@FirmId, @MemberId, @RoleInFirm, 1, @LinkedBy, GETDATE(), @LinkedBy);
END
