CREATE TABLE [dbo].[Campaign_File]
(
	[CampaignId] INT NOT NULL,
    [FileId] INT NOT NULL,
    PRIMARY KEY ([CampaignId], [FileId]), 
    CONSTRAINT [FK_Campaing_File_Campaign] FOREIGN KEY ([CampaignId]) REFERENCES [Campaign]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Campaing_File_File] FOREIGN KEY ([FileId]) REFERENCES [File]([Id]) ON DELETE CASCADE
)
