CREATE TABLE [dbo].[Auxiliary]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Description] VARCHAR(50) NOT NULL, 
    [PathId] INT NULL, 
    CONSTRAINT [FK_Auxiliary_Path] FOREIGN KEY ([PathId]) REFERENCES [Path]([Id])
)
