CREATE TABLE [dbo].[Collection_File]
(
	[CollectionId] INT NOT NULL,
    [FileId] INT NOT NULL,
    PRIMARY KEY ([CollectionId], [FileId]), 
    CONSTRAINT [FK_Collection_File_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [Collection]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Collection_File_File] FOREIGN KEY ([FileId]) REFERENCES [File]([Id]) ON DELETE CASCADE
)
