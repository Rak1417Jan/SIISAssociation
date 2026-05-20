CREATE PROCEDURE [dbo].[sp_GenerateOTP]
    @MobileNo NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@MobileNo IS NULL OR LEN(LTRIM(RTRIM(@MobileNo))) = 0)
    BEGIN
        RAISERROR('Mobile number is required.', 16, 1);
        RETURN;
    END

    DECLARE @normalized NVARCHAR(20) = LTRIM(RTRIM(@MobileNo));
    /* 6-digit numeric OTP (100000–999999) */
    DECLARE @otpCode NVARCHAR(10) = CAST((ABS(CHECKSUM(NEWID())) % 900000) + 100000 AS NVARCHAR(6));
    DECLARE @expiresAt DATETIME2(7) = DATEADD(MINUTE, 10, SYSUTCDATETIME());

    INSERT INTO dbo.OTP_VERIFICATIONS (MOBILE_NUMBER, OTP_CODE, IS_VERIFIED, EXPIRY_TIME, CREATED_DATE)
    VALUES (@normalized, @otpCode, 0, @expiresAt, SYSUTCDATETIME());

    SELECT
        CAST(SCOPE_IDENTITY() AS INT) AS OtpId,
        @normalized AS MobileNo,
        @otpCode AS OtpCode,
        @expiresAt AS ExpiresOn;
END
