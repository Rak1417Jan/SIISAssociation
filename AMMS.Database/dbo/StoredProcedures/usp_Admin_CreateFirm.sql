CREATE PROCEDURE [dbo].[usp_Admin_CreateFirm]
    @ClientId INT,
    @Name NVARCHAR(200),
    @CompanyTypeId INT,
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
    @CreatedBy INT = NULL
AS
BEGIN
    IF (@Name IS NULL OR LEN(LTRIM(RTRIM(@Name))) = 0)
    BEGIN
        RAISERROR('Firm name is required.', 16, 1);
        RETURN;
    END

    IF (@CompanyTypeId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.COMPANY_TYPE WHERE COMPANY_TYPE_ID = @CompanyTypeId))
    BEGIN
        RAISERROR('Valid company type is required.', 16, 1);
        RETURN;
    END

    IF (@GstNo IS NOT NULL AND LEN(LTRIM(RTRIM(@GstNo))) <> 15)
    BEGIN
        RAISERROR('GST number must be 15 characters.', 16, 1);
        RETURN;
    END

    IF (@GstNo IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.COMPANY_MASTER WHERE CLIENT_ID = @ClientId AND GST_NUMBER = @GstNo))
    BEGIN
        RAISERROR('GST number already exists.', 16, 1);
        RETURN;
    END

    IF (@CompanyCode IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.COMPANY_MASTER WHERE CLIENT_ID = @ClientId AND COMPANY_CODE = @CompanyCode))
    BEGIN
        RAISERROR('Company code already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.COMPANY_MASTER (
        CLIENT_ID, COMPANY_TYPE_ID, COMPANY_CODE, COMPANY_NAME, GST_NUMBER,
        ADDRESS, CITY, STATE, PIN_CODE,
        DATE_OF_ESTABLISHMENT, REG_NO, TELEPHONE_NO, MOBILE, WEBSITE, PRODUCTS,
        IS_ACTIVE, CREATED_BY)
    VALUES (
        @ClientId, @CompanyTypeId,
        NULLIF(LTRIM(RTRIM(@CompanyCode)), N''),
        @Name,
        NULLIF(LTRIM(RTRIM(@GstNo)), N''),
        @Address, @City, @State, @PinCode,
        @DateOfEstablishment,
        NULLIF(LTRIM(RTRIM(@RegNo)), N''),
        NULLIF(LTRIM(RTRIM(@TelephoneNo)), N''),
        NULLIF(LTRIM(RTRIM(@Mobile)), N''),
        NULLIF(LTRIM(RTRIM(@Website)), N''),
        NULLIF(LTRIM(RTRIM(@Products)), N''),
        1, @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
