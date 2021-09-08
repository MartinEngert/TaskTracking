using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessLibrary.DataAccess
{
    /// <summary>
    /// DAO (Data Access Object) auf Grundlage von Dapper (ORM)
    /// Alternativ: ADO.NET bzw. Entity Framework
    /// </summary>
    public static class SqlDataAccess
    {
        /// <summary>
        /// Erstellt eine Verbindungszeichenfolge fuer den Zugriff auf eine Datenbank
        /// Grundlage ist der ConnectionString in der [App.config] und der Name der Datenbank
        /// </summary>
        /// <param name="connectionName">Name der Datenbank</param>
        /// <returns>Verbindungszeichenfolge</returns>
        public static string GetConnectionString(string connectionName = "ConnectionString_TasksDB")
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        /// <summary>
        /// Ruft Daten aus einer Datenbanktabelle ab
        /// </summary>
        /// <typeparam name="T">Rueckgabetyp</typeparam>
        /// <param name="sql">Select Statement</param>
        /// <returns>Resultset</returns>
        public static List<T> LoadData<T>(string sql, object data)
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                return cnn.Query<T>(sql, data).ToList();
            }
        }

        /// <summary>
        /// Fuehrt Datenbankanweisungen aus
        /// </summary>
        /// <typeparam name="T">Typ/Model der Entitaet</typeparam>
        /// <param name="sql">SQL-Statements</param>
        /// <param name="data">Entitaet</param>
        /// <returns>True, wenn die Transaktion erfolgreich war</returns>
        public static bool ChangeData<T>(string sql, T data)
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                // Oeffnen der Datenbankverbindung
                cnn.Open();
                using (IDbTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        // Aktuelle Transaktion wird ausgefuehrt -> commit wenn erfolgreich
                        cnn.Execute(sql, data, tran);
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        // Rollback der aktuellen Transaktion bei Auftreten eines Fehlers
                        tran.Rollback();
                        throw;
                    }
                    finally
                    {
                        // Schließen der Datenbankverbindung
                        cnn.Close();
                    }
                }
            }
        }
    }
}