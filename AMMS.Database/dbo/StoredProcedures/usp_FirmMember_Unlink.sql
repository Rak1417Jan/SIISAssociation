CREATE PROCEDURE [dbo].[usp_FirmMember_Unlink]
    @ClientId INT,
    @FirmId INT,
    @MemberId INT,
    @UnlinkedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE fm
    SET fm.IS_ACTIVE = 0,
        fm.MODIFIED_DATE = GETDATE(),
        fm.MODIFIED_BY = @UnlinkedBy
    FROM dbo.FIRM_MEMBERS fm
    INNER JOIN dbo.COMPANY_MASTER c ON c.COMPANY_ID = fm.COMPANY_ID AND c.CLIENT_ID = @ClientId
    WHERE fm.COMPANY_ID = @FirmId AND fm.MEMBER_ID = @MemberId AND fm.IS_ACTIVE = 1;

    IF (@@ROWCOUNT = 0)
    BEGIN
        RAISERROR('Link not found.', 16, 1);
        RETURN;
    END
END
