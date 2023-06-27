CREATE TABLE Purchase_Sugestion
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseId INT NOT NULL,
	[Target] DECIMAL(9,2),
	AverageTicket DECIMAL(6,2),
	PartsPerService INT,
	Coverage DECIMAL(5,2),
	TotalSugestedItems INT,

	CONSTRAINT FK_Purchase_Sugestion_Purchase FOREIGN KEY (PurchaseId) REFERENCES Purchase (Id) ON DELETE CASCADE
)
GO

CREATE TABLE Purchase_Sugestion_Family
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseSugestionId INT NOT NULL,
	FamilyId INT NOT NULL,
	[Percentage] DECIMAL(5,2) NOT NULL,
	FamilySugestedItems INT,

	CONSTRAINT FK_Purchase_Sugestion_Family_Purchase_Sugestion FOREIGN KEY (PurchaseSugestionId) REFERENCES Purchase_Sugestion (Id) ON DELETE CASCADE
)
GO

CREATE TABLE Purchase_Sugestion_Family_Size
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseSugestionFamilyId INT NOT NULL,
	SizeId VARCHAR(5) NOT NULL,
	[Percentage] DECIMAL(5,2) NOT NULL,
	SizeSugestedItems INT,
	
	CONSTRAINT FK_Purchase_Sugestion_Family_Size_Purchase_Sugestion_Family FOREIGN KEY (PurchaseSugestionFamilyId) REFERENCES Purchase_Sugestion_Family (Id) ON DELETE CASCADE
)
GO