﻿CREATE TABLE [dbo].[Campaign]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] VARCHAR(45) NOT NULL,
    [Status] SMALLINT NOT NULL, 
    [PathId] INT NULL, 
    CONSTRAINT [FK_Campaign_Path] FOREIGN KEY ([PathId]) REFERENCES [Path]([Id])
)
