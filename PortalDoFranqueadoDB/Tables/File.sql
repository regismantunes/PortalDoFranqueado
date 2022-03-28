CREATE TABLE [dbo].[File]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] VARCHAR(100) NOT NULL,
    [ContentType] VARCHAR(150) NULL,
    [CreatedDate] DATETIME NOT NULL, 
    [Extension] VARCHAR(50) NOT NULL, 
    [Size] BIGINT NOT NULL, 
    [CompressionType] VARCHAR(50) NOT NULL, 
)
