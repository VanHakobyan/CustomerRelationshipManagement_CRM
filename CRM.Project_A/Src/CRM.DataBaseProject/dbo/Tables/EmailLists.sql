CREATE TABLE [dbo].[EmailLists] (
    [EmailListID]   INT           IDENTITY (1, 1) NOT NULL,
    [EmailListName] VARCHAR (250) NOT NULL,
    CONSTRAINT [PK_dbo.EmailLists] PRIMARY KEY CLUSTERED ([EmailListID] ASC)
);

