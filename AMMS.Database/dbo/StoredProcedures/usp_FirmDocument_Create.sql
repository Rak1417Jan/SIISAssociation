CREATE PROCEDURE [dbo].[usp_FirmDocument_Create]
    @ClientId INT,
    @FirmId INT,
    @DocumentType NVARCHAR(50),
    @BlobUrl NVARCHAR(500),
    @UploadedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.COMPANY_MASTER WHERE COMPANY_ID = @FirmId AND CLIENT_ID = @ClientId)
    BEGIN
        RAISERROR('Firm not found.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.FIRM_DOCUMENTS (COMPANY_ID, DOCUMENT_TYPE, BLOB_URL, UPLOADED_BY, UPLOADED_AT, CREATED_BY)
    VALUES (@FirmId, @DocumentType, @BlobUrl, @UploadedBy, GETDATE(), @UploadedBy);
END
