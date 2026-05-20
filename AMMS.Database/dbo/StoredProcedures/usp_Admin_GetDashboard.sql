CREATE PROCEDURE [dbo].[usp_Admin_GetDashboard]
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @startOfYear DATE = DATEFROMPARTS(YEAR(GETDATE()), 1, 1);

    SELECT
        totalMembers = (SELECT COUNT(1) FROM dbo.MEMBERS m WHERE m.CLIENT_ID = @ClientId),
        activeMembers = (SELECT COUNT(1) FROM dbo.MEMBERS m WHERE m.CLIENT_ID = @ClientId AND m.IS_ACTIVE = 1),
        inactiveMembers = (SELECT COUNT(1) FROM dbo.MEMBERS m WHERE m.CLIENT_ID = @ClientId AND m.IS_ACTIVE = 0),
        pendingApplications = (SELECT COUNT(1) FROM dbo.MEMBER_APPLICATIONS a WHERE a.CLIENT_ID = @ClientId AND a.STATUS = 'PENDING'),
        onHoldApplications = (SELECT COUNT(1) FROM dbo.MEMBER_APPLICATIONS a WHERE a.CLIENT_ID = @ClientId AND a.STATUS = 'ON_HOLD'),
        rejectedApplications = (SELECT COUNT(1) FROM dbo.MEMBER_APPLICATIONS a WHERE a.CLIENT_ID = @ClientId AND a.STATUS = 'REJECTED'),
        currentYearRevenue = (
            SELECT ISNULL(SUM(p.AMOUNT), 0.00)
            FROM dbo.PAYMENTS p
            INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID AND m.CLIENT_ID = @ClientId
            WHERE p.PAID_DATE >= @startOfYear AND p.PAYMENT_STATUS = 'SUCCESS'
        );

    ;WITH last7 AS
    (
        SELECT CAST(DATEADD(DAY, -v.n, CAST(GETDATE() AS date)) AS date) AS [Date]
        FROM (VALUES (0),(1),(2),(3),(4),(5),(6)) v(n)
    )
    SELECT
        [Date] = l.[Date],
        [Count] = ISNULL(x.Cnt, 0)
    FROM last7 l
    OUTER APPLY
    (
        SELECT COUNT(1) AS Cnt
        FROM dbo.MEMBER_APPLICATIONS a
        WHERE a.CLIENT_ID = @ClientId
          AND CAST(a.SUBMITTED_DATE AS date) = l.[Date]
    ) x
    ORDER BY l.[Date] ASC;
END
