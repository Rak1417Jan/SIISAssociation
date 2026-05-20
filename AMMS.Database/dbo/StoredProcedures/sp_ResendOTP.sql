CREATE PROCEDURE [dbo].[sp_ResendOTP]
    @MobileNo NVARCHAR(20),
    @CooldownSeconds INT = 60
AS
BEGIN
    SET NOCOUNT ON;

    IF (@MobileNo IS NULL OR LEN(LTRIM(RTRIM(@MobileNo))) = 0)
    BEGIN
        RAISERROR('Mobile number is required.', 16, 1);
        RETURN;
    END

    IF (@CooldownSeconds < 1)
        SET @CooldownSeconds = 60;

    DECLARE @normalized NVARCHAR(20) = LTRIM(RTRIM(@MobileNo));

    DECLARE @lastSend DATETIME2(7);

    SELECT @lastSend = MAX(v.CREATED_DATE)
    FROM dbo.OTP_VERIFICATIONS AS v
    WHERE v.MOBILE_NUMBER = @normalized;

    IF @lastSend IS NOT NULL
    BEGIN
        DECLARE @elapsed INT = DATEDIFF(SECOND, @lastSend, SYSUTCDATETIME());
        IF @elapsed < @CooldownSeconds
        BEGIN
            DECLARE @wait INT = @CooldownSeconds - @elapsed;
            DECLARE @msg NVARCHAR(400) = N'Please wait ' + CAST(@wait AS NVARCHAR(10)) + N' second(s) before resending the OTP.';
            THROW 50000, @msg, 1;
        END
    END

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
