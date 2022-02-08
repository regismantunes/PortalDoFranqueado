CREATE TABLE [dbo].[User]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Name] VARCHAR(50) NOT NULL,
  [Status] SMALLINT NOT NULL,
  [Email] VARCHAR(100) NOT NULL,
  [Password] CHAR(52) NULL,
  [Role] SMALLINT NOT NULL,
  [Treatment] VARCHAR(20) NULL
)

GO

CREATE UNIQUE INDEX [IX_User_Email] ON [dbo].[User] ([Email])
