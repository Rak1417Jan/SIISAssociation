CREATE PROCEDURE [dbo].[usp_Admin_UpdateStaff]
    @ClientId INT,
    @UserId INT,
    @Email NVARCHAR(150) = NULL,
    @FullName NVARCHAR(100) = NULL,
    @MobileNo NVARCHAR(15) = NULL,
    @RoleIdsJson NVARCHAR(MAX) = NULL,
    @IsActive BIT = NULL,
    @ModifiedBy INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.USERS WHERE USER_ID = @UserId AND CLIENT_ID = @ClientId)
    BEGIN
        RAISERROR('Staff not found.', 16, 1);
        RETURN;
    END

    IF (@Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.USERS WHERE EMAIL = @Email AND USER_ID <> @UserId AND CLIENT_ID = @ClientId))
    BEGIN
        RAISERROR('Email already exists.', 16, 1);
        RETURN;
    END

    UPDATE dbo.USERS
    SET EMAIL = ISNULL(@Email, EMAIL),
        FULL_NAME = CASE WHEN @FullName IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@FullName)), N'') ELSE FULL_NAME END,
        MOBILE_NO = CASE WHEN @MobileNo IS NOT NULL THEN NULLIF(LTRIM(RTRIM(@MobileNo)), N'') ELSE MOBILE_NO END,
        IS_ACTIVE = ISNULL(@IsActive, IS_ACTIVE),
        MODIFIED_DATE = GETDATE(),
        MODIFIED_BY = @ModifiedBy
    WHERE USER_ID = @UserId
      AND CLIENT_ID = @ClientId;

    IF (@RoleIdsJson IS NOT NULL)
    BEGIN
        IF ((SELECT COUNT(*) FROM OPENJSON(@RoleIdsJson)) = 0)
        BEGIN
            RAISERROR('At least one role is required.', 16, 1);
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM OPENJSON(@RoleIdsJson) j
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.ROLES r
                WHERE r.ROLE_ID = CAST(j.[value] AS INT) AND r.CLIENT_ID = @ClientId
            )
        )
        BEGIN
            RAISERROR('One or more roles were not found for this client.', 16, 1);
            RETURN;
        END

        DELETE ur
        FROM dbo.USER_ROLES ur
        INNER JOIN dbo.USERS u ON u.USER_ID = ur.USER_ID
        WHERE ur.USER_ID = @UserId AND u.CLIENT_ID = @ClientId;

        INSERT INTO dbo.USER_ROLES (USER_ID, ROLE_ID)
        SELECT DISTINCT @UserId, CAST(j.[value] AS INT)
        FROM OPENJSON(@RoleIdsJson) j;
    END
END
