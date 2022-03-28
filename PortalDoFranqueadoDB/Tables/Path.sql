CREATE TABLE [dbo].[Path]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [FatherPathId] INT NULL, 
    [PathType] SMALLINT NOT NULL, 
    CONSTRAINT [FK_Path_Path] FOREIGN KEY ([FatherPathId]) REFERENCES [Path]([Id])
)
