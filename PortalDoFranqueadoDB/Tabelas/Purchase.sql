CREATE TABLE [dbo].[Purchase] 
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[StoreId] INT NOT NULL,
	[CollectionId] INT NOT NULL,
	[Status] SMALLINT NOT NULL, 
	CONSTRAINT [FK_Purchase_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [Collection]([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_Purchase_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id])
)