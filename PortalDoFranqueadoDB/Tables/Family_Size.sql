CREATE TABLE [dbo].[Family_Size]
(
  [FamilyId] INT NOT NULL,
  [SizeId] VARCHAR(5) NOT NULL, 
    CONSTRAINT [PK_Family_Size] PRIMARY KEY ([FamilyId], [SizeId]), 
    CONSTRAINT [FK_Family_Size_Family] FOREIGN KEY ([FamilyId]) REFERENCES [Family] ([Id])
)