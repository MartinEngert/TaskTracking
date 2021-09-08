using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataAccessLibrary.BusinessLogic;
using DataAccessLibrary.Models;

namespace TaskTracking
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Liste aller abgerufenen Aufgaben
        public List<TaskModel> TaskList { get; set; }
        // Liste der gesamten Historie zu einer Aufgabe
        public List<TaskHistoryModel> TaskHistoryList { get; set; }
        // ID der aktuell gewaehlten Aufgabe (-1 wenn keine Aufgabe gewaehlt)
        public int TaskSelected { get; set; }
        // Aktuelle Ansicht
        public bool IsHistoryView { get; set; }

        public MainWindow()
        {
            TaskList = new List<TaskModel>();
            IsHistoryView = false;
            InitializeComponent();
            ResetFormStat();
            BindTaskGrid();
        }

        #region " Task "
        /// <summary>
        /// Ausfuellen der Formularfelder
        /// </summary>
        /// <param name="taskId">Aktuell gewahlte Augabe</param>
        /// <param name="taskName">Name der Aufgabe</param>
        /// <param name="taskDescription">Beschreibung der Aufgabe</param>
        /// <param name="isAtive">Gestartet/gestoppt</param>
        private void FillForm(int taskId, string taskName, string taskDescription, bool isAtive)
        {
            TaskSelected = taskId;
            TB_TaskName.Text = taskName;
            TB_TaskDescription.Text = taskDescription ?? "";
            CB_Active.IsChecked = isAtive;
        }

        /// <summary>
        /// Anpassen des aktuellen Ansichtszustands
        /// </summary>
        private void SetFormStat()
        {
            BT_New.IsEnabled = true;
            BT_Update.Content = "editieren";
            BT_Delete.IsEnabled = true;
            BT_History.IsEnabled = true;
            ErrorTaskName(0);
        }

        /// <summary>
        /// Setzt den Ansichtszustand zurueck (default-Ansicht)
        /// </summary>
        private void ResetFormStat()
        {
            BT_New.IsEnabled = false;
            BT_Update.Content = "anlegen";
            BT_Delete.IsEnabled = false;
            BT_History.IsEnabled = false;
            ErrorTaskName(0);
            TaskSelected = -1;
            IsHistoryView = true;
            SwitchView();
        }

        /// <summary>
        /// Fehlermeldung, wenn keine Aufgabe angegeben wurde
        /// </summary>
        /// <param name="code">Einblende/ausblenden</param>
        private void ErrorTaskName(int code, string errorText = "Bitte geben Sie eine Aufgabe an")
        {
            LB_EmptyTaskName.Content = errorText;
            if (code == 0)
            {
                LB_EmptyTaskName.Visibility = Visibility.Hidden;
            }
            else
            {
                LB_EmptyTaskName.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Das Formular zuruecksetzen, um eine neue Aufgabe zu erstellen
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (Button)</param>
        /// <param name="e">Ereignisdaten</param>
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            FillForm(-1, "", "", false);
            ResetFormStat();
        }

        /// <summary>
        /// Eine neue Aufgabe anlegen bzw. eine vorhandene Aufgabe editieren
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (Button)</param>
        /// <param name="e">Ereignisdaten</param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string taskDescriptionState = TB_TaskDescription.Text != "" ? TB_TaskDescription.Text : null;
                bool checkboxState = CB_Active.IsChecked == true ? true : false; // wegen nullable

                // Insert
                if (TaskSelected == -1)
                {
                    TaskSelected = TaskProcessor.CreateTask(TB_TaskName.Text, taskDescriptionState, checkboxState)[0];

                    // Die Historie der aktuell gewaehlten Aufgabe wird aktualisiert
                    TaskProcessor.UpdateHistory(TaskSelected, checkboxState, false);

                    // Ansichtszustand der Seite wird angepasst
                    SetFormStat();

                }
                // Update
                else
                {
                    TaskProcessor.UpdateTask(TaskSelected, TB_TaskName.Text, taskDescriptionState, checkboxState);

                    // Die Historie der aktuell gewaehlten Aufgabe wird aktualisiert
                    TaskProcessor.UpdateHistory(TaskSelected, checkboxState, TaskList.First(task => task.Id == TaskSelected).IsActive);

                }
                BindTaskGrid();
            }
        }

        /// <summary>
        /// Bevor eine Aufgabe angelegt bzw. editiert wird, werden die relevanten Benutzereingaben geprueft
        /// </summary>
        /// <returns>True: Alles korrekt; False: Fehler bei der Benutzereingabe</returns>
        private bool ValidateInput()
        {
            // Pruefen der Eingaben des Benutzers
            if (TB_TaskName.Text == "")
            {
                ErrorTaskName(1);
                return false;
            }
            else if (TB_TaskName.Text.Length > 100)
            {
                ErrorTaskName(1, "Name der Aufgabe ist zu lang");
                return false;
            }
            else
            {
                ErrorTaskName(0);
            }
            return true;
        }

        /// <summary>
        /// Loeschen einer gewaehlten Aufgabe
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (Button)</param>
        /// <param name="e">Ereignisdaten</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskSelected != -1)
            {
                TaskProcessor.DeleteTask(TaskSelected);
                BindTaskGrid();
                FillForm(-1, "", "", false);
                ResetFormStat();
            }
        }

        /// <summary>
        /// Wenn der Benutzer auf 'Historie' bzw. 'Uebersicht' klickt, aendert sich entsprechend die Ansicht
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (Button)</param>
        /// <param name="e">Ereignisdaten</param>
        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchView();
        }

        /// <summary>
        /// Aenderung des Seitenlayouts
        /// </summary>
        private void SwitchView()
        {
            // Gesamtansicht aller vorhandenen Aufgaben
            if (IsHistoryView)
            {
                IsHistoryView = false;
                BT_History.Content = "Historie";

                DG_Tasks.Visibility = Visibility.Visible;
                DG_TaskHistory.Visibility = Visibility.Collapsed;
                BindTaskGrid();
            }
            // Ansicht der Historie einer bestimmten Aufgabe
            else
            {
                IsHistoryView = true;
                BT_History.Content = "Übersicht";

                if (TaskSelected != -1)
                {
                    DG_Tasks.Visibility = Visibility.Collapsed;
                    DG_TaskHistory.Visibility = Visibility.Visible;
                    BindTaskHistoryGrid();
                }
            }
        }
        #endregion

        #region " DataGrid "
        /// <summary>
        /// Bindet das DataGrid an eine Datenquelle
        /// </summary>
        private void BindTaskGrid()
        {
            TaskList = TaskProcessor.LoadTasks();
            DG_Tasks.ItemsSource = TaskList;
            Labeling();
        }

        /// <summary>
        /// Bindet das DataGrid fuer die Historie an eine Datenquelle
        /// </summary>
        private void BindTaskHistoryGrid()
        {
            TaskHistoryList = TaskProcessor.LoadTaskHistory(TaskSelected);
            DG_TaskHistory.ItemsSource = TaskHistoryList;
            Labeling();
        }

        /// <summary>
        /// Beschriftung der Seite entsprechend der akuell gewaehlten Ansicht
        /// </summary>
        private void Labeling()
        {
            if (IsHistoryView)
            {
                // Anzahl der gefundenen Statusänderungen fuer eine betreffende Aufgabe
                LB_TaskCount.Content = TaskHistoryList.Count.ToString() + " gefundene Statusänderung" + (TaskHistoryList.Count == 1 ? "" : "en" + ":");
                // Summe der Dauer der betreffenden Aufgaben
                LB_SumDuration.Content = "Summe: " + TaskHistoryList.Sum(x => x.Duration).ToString() + " Sekunden";
            }
            else
            {
                // Anzahl der gefundenen Aufgaben
                LB_TaskCount.Content = TaskList.Count.ToString() + " gefundene Aufgabe" + (TaskList.Count == 1 ? "" : "n" + ":");
                // Summe der Dauer aller Aufgaben
                LB_SumDuration.Content = "Summe: " + TaskList.Sum(x => x.Duration).ToString() + " Sekunden";
            }
        }

        /// <summary>
        /// Dient der Formatierung des DataGrids
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (DataGrid)</param>
        /// <param name="e">Element des DataGrids</param>
        private void DG_Tasks_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Spaltenbezeichnungen und Spaltenbreiten werden angepasst
            if (e.PropertyName == "Id")
            {
                e.Column.Header = "ID";
                e.Column.Width = new DataGridLength(8, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "TaskName")
            {
                e.Column.Header = "Aufgabe";
                e.Column.Width = new DataGridLength(30, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "TaskDescription")
            {
                e.Column.Header = "Beschreibung";
                e.Column.Width = new DataGridLength(25, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "TimeStamp")
            {
                e.Column.Header = "Erstellt am";
                e.Column.Width = new DataGridLength(17, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "IsActive")
            {
                e.Column.Header = "Aktiv";
                e.Column.Width = new DataGridLength(7, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "Duration")
            {
                e.Column.Header = "Dauer [sek]";
                e.Column.Width = new DataGridLength(13, DataGridLengthUnitType.Star);
            }

            // Darstellung von Datumsfeldern wird angepasst
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yy HH:mm";
        }

        /// <summary>
        /// Dient der Formatierung des DataGrids fuer die Historie
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (DataGrid)</param>
        /// <param name="e">Element des DataGrids</param>
        private void DG_TaskHistory_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Spaltenbezeichnungen und Spaltenbreiten werden angepasst
            if (e.PropertyName == "TaskID")
            {
                e.Column.Header = "ID";
                e.Column.Width = new DataGridLength(10, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "StartDate")
            {
                e.Column.Header = "Startzeitpunkt";
                e.Column.Width = new DataGridLength(35, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "EndDate")
            {
                e.Column.Header = "Endzeitpunkt";
                e.Column.Width = new DataGridLength(35, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "Duration")
            {
                e.Column.Header = "Dauer [sek]";
                e.Column.Width = new DataGridLength(20, DataGridLengthUnitType.Star);
            }

            // Darstellung von Datumsfeldern wird angepasst
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yy HH:mm:ss";
        }

        /// <summary>
        /// Zeilen des DataGrids koennen angeklickt (Doppelklick) und somit ausgewaehlt werden
        /// </summary>
        /// <param name="sender">Ausloesendes Objekt des Events (DataGridRow)</param>
        /// <param name="e">Ereignisse des MouseEvents</param>
        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            // Klickevent der einzelnen Zeilen des DataGrids
            if (sender.GetType() == typeof(DataGridRow))
            {
                DataGridRow row = sender as DataGridRow;
                if (row != null)
                {
                    var listElement = TaskList[row.GetIndex()];
                    FillForm(listElement.Id, listElement.TaskName, listElement.TaskDescription, listElement.IsActive);
                    SetFormStat();
                }
            }
        }
        #endregion
    }
}