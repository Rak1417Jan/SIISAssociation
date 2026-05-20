CREATE PROCEDURE [dbo].[usp_Admin_CreateStaff]

    @ClientId INT,

    @Username NVARCHAR(100),

    @Email NVARCHAR(150),

    @PasswordHash VARBINARY(500),

    @PasswordSalt VARBINARY(256),

    @RoleIdsJson NVARCHAR(MAX),

    @FullName NVARCHAR(100) = NULL,

    @MobileNo NVARCHAR(15) = NULL,

    @CreatedBy INT = NULL

AS

BEGIN

    SET NOCOUNT ON;



    IF EXISTS (SELECT 1 FROM dbo.USERS WHERE USERNAME = @Username AND CLIENT_ID = @ClientId)

    BEGIN

        RAISERROR('Username already exists.', 16, 1);

        RETURN;

    END



    IF EXISTS (SELECT 1 FROM dbo.USERS WHERE EMAIL = @Email AND CLIENT_ID = @ClientId)

    BEGIN

        RAISERROR('Email already exists.', 16, 1);

        RETURN;

    END



    IF (@RoleIdsJson IS NULL OR (SELECT COUNT(*) FROM OPENJSON(@RoleIdsJson)) = 0)

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



    INSERT INTO dbo.USERS (

        CLIENT_ID, USERNAME, EMAIL, PASSWORD_HASH, PASSWORD_SALT,

        FULL_NAME, MOBILE_NO,

        IS_ACTIVE, IS_FIRST_LOGIN, MUST_CHANGE_PASSWORD, CREATED_BY)

    VALUES (

        @ClientId, @Username, @Email, @PasswordHash, @PasswordSalt,

        NULLIF(LTRIM(RTRIM(@FullName)), N''),

        NULLIF(LTRIM(RTRIM(@MobileNo)), N''),

        1, 1, 1, @CreatedBy);



    DECLARE @newUserId INT = CAST(SCOPE_IDENTITY() AS INT);



    INSERT INTO dbo.USER_ROLES (USER_ID, ROLE_ID)

    SELECT DISTINCT @newUserId, CAST(j.[value] AS INT)

    FROM OPENJSON(@RoleIdsJson) j;



    SELECT @newUserId;

END
