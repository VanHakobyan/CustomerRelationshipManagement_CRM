CREATE TABLE [dbo].[EmailListContacts] (
    [EmailListID] INT NOT NULL,
    [ContactID]   INT NOT NULL,
    CONSTRAINT [PK_dbo.EmailListContacts] PRIMARY KEY CLUSTERED ([EmailListID] ASC, [ContactID] ASC),
    CONSTRAINT [FK_dbo.EmailListContacts_dbo.Contacts_Contact_Id] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.EmailListContacts_dbo.EmailLists_EmailList_ID] FOREIGN KEY ([EmailListID]) REFERENCES [dbo].[EmailLists] ([EmailListID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_EmailListID]
    ON [dbo].[EmailListContacts]([EmailListID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ContactID]
    ON [dbo].[EmailListContacts]([ContactID] ASC);

