CREATE TABLE [dbo].[Templates] (
    [TemplateId]   INT            IDENTITY (1, 1) NOT NULL,
    [TemplateName] NVARCHAR (100) NOT NULL,
    [Path] VARCHAR(200) NOT NULL, 
    PRIMARY KEY CLUSTERED ([TemplateId] ASC)
);

