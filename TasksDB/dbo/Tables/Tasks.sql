CREATE TABLE [dbo].[Tasks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TaskName] NVARCHAR(100) NOT NULL, 
    [TaskDescription] NVARCHAR(500) NULL, 
	[TimeStamp] DATETIME NOT NULL,
    [IsActive] BIT NOT NULL
)
