CREATE PROCEDURE [dbo].[usp_Admin_UpdateRolePermissions]
    @ClientId INT,
    @RoleName NVARCHAR(50),
    @PermissionIdsJson NVARCHAR(MAX),
    @ModifiedBy INT = NULL
AS
BEGIN
    DECLARE @RoleId INT;

    SELECT @RoleId = r.ROLE_ID
    FROM dbo.ROLES r
    WHERE r.ROLE_NAME = @RoleName AND r.CLIENT_ID = @ClientId;

    IF @RoleId IS NULL
    BEGIN
        RAISERROR('Role not found.', 16, 1);
        RETURN;
    END;

    DELETE FROM dbo.ROLE_PERMISSION WHERE ROLE_ID = @RoleId;

    INSERT INTO dbo.ROLE_PERMISSION (ROLE_ID, PERMISSION_ID, CREATED_DATE, CREATED_BY, MODIFICATION_DATE, MODIFIED_BY)
    SELECT
        @RoleId,
        CAST(j.[value] AS INT),
        SYSUTCDATETIME(),
        @ModifiedBy,
        SYSUTCDATETIME(),
        @ModifiedBy
    FROM OPENJSON(@PermissionIdsJson) AS j
    INNER JOIN dbo.PERMISSIONS p ON p.PERMISSION_ID = CAST(j.[value] AS INT);

    DECLARE @PermJson NVARCHAR(MAX);

    SELECT @PermJson = COALESCE((
        SELECT N'[' + STRING_AGG(N'"' + REPLACE(p.PERMISSION_CODE, N'"', N'\"') + N'"', N',')
            WITHIN GROUP (ORDER BY p.PERMISSION_CODE) + N']'
        FROM dbo.ROLE_PERMISSION rp
        INNER JOIN dbo.PERMISSIONS p ON p.PERMISSION_ID = rp.PERMISSION_ID
        WHERE rp.ROLE_ID = @RoleId
    ), N'[]');

    UPDATE dbo.ROLES
    SET PERMISSIONS = @PermJson,
        MODIFIED_DATE = SYSUTCDATETIME(),
        MODIFIED_BY = @ModifiedBy
    WHERE ROLE_ID = @RoleId
      AND CLIENT_ID = @ClientId;
END
