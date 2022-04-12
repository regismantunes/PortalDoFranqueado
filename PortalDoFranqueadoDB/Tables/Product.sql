CREATE TABLE [dbo].[Product]
(
  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [CollectionId] INT NOT NULL,
  [FamilyId] INT NOT NULL,
  [FileId] INT NULL,
  [Price] DECIMAL(6,2) NULL,
  [LockedSizes] VARCHAR(41) NULL,
    [SupplierId] INT NULL, 
    CONSTRAINT [FK_Product_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [Collection]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [FK_Product_Family] FOREIGN KEY ([FamilyId]) REFERENCES [Family]([Id]),
    CONSTRAINT [FK_Product_File] FOREIGN KEY ([FileId]) REFERENCES [File]([Id]), 
    CONSTRAINT [FK_Product_Supplier] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier]([Id])
)