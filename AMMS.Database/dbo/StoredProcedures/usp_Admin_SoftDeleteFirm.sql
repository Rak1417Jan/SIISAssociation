CREATE PROCEDURE [dbo].[usp_Admin_SoftDeleteFirm]
    @ClientId INT,
    @FirmId INT,
    @ModifiedBy INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.COMPANY_MASTER WHERE COMPANY_ID = @FirmId AND CLIENT_ID = @ClientId)
    BEGIN
        RAISERROR('Firm not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.MEMBERS m WHERE m.COMPANY_ID = @FirmId AND m.CLIENT_ID = @ClientId AND m.IS_ACTIVE = 1)
    BEGIN
        RAISERROR('Firm has active members linked.', 16, 1);
        RETURN;
    END

    UPDATE dbo.COMPANY_MASTER
    SET IS_ACTIVE = 0,
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ModifiedBy
    WHERE COMPANY_ID = @FirmId
      AND CLIENT_ID = @ClientId;
END
