using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DevExpress.LookAndFeel;

namespace SmartSheetProject.Classes
{
    internal static class ThemeManager
    {
        /// <summary>
        /// Kullanıcının kayıtlı temasını yükler (tek kullanıcı)
        /// </summary>
        public static async Task LoadUserThemeAsync()
        {
            try
            {
                string query = "SELECT Theme FROM Users LIMIT 1";
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string savedTheme = dt.Rows[0]["Theme"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(savedTheme))
                        UserLookAndFeel.Default.SetSkinStyle(savedTheme);
                    else
                        UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
                }
                else
                    UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"Tema yükleme hatası: {ex.Message}");
                UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
            }
        }
        /// <summary>
        /// Temayı kaydeder (tek kullanıcı - ID her zaman 1)
        /// </summary>
        public static async Task SaveUserThemeAsync(string themeName)
        {
            try
            {
                string query = "UPDATE Users SET Theme = @theme WHERE Id = 1";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@theme", themeName }
                };
                await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"Tema kaydetme hatası: {ex.Message}");
            }
        }
        /// <summary>
        /// Mevcut temayı getirir
        /// </summary>
        public static async Task<string> GetCurrentThemeAsync()
        {
            try
            {
                string query = "SELECT Theme FROM Users LIMIT 1";
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query);
                if (dt != null && dt.Rows.Count > 0)
                    return dt.Rows[0]["Theme"]?.ToString() ?? "Office 2019 Colorful";
                return "Office 2019 Colorful";
            }
            catch
            {
                return "Office 2019 Colorful";
            }
        }
    }
}