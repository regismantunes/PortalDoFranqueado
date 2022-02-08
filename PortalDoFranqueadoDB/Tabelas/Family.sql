CREATE TABLE [dbo].[Family]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Name] VARCHAR(50) NOT NULL
)
GO

CREATE UNIQUE INDEX [IX_Family_Name] ON [dbo].[Family] ([Name])
