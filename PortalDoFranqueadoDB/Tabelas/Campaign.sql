CREATE TABLE [dbo].[Campaign]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] VARCHAR(45) NOT NULL, 
    [FolderId] CHAR(33) NULL, 
    [Status] SMALLINT NOT NULL
)
