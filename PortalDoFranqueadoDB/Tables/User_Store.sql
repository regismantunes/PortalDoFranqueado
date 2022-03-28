CREATE TABLE [dbo].[User_Store]
(
  [UserId] INT NOT NULL ,
  [StoreId] INT NOT NULL, 
    PRIMARY KEY ([UserId], [StoreId]), 
    CONSTRAINT [FK_User_Store_User] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [FK_User_Store_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id])
)