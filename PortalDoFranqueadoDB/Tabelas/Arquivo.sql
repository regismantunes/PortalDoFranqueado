CREATE TABLE [dbo].[Arquivo]
(
	[IdArquivo] INT NOT NULL PRIMARY KEY IDENTITY, 
    [EnderecoArquivo] VARCHAR(255) NOT NULL, 
    [TipoArquivo] BIT NOT NULL -- 0 - Foto / 1 - Outros
)
