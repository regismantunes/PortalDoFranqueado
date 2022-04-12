CREATE TABLE [dbo].[Store]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Name] VARCHAR(100) NOT NULL, 
    [DocumentNumber] VARCHAR(20) NULL
)

GO

CREATE UNIQUE INDEX [IX_Store_Name] ON [dbo].[Store] ([Name])
