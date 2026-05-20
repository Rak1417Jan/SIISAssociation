CREATE PROCEDURE [dbo].[sp_ValidateOTP]
    @MobileNo NVARCHAR(20),
    @OTPCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@MobileNo IS NULL OR LEN(LTRIM(RTRIM(@MobileNo))) = 0
        OR @OTPCode IS NULL OR LEN(LTRIM(RTRIM(@OTPCode))) = 0)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsValid,
            N'Mobile number and OTP are required.' AS Message,
            N'' AS AccessToken,
            N'' AS RefreshToken;
        RETURN;
    END

    DECLARE @normalizedMobile NVARCHAR(20) = LTRIM(RTRIM(@MobileNo));
    DECLARE @normalizedOtp NVARCHAR(10) = LTRIM(RTRIM(@OTPCode));

    DECLARE @otpId INT;

    SELECT TOP (1)
        @otpId = v.OTP_ID
    FROM dbo.OTP_VERIFICATIONS AS v
    WHERE v.MOBILE_NUMBER = @normalizedMobile
      AND v.OTP_CODE = @normalizedOtp
      AND ISNULL(v.IS_VERIFIED, 0) = 0
      AND v.EXPIRY_TIME IS NOT NULL
      AND v.EXPIRY_TIME > SYSUTCDATETIME()
    ORDER BY v.OTP_ID DESC;

    IF @otpId IS NULL
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsValid,
            N'Invalid or expired OTP.' AS Message,
            N'' AS AccessToken,
            N'' AS RefreshToken;
        RETURN;
    END

    UPDATE dbo.OTP_VERIFICATIONS
    SET IS_VERIFIED = 1,
        MODIFIED_DATE = SYSUTCDATETIME()
    WHERE OTP_ID = @otpId;

    SELECT
        CAST(1 AS bit) AS IsValid,
        N'OTP verified.' AS Message,
        N'' AS AccessToken,
        N'' AS RefreshToken;
END
