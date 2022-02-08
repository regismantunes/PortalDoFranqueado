CREATE TABLE [dbo].[Informative]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Title] VARCHAR(100) NULL,
  [DateTime] DATETIME NOT NULL,
  [Text] TEXT NULL,
  [UserId] INT NULL, 
    CONSTRAINT [FK_Informative_User] FOREIGN KEY ([UserId]) REFERENCES [User]([Id])
)