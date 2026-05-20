CREATE PROCEDURE [dbo].[usp_Admin_GetMembers]
    @ClientId INT,
    @Page INT = 1,
    @PageSize INT = 20,
    @Status NVARCHAR(50) = NULL,
    @FirmId INT = NULL,
    @PlanId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @SortBy NVARCHAR(50) = NULL,
    @SortOrder NVARCHAR(10) = NULL
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
            m.MEMBER_ID AS MemberId,
            ISNULL(m.MEMBERSHIP_ID, '') AS MembershipId,
            ISNULL(m.OWNER_NAME, '') AS OwnerName,
            ISNULL(m.MOBILE_NUMBER, '') AS MobileNumber,
            ISNULL(m.EMAIL, '') AS Email,
            m.COMPANY_ID AS CompanyId,
            c.COMPANY_NAME AS CompanyName,
            CAST(ISNULL(m.IS_ACTIVE, 0) AS bit) AS IsActive,
            ISNULL(m.CREATED_DATE, GETDATE()) AS CreatedDate,
            ISNULL(a.STATUS, '') AS ApplicationStatus,
            s.PLAN_ID AS PlanId
        FROM dbo.MEMBERS m
        INNER JOIN dbo.COMPANY_MASTER c ON c.COMPANY_ID = m.COMPANY_ID AND c.CLIENT_ID = @ClientId
        LEFT JOIN dbo.MEMBER_APPLICATIONS a ON a.APPLICATION_ID = m.APPLICATION_ID
        LEFT JOIN dbo.SUBSCRIPTIONS s ON s.MEMBER_ID = m.MEMBER_ID AND s.STATUS = 'ACTIVE'
        WHERE m.CLIENT_ID = @ClientId
          AND (@FirmId IS NULL OR m.COMPANY_ID = @FirmId)
          AND (@PlanId IS NULL OR s.PLAN_ID = @PlanId)
          AND (@Status IS NULL OR a.STATUS = @Status)
          AND (@Search IS NULL OR @Search = '' OR
               m.OWNER_NAME LIKE '%' + @Search + '%' OR
               m.MOBILE_NUMBER LIKE '%' + @Search + '%' OR
               m.EMAIL LIKE '%' + @Search + '%' OR
               m.MEMBERSHIP_ID LIKE '%' + @Search + '%')
          AND (@DateFrom IS NULL OR CAST(m.CREATED_DATE AS date) >= @DateFrom)
          AND (@DateTo IS NULL OR CAST(m.CREATED_DATE AS date) <= @DateTo)
    )
    SELECT
        b.MemberId,
        b.MembershipId,
        b.OwnerName,
        b.MobileNumber,
        b.Email,
        b.CompanyId,
        b.CompanyName,
        b.IsActive,
        b.CreatedDate,
        Total = (SELECT COUNT(1) FROM base)
    FROM base b
    ORDER BY
        CASE WHEN ISNULL(@SortBy,'') = 'createdDate' AND ISNULL(@SortOrder,'asc') = 'asc' THEN b.CreatedDate END ASC,
        CASE WHEN ISNULL(@SortBy,'') = 'createdDate' AND ISNULL(@SortOrder,'asc') = 'desc' THEN b.CreatedDate END DESC,
        b.MemberId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
