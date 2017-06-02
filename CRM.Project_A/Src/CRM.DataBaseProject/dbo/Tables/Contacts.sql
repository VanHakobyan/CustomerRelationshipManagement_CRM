CREATE TABLE [dbo].[Contacts] (
    [ContactId]    INT              IDENTITY (1, 1) NOT NULL,
    [FullName]     VARCHAR (250)    NOT NULL,
    [CompanyName]  VARCHAR (250)    NOT NULL,
    [Position]     VARCHAR (250)    NOT NULL,
    [Country]      VARCHAR (150)    NOT NULL,
    [Email]        VARCHAR (250)    NOT NULL UNIQUE,
    [GuID]         UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [DateInserted] DATETIME2 (7)    CONSTRAINT [DF_Contacts_DateInserted] DEFAULT (getdate()) NOT NULL,
	[DateModified] DATETIME2 NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([ContactId] ASC)
);

