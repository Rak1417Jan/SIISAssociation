/*
    Master permission catalog (global). Safe to re-run: inserts only missing codes.
*/
SET NOCOUNT ON;

MERGE dbo.PERMISSIONS AS tgt
USING (VALUES
    (N'members.read', N'View members and applications'),
    (N'members.write', N'Approve, reject, or update member records'),
    (N'firms.read', N'View firms'),
    (N'firms.write', N'Create or update firms'),
    (N'broadcast.send', N'Create or dispatch broadcasts'),
    (N'staff.manage', N'Manage staff users'),
    (N'roles.manage', N'Assign permissions to roles'),
    (N'admin.full', N'Full administrative access')
) AS src(PERMISSION_CODE, DESCRIPTION)
ON tgt.PERMISSION_CODE = src.PERMISSION_CODE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PERMISSION_CODE, DESCRIPTION, CREATED_DATE)
    VALUES (src.PERMISSION_CODE, src.DESCRIPTION, SYSUTCDATETIME());
