CREATE PROCEDURE [dbo].[usp_Admin_UpdateFirm]
    @ClientId INT,
    @FirmId INT,
    @Name NVARCHAR(200),
    @CompanyTypeId INT = NULL,
    @GstNo NVARCHAR(50) = NULL,
    @CompanyCode NVARCHAR(50) = NULL,
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @PinCode NVARCHAR(20) = NULL,
    @DateOfEstablishment DATE = NULL,
    @RegNo NVARCHAR(100) = NULL,
    @TelephoneNo NVARCHAR(30) = NULL,
    @Mobile NVARCHAR(20) = NULL,
    @Website NVARCHAR(500) = NULL,
    @Products NVARCHAR(MAX) = NULL,
    @IsActive BIT = NULL,
    @ModifiedBy INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.COMPANY_MASTER WHERE COMPANY_ID = @FirmId AND CLIENT_ID = @ClientId)
    BEGIN
        RAISERROR('Firm not found.', 16, 1);
        RETURN;
    END

    IF (@Name IS NULL OR LEN(LTRIM(RTRIM(@Name))) = 0)
    BEGIN
        RAISERROR('Firm name is required.', 16, 1);
        RETURN;
    END

    IF (@CompanyTypeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.COMPANY_TYPE WHERE COMPANY_TYPE_ID = @CompanyTypeId))
    BEGIN
        RAISERROR('Invalid company type.', 16, 1);
        RETURN;
    END

    IF (@GstNo IS NOT NULL AND LEN(LTRIM(RTRIM(@GstNo))) <> 15)
    BEGIN
        RAISERROR('GST number must be 15 characters.', 16, 1);
        RETURN;
    END

    IF (@GstNo IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.COMPANY_MASTER WHERE CLIENT_ID = @ClientId AND GST_NUMBER = @GstNo AND COMPANY_ID <> @FirmId))
    BEGIN
        RAISERROR('GST number already exists.', 16, 1);
        RETURN;
    END

    IF (@CompanyCode IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.COMPANY_MASTER WHERE CLIENT_ID = @ClientId AND COMPANY_CODE = @CompanyCode AND COMPANY_ID <> @FirmId))
    BEGIN
        RAISERROR('Company code already exists.', 16, 1);
        RETURN;
    END

    UPDATE dbo.COMPANY_MASTER
    SET COMPANY_NAME = @Name,
        COMPANY_TYPE_ID = COALESCE(@CompanyTypeId, COMPANY_TYPE_ID),
        COMPANY_CODE = CASE WHEN @CompanyCode IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@CompanyCode)), N'') ELSE COMPANY_CODE END,
        GST_NUMBER = CASE WHEN @GstNo IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@GstNo)), N'') ELSE GST_NUMBER END,
        ADDRESS = CASE WHEN @Address IS NOT NULL THEN @Address ELSE ADDRESS END,
        CITY = CASE WHEN @City IS NOT NULL THEN @City ELSE CITY END,
        STATE = CASE WHEN @State IS NOT NULL THEN @State ELSE STATE END,
        PIN_CODE = CASE WHEN @PinCode IS NOT NULL THEN @PinCode ELSE PIN_CODE END,
        DATE_OF_ESTABLISHMENT = CASE WHEN @DateOfEstablishment IS NOT NULL THEN @DateOfEstablishment ELSE DATE_OF_ESTABLISHMENT END,
        REG_NO = CASE WHEN @RegNo IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@RegNo)), N'') ELSE REG_NO END,
        TELEPHONE_NO = CASE WHEN @TelephoneNo IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@TelephoneNo)), N'') ELSE TELEPHONE_NO END,
        MOBILE = CASE WHEN @Mobile IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@Mobile)), N'') ELSE MOBILE END,
        WEBSITE = CASE WHEN @Website IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@Website)), N'') ELSE WEBSITE END,
        PRODUCTS = CASE WHEN @Products IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@Products)), N'') ELSE PRODUCTS END,
        IS_ACTIVE = ISNULL(@IsActive, IS_ACTIVE),
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ModifiedBy
    WHERE COMPANY_ID = @FirmId
      AND CLIENT_ID = @ClientId;
END
