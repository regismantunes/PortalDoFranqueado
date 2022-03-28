CREATE TABLE [dbo].[File_Content]
(
	[FileId] INT NOT NULL PRIMARY KEY, 
    [Content] TEXT NOT NULL
	CONSTRAINT [FK_File_Content_File] FOREIGN KEY ([FileId]) REFERENCES [File]([Id]) ON DELETE CASCADE
)
