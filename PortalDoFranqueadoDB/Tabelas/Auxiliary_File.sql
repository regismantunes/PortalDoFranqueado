CREATE TABLE [dbo].[Auxiliary_File]
(
	[AuxiliaryId] INT NOT NULL, 
    [FileId] INT NOT NULL, 
    PRIMARY KEY ([FileId], [AuxiliaryId]), 
    CONSTRAINT [FK_Auxiliary_File_Auxiliary] FOREIGN KEY ([AuxiliaryId]) REFERENCES [Auxiliary]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Auxiliary_File_File] FOREIGN KEY ([FileId]) REFERENCES [File]([Id]) ON DELETE CASCADE
)
