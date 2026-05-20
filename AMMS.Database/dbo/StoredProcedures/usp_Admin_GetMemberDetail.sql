CREATE PROCEDURE [dbo].[usp_Admin_GetMemberDetail]
    @ClientId INT,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.MEMBER_ID AS MemberId,
        m.APPLICATION_ID AS ApplicationId,
        ISNULL(m.MEMBERSHIP_ID, '') AS MembershipId,
        ISNULL(m.OWNER_NAME, '') AS OwnerName,
        ISNULL(m.EMAIL, '') AS Email,
        ISNULL(m.MOBILE_NUMBER, '') AS MobileNumber,
        ISNULL(m.ADDRESS, '') AS Address,
        ISNULL(m.CITY, '') AS City,
        m.DATE_OF_BIRTH AS DateOfBirth,
        m.ANNIVERSARY_DATE AS AnniversaryDate,
        CAST(ISNULL(m.IS_ACTIVE, 0) AS bit) AS IsActive,
        m.COMPANY_ID AS CompanyId,
        c.COMPANY_NAME AS CompanyName,
        ISNULL(a.STATUS, '') AS ApplicationStatus,
        ISNULL(a.REMARKS, '') AS ApplicationRemarks
    FROM dbo.MEMBERS m
    INNER JOIN dbo.COMPANY_MASTER c ON c.COMPANY_ID = m.COMPANY_ID AND c.CLIENT_ID = @ClientId
    LEFT JOIN dbo.MEMBER_APPLICATIONS a ON a.APPLICATION_ID = m.APPLICATION_ID
    WHERE m.MEMBER_ID = @MemberId
      AND m.CLIENT_ID = @ClientId;
END
