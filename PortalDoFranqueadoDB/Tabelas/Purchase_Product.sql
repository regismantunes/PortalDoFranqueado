CREATE TABLE [dbo].[Purchase_Product]
(
	[PurchaseId] INT NOT NULL,
	[Item] INT NOT NULL,
	[ProductId] INT NOT NULL,
	[SizeId] VARCHAR(5) NOT NULL,
	[Quantity] INT NOT NULL, 
    CONSTRAINT [PK_Purchase_Product] PRIMARY KEY ([PurchaseId], [Item]), 
    CONSTRAINT [FK_Purchase_Product_Purchase] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchase]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Purchase_Product_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id])
)