CREATE TABLE [dbo].[MEMBERSHIP_PLANS] (
    [PLAN_ID]            INT             IDENTITY (1, 1) NOT NULL,
    [CLIENT_ID]          INT             NOT NULL,
    [PLAN_NAME]          NVARCHAR (100)  NOT NULL,
    [PRICE]              DECIMAL (18, 2) NOT NULL,
    [VALIDITY_IN_MONTHS] INT             NOT NULL,
    [IS_ACTIVE]          BIT             DEFAULT ((1)) NULL,
    [CREATED_DATE]       DATETIME2 (7)   DEFAULT (getdate()) NULL,
    [CREATED_BY]         INT             NULL,
    [MODIFIED_DATE]      DATETIME2 (7)   NULL,
    [MODIFIED_BY]        INT             NULL,
    PRIMARY KEY CLUSTERED ([PLAN_ID] ASC),
    CONSTRAINT [FK_MEMBERSHIP_PLANS_CLIENTS] FOREIGN KEY ([CLIENT_ID]) REFERENCES [dbo].[CLIENTS] ([CLIENT_ID])
);

