CREATE PROCEDURE [dbo].[usp_Admin_GetFirms]
    @ClientId INT,
    @Page INT = 1,
    @PageSize INT = 20,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page < 1) SET @Page = 1;
    IF (@PageSize < 1) SET @PageSize = 20;
    IF (@PageSize > 100) SET @PageSize = 100;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    ;WITH base AS
    (
        SELECT
            c.COMPANY_ID AS FirmId,
            c.COMPANY_NAME AS [Name],
            ISNULL(c.GST_NUMBER, '') AS GstNo,
            ISNULL(c.CITY, '') AS City,
            ISNULL(c.COMPANY_CODE, '') AS CompanyCode,
            CAST(ISNULL(c.COMPANY_TYPE_ID, 0) AS int) AS CompanyTypeId,
            ISNULL(ct.NAME, '') AS CompanyTypeName,
            ISNULL(c.REG_NO, '') AS RegNo,
            CAST(ISNULL(c.IS_ACTIVE, 0) AS bit) AS IsActive,
            ISNULL(c.CREATED_DATE, GETDATE()) AS CreatedDate
        FROM dbo.COMPANY_MASTER c
        LEFT JOIN dbo.COMPANY_TYPE ct ON ct.COMPANY_TYPE_ID = c.COMPANY_TYPE_ID
        WHERE c.CLIENT_ID = @ClientId
          AND (@Search IS NULL OR @Search = ''
               OR c.COMPANY_NAME LIKE '%' + @Search + '%'
               OR c.GST_NUMBER LIKE '%' + @Search + '%'
               OR c.COMPANY_CODE LIKE '%' + @Search + '%'
               OR c.REG_NO LIKE '%' + @Search + '%')
    )
    SELECT
        b.*,
        Total = (SELECT COUNT(1) FROM base)
    FROM base b
    ORDER BY b.CreatedDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
