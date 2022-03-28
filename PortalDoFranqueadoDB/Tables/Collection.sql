CREATE TABLE [dbo].[Collection]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [Status] SMALLINT NOT NULL,
    [Excluded] BIT NOT NULL, 
    [PathId] INT NULL, 
    CONSTRAINT [FK_Collection_Path] FOREIGN KEY ([PathId]) REFERENCES [Path]([Id])
)