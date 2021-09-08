using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    /// <summary>
    /// Objektmodell, welches der Kommunikation mit der Datenbank dient (entspricht der Repraesentation der Daten)
    /// 
    /// Annotationen nur fuer MVC-Anwendungen relevant
    /// </summary>
    public class TaskHistoryModel
    {
        [Display(Name = "ID")]
        public int TaskID { get; set; }

        [Display(Name = "Startatum")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Enddatum")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Dauer")]
        public int Duration { get; set; }
    }
}