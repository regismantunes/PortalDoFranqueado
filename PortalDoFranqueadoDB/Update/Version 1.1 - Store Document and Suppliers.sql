ALTER TABLE Store
	ADD DocumentNumber VARCHAR(20)
GO

UPDATE Store SET DocumentNumber = '31549562000112' WHERE [Name] = 'Norte Sul Plaza'
UPDATE Store SET DocumentNumber = '39582400000198' WHERE [Name] = 'Rondon Plaza'
UPDATE Store SET DocumentNumber = '43584609000168' WHERE [Name] = 'Shopping Sinop'
UPDATE Store SET DocumentNumber = '43357195000134' WHERE [Name] = 'Shopping Campo Grande'
UPDATE Store SET DocumentNumber = '44190613000104' WHERE [Name] = 'Norte Shopping RJ'
UPDATE Store SET DocumentNumber = '31582280000117' WHERE [Name] = 'Porto Velho Shopping'
GO

CREATE TABLE [dbo].[Supplier]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Active] BIT NOT NULL
)
GO

ALTER TABLE Product
	ADD SupplierId INT
GO

ALTER TABLE Product
	ADD CONSTRAINT [FK_Product_Supplier] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier]([Id])
GO