CREATE PROCEDURE [dbo].[usp_Admin_GetAnalytics]
    @ClientId INT,
    @Year INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @y INT = ISNULL(@Year, YEAR(GETDATE()));
    DECLARE @startOfYear DATE = DATEFROMPARTS(@y, 1, 1);
    DECLARE @endOfYear DATE = DATEFROMPARTS(@y + 1, 1, 1);

    -- 1) membershipGrowth[]
    SELECT
        [Month] = MONTH(a.SUBMITTED_DATE),
        newMembers = COUNT(1)
    FROM dbo.MEMBER_APPLICATIONS a
    WHERE a.CLIENT_ID = @ClientId
      AND a.SUBMITTED_DATE >= @startOfYear
      AND a.SUBMITTED_DATE < @endOfYear
      AND a.STATUS = 'VERIFIED'
    GROUP BY MONTH(a.SUBMITTED_DATE)
    ORDER BY [Month];

    -- 2) monthlyRevenue[]
    SELECT
        [Month] = MONTH(p.PAID_DATE),
        total = ISNULL(SUM(p.AMOUNT), 0.00)
    FROM dbo.PAYMENTS p
    INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID AND m.CLIENT_ID = @ClientId
    WHERE p.PAID_DATE >= @startOfYear
      AND p.PAID_DATE < @endOfYear
      AND p.PAYMENT_STATUS = 'SUCCESS'
    GROUP BY MONTH(p.PAID_DATE)
    ORDER BY [Month];

    -- 3) planBreakdown[]
    SELECT
        planId = mp.PLAN_ID,
        planName = mp.PLAN_NAME,
        memberCount = COUNT(1)
    FROM dbo.SUBSCRIPTIONS s
    INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = s.MEMBER_ID AND m.CLIENT_ID = @ClientId
    INNER JOIN dbo.MEMBERSHIP_PLANS mp ON mp.PLAN_ID = s.PLAN_ID AND mp.CLIENT_ID = @ClientId
    WHERE s.START_DATE >= @startOfYear
      AND s.START_DATE < @endOfYear
      AND s.STATUS IN ('ACTIVE','EXPIRED')
    GROUP BY mp.PLAN_ID, mp.PLAN_NAME
    ORDER BY memberCount DESC;

    -- 4) yearComparison { current, previous }
    DECLARE @prevStart DATE = DATEFROMPARTS(@y - 1, 1, 1);
    DECLARE @prevEnd DATE = DATEFROMPARTS(@y, 1, 1);

    SELECT
        currentSatus = (
            SELECT ISNULL(SUM(p.AMOUNT), 0.00)
            FROM dbo.PAYMENTS p
            INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID AND m.CLIENT_ID = @ClientId
            WHERE p.PAID_DATE >= @startOfYear AND p.PAID_DATE < @endOfYear AND p.PAYMENT_STATUS = 'SUCCESS'
        ),
        previousStatus = (
            SELECT ISNULL(SUM(p.AMOUNT), 0.00)
            FROM dbo.PAYMENTS p
            INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID AND m.CLIENT_ID = @ClientId
            WHERE p.PAID_DATE >= @prevStart AND p.PAID_DATE < @prevEnd AND p.PAYMENT_STATUS = 'SUCCESS'
        );
END
