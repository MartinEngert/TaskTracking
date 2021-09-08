using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    /// <summary>
    /// Objektmodell, welches der Kommunikation mit der Datenbank dient (entspricht der Repraesentation der Daten)
    /// 
    /// Annotationen nur fuer MVC-Anwendungen relevant
    /// </summary>
    public class TaskModel
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Aufgabe")]
        [Required(ErrorMessage = "Geben Sie einen Namen für die Aufgabe an.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Der Name der Aufgabe muss mindestens 10 Zeichen betragen.")]
        [DataType(DataType.Text)]
        public string TaskName { get; set; }

        [Display(Name = "Beschreibung")]
        [StringLength(500, ErrorMessage = "Die Beschreibung der Aufgabe darf nicht länger als 500 Zeichen sein.")]
        [DataType(DataType.Text)]
        public string TaskDescription { get; set; }

        [Display(Name = "Datum")]
        public DateTime TimeStamp { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Dauer")]
        public int Duration { get; set; }
    }
}