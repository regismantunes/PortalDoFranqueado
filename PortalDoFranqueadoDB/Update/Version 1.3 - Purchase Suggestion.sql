CREATE TABLE Purchase_Suggestion
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseId INT NOT NULL,
	[Target] DECIMAL(9,2),
	AverageTicket DECIMAL(6,2),
	PartsPerService INT,
	Coverage DECIMAL(5,2),
	TotalSuggestedItems INT,

	CONSTRAINT FK_Purchase_Suggestion_Purchase FOREIGN KEY (PurchaseId) REFERENCES Purchase (Id) ON DELETE CASCADE
)
GO

CREATE TABLE Purchase_Suggestion_Family
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseSuggestionId INT NOT NULL,
	FamilyId INT NOT NULL,
	[Percentage] DECIMAL(5,4) NOT NULL,
	FamilySuggestedItems INT,

	CONSTRAINT FK_Purchase_Suggestion_Family_Purchase_Suggestion FOREIGN KEY (PurchaseSuggestionId) REFERENCES Purchase_Suggestion (Id) ON DELETE CASCADE
)
GO

CREATE TABLE Purchase_Suggestion_Family_Size
(
	Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	PurchaseSuggestionFamilyId INT NOT NULL,
	SizeId VARCHAR(5) NOT NULL,
	[Percentage] DECIMAL(5,4) NOT NULL,
	SizeSuggestedItems INT,
	
	CONSTRAINT FK_Purchase_Suggestion_Family_Size_Purchase_Suggestion_Family FOREIGN KEY (PurchaseSuggestionFamilyId) REFERENCES Purchase_Suggestion_Family (Id) ON DELETE CASCADE
)
GO