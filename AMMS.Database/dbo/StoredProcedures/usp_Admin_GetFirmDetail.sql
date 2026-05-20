CREATE PROCEDURE [dbo].[usp_Admin_GetFirmDetail]
    @ClientId INT,
    @FirmId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.COMPANY_ID AS FirmId,
        c.COMPANY_NAME AS [Name],
        ISNULL(c.GST_NUMBER, '') AS GstNo,
        ISNULL(c.ADDRESS, '') AS Address,
        ISNULL(c.CITY, '') AS City,
        ISNULL(c.STATE, '') AS State,
        ISNULL(c.PIN_CODE, '') AS PinCode,
        CAST(ISNULL(c.COMPANY_TYPE_ID, 0) AS int) AS CompanyTypeId,
        ISNULL(ct.NAME, '') AS CompanyTypeName,
        ISNULL(ct.CODE, '') AS CompanyTypeCode,
        ISNULL(c.COMPANY_CODE, '') AS CompanyCode,
        c.DATE_OF_ESTABLISHMENT AS DateOfEstablishment,
        ISNULL(c.REG_NO, '') AS RegNo,
        ISNULL(c.TELEPHONE_NO, '') AS TelephoneNo,
        ISNULL(c.MOBILE, '') AS Mobile,
        ISNULL(c.WEBSITE, '') AS Website,
        ISNULL(c.PRODUCTS, '') AS Products,
        CAST(ISNULL(c.IS_ACTIVE, 0) AS bit) AS IsActive,
        ISNULL(c.CREATED_DATE, GETDATE()) AS CreatedDate
    FROM dbo.COMPANY_MASTER c
    LEFT JOIN dbo.COMPANY_TYPE ct ON ct.COMPANY_TYPE_ID = c.COMPANY_TYPE_ID
    WHERE c.COMPANY_ID = @FirmId
      AND c.CLIENT_ID = @ClientId;
END
