using DataAccessLibrary.DataAccess;
using DataAccessLibrary.Models;
using System.Collections.Generic;

namespace DataAccessLibrary.BusinessLogic
{
    /// <summary>
    /// Logik zum Verwalten von Aufgaben/Tasks
    /// </summary>
    public static class TaskProcessor
    {
        /// <summary>
        /// Anlagen einer neuen Aufgabe
        /// </summary>
        /// <param name="taskname">Name der Aufgabe</param>
        /// <param name="taskDescription">Ergaenzende Beschreibung zur Aufgabe</param>
        /// <param name="isActive">Gestartet/gestoppt</param>
        /// <returns>True, wenn die Transaktion erfolgreich war</returns>
        public static List<int> CreateTask(string taskname, string taskDescription, bool isActive)
        {
            string sql = @"insert into dbo.Tasks (TaskName, TaskDescription, TimeStamp, IsActive)
                            output inserted.Id
                            values (@TaskName, @TaskDescription, GETDATE(), @IsActive);";

            return SqlDataAccess.LoadData<int>(sql, new TaskModel
            {
                TaskName = taskname,
                TaskDescription = taskDescription,
                IsActive = isActive
            });
        }

        /// <summary>
        /// Aktualiseren einer Aufgabe
        /// </summary>
        /// <param name="taskname">Name der Aufgabe</param>
        /// <param name="taskDescription">Ergaenzende Beschreibung zur Aufgabe</param>
        /// <param name="isActive">Gestartet/gestoppt</param>
        /// <returns>True, wenn die Transaktion erfolgreich war</returns>
        public static bool UpdateTask(int taskId, string taskname, string taskDescription, bool isActive)
        {
            string sql = @"update dbo.Tasks set TaskName = @TaskName, TaskDescription = @TaskDescription, IsActive = @IsActive
                            where Id = @Id;";

            return SqlDataAccess.ChangeData(sql, new TaskModel
            {
                Id = taskId,
                TaskName = taskname,
                TaskDescription = taskDescription,
                IsActive = isActive
            });
        }

        /// <summary>
        /// Loescht eine vorhandene Aufgabe, sowie die Historie dieser Aufgabe
        /// </summary>
        /// <param name="taskId">ID der Aufgabe</param>
        /// <returns>True, wenn die Transaktion erfolgreich war</returns>
        public static bool DeleteTask(int taskId)
        {
            string sql = @"delete from dbo.Tasks where Id = @TaskID;
                            delete from dbo.TasksHistory where TaskID = @TaskID;";

            return SqlDataAccess.ChangeData(sql, new { TaskID = taskId });
        }

        /// <summary>
        /// Ruft die Historie bzgl. aller Aenderungen einer Aufgabe ab
        /// </summary>
        /// <param name="isActive">Aufgabe gestartet oder pausiert</param>
        /// <param name="taskId">ID der Aufgabe</param>
        /// <returns>True, wenn die Transaktion erfolgreich war</returns>
        public static bool UpdateHistory(int taskId, bool isActive, bool isActive_old)
        {
            if (isActive != isActive_old)
            {
                string sql = "";

                // Aufgabe wurde gestartet
                if (isActive)
                {
                    sql = @"insert into dbo.TasksHistory (TaskID, StartDate) values (@TaskID, GETDATE());";
                }
                // Aufgabe wurde pausiert
                else
                {
                    sql = @"update dbo.TasksHistory set EndDate = GETDATE(), Duration = DATEDIFF(second, StartDate, GETDATE())
                        where TaskID = @TaskID and EndDate is null;";
                }

                return SqlDataAccess.ChangeData(sql, new { TaskID = taskId });
            }
            return false;
        }

        /// <summary>
        /// Abrufen aller vorhandenen Aufgaben
        /// </summary>
        /// <returns>Liste aller Aufgaben</returns>
        public static List<TaskModel> LoadTasks()
        {
            string sql = @"select T.Id, TaskName, TaskDescription, TimeStamp, IsActive, Duration = sum(ISNULL(TH.Duration, DATEDIFF(second, StartDate, GETDATE())))
                            from dbo.Tasks as T left join dbo.TasksHistory as TH on T.Id = TH.TaskID
                            group by T.Id, TaskName, TaskDescription, TimeStamp, IsActive
                            order by T.Id;";

            return SqlDataAccess.LoadData<TaskModel>(sql, null);
        }

        /// <summary>
        /// Abrufen der Historie einer bestimmten Aufgabe
        /// </summary>
        /// <returns></returns>
        public static List<TaskHistoryModel> LoadTaskHistory(int taskId)
        {
            string sql = @"select TaskID = row_number() over(order by TaskID), StartDate, EndDate = ISNULL(EndDate, GETDATE()), Duration = ISNULL(Duration, DATEDIFF(second, StartDate, GETDATE()))
                            from dbo.TasksHistory where TaskID = @TaskID order by Id desc;";

            return SqlDataAccess.LoadData<TaskHistoryModel>(sql, new { TaskID = taskId });
        }
    }
}