CREATE PROCEDURE [dbo].[usp_GetCompanyTypes]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COMPANY_TYPE_ID AS CompanyTypeId,
        NAME AS Name,
        CODE AS Code,
        CREATED_DATE AS CreatedDate,
        MODIFIED_DATE AS ModifiedDate
    FROM dbo.COMPANY_TYPE
    ORDER BY NAME ASC;
END
