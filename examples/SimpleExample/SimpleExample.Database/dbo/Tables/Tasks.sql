CREATE TABLE [dbo].[Tasks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Description] VARCHAR(1000) NOT NULL, 
    [TaskStatusId] INT NOT NULL, 
    [Created] DATETIME NOT NULL , 
    [CreatedBy] VARCHAR(50) NOT NULL, 
    [Updated] DATETIME NOT NULL, 
    [UpdatedBy] VARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_Tasks_ToTaskStatus] FOREIGN KEY ([TaskStatusId]) REFERENCES [TaskStatus]([Id])
)
