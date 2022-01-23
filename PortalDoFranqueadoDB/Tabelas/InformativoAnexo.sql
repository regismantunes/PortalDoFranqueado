CREATE TABLE [dbo].[InformativoAnexo]
(
	[IdInformativo] INT NOT NULL , 
    [IdArquivo] INT NOT NULL, 
    PRIMARY KEY ([IdInformativo], [IdArquivo])
)
