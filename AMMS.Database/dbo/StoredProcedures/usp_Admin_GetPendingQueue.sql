CREATE PROCEDURE [dbo].[usp_Admin_GetPendingQueue]
    @ClientId INT,
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page < 1) SET @Page = 1;
    IF (@PageSize < 1) SET @PageSize = 20;
    IF (@PageSize > 100) SET @PageSize = 100;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    ;WITH q AS
    (
        SELECT
            a.APPLICATION_ID AS ApplicationId,
            a.OWNER_NAME AS OwnerName,
            a.MOBILE_NUMBER AS MobileNumber,
            a.CREATED_DATE AS CreatedDate,
            a.STATUS AS Status,
            CAST(CASE WHEN a.STATUS = 'ON_HOLD' AND a.MODIFIED_DATE < DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END AS bit) AS IsOnHoldOver7Days
        FROM dbo.MEMBER_APPLICATIONS a
        WHERE a.CLIENT_ID = @ClientId
          AND a.STATUS IN ('PENDING','ON_HOLD')
    )
    SELECT
        q.*,
        Total = (SELECT COUNT(1) FROM q)
    FROM q
    ORDER BY q.CreatedDate ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
