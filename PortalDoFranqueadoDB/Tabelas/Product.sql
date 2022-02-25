CREATE TABLE [dbo].[Product]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [CollectionId] INT NOT NULL,
  [FamilyId] INT NOT NULL,
  [PhotoId] CHAR(33) NULL,
  [Price] DECIMAL(6,2) NULL,
  [LockedSizes] VARCHAR(41) NULL,
    CONSTRAINT [FK_Product_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [Collection]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [FK_Product_Family] FOREIGN KEY ([FamilyId]) REFERENCES [Family]([Id])
)