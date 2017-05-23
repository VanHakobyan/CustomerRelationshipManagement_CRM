CREATE TABLE [dbo].[Contacts] (
    [ContactId]    INT              IDENTITY (1, 1) NOT NULL,
    [FullName]     VARCHAR (250)    NOT NULL,
    [CompanyName]  VARCHAR (250)    NULL,
    [Position]     VARCHAR (250)    NULL,
    [Country]      VARCHAR (150)    NULL,
    [Email]        VARCHAR (250)    NOT NULL,
    [GuID]         UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [DateInserted] DATETIME2 (7)    CONSTRAINT [DF_Contacts_DateInserted] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([ContactId] ASC)
);

