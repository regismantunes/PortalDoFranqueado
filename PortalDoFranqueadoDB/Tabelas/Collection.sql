CREATE TABLE [dbo].[Collection]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [StatDate] DATE NOT NULL,
  [EndDate] DATE NOT NULL,
  [Status] INT NOT NULL
)