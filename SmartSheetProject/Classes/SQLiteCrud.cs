using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSheetProject.Classes
{
    internal class SQLiteCrud
    {
        private static readonly string DatabasePath = Path.Combine(Application.StartupPath, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "Settings.db");
        internal static readonly string connectionString = $"Data Source={DatabaseFile};Version=3;Pooling=True;Max Pool Size=100;";

        #region SQLite Operations

        internal static async Task<DataTable> GetDataFromSQLiteAsync(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        AddSQLiteParameters(command, parameters);
                        using (var reader = await command.ExecuteReaderAsync())
                            dataTable.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    await TextLog.LogToSQLiteAsync($"SQLite sorgu hatası: {ex.Message}");
                    return new DataTable();
                }
            }
            return dataTable;
        }
        internal static async Task<(bool Success, string ErrorMessage)> InsertUpdateDeleteAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        AddSQLiteParameters(cmd, parameters);
                        await cmd.ExecuteNonQueryAsync();
                        return (true, null);
                    }
                }
                catch (Exception ex)
                {
                    await TextLog.LogToSQLiteAsync($"SQLite işlem hatası: {ex.Message}");
                    return (false, ex.Message);
                }
            }
        }
        #endregion

        #region Helper Methods

        private static void AddSQLiteParameters(SQLiteCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;
            foreach (var param in parameters)
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        }

        #endregion

    }
}