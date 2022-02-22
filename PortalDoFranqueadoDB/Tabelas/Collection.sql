﻿CREATE TABLE [dbo].[Collection]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [StartDate] DATE NOT NULL,
  [EndDate] DATE NOT NULL,
  [FolderId] CHAR(33) NULL,
  [Status] SMALLINT NOT NULL,
  [Excluded] BIT NOT NULL,
)