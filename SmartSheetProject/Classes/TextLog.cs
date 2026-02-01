using SmartSheetProject.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSheetProject.Classes
{
    internal static class TextLog
    {
        private static readonly SemaphoreSlim fileLock = new SemaphoreSlim(1, 1);
        private const long MaxLogFileSize = 10 * 1024 * 1024; // 10MB
        internal static async Task LogToSQLiteAsync(string details)
        {
            try
            {
                string query = @"INSERT INTO ErrorLogs (Details, Date_) 
                             VALUES (@details, @date)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@details", details },
                { "@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } // String olarak kalsın
            };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                if (!result.Success)
                    await LogToFileFallbackAsync($"SQLite log hatası: {result.ErrorMessage} | Orijinal: {details}");
            }
            catch (Exception ex)
            {
                await LogToFileFallbackAsync($"LogToSQLiteAsync exception: {ex.Message} | Orijinal: {details}");
            }
        }
        private static async Task LogToFileFallbackAsync(string message)
        {
            await fileLock.WaitAsync();
            try
            {
                string logFilePath = Path.Combine(Application.StartupPath, "Logs", "UILog.txt");
                string logDir = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                // Dosya boyutu kontrolü
                FileInfo fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Exists && fileInfo.Length > MaxLogFileSize)
                {
                    string archivePath = Path.Combine(logDir, $"UILog_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    File.Move(logFilePath, archivePath);
                }
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                using (StreamWriter sw = new StreamWriter(logFilePath, true, Encoding.UTF8))
                {
                    await sw.WriteLineAsync(logEntry);
                }
            }
            catch
            {
                // Son çare - sessizce başarısız ol
            }
            finally
            {
                fileLock.Release();
            }
        }
    }
}