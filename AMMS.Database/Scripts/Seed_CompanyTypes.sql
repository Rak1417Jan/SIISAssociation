/*
    Global company type lookup + backfill COMPANY_MASTER.COMPANY_TYPE_ID for existing rows.
    Safe to re-run (MERGE by CODE).
*/
SET NOCOUNT ON;

MERGE dbo.COMPANY_TYPE AS tgt
USING (VALUES
    (N'Private', N'PRIVATE'),
    (N'Public', N'PUBLIC'),
    (N'Government-Owned', N'GOV_OWNED'),
    (N'Partnership', N'PARTNERSHIP'),
    (N'Sole Proprietorship', N'SOLE_PROP'),
    (N'Limited Liability Company (LLC)', N'LLC'),
    (N'Cooperative', N'COOPERATIVE')
) AS src(NAME, CODE)
ON tgt.CODE = src.CODE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (NAME, CODE, CREATED_DATE)
    VALUES (src.NAME, src.CODE, SYSUTCDATETIME());

UPDATE cm
SET cm.COMPANY_TYPE_ID = ct.COMPANY_TYPE_ID
FROM dbo.COMPANY_MASTER cm
INNER JOIN dbo.COMPANY_TYPE ct ON ct.CODE = N'PRIVATE'
WHERE cm.COMPANY_TYPE_ID IS NULL;
