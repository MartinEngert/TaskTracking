CREATE TABLE [dbo].[TasksHistory]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TaskID] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NULL, 
    [Duration] INT NULL
)
