using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSheetProject.Classes
{
    internal class LicenseManager
    {
        private const string API_BASE_URL = "http://188.132.128.186:1020/api/license";
        private static string _cachedHardwareId = null;

        /// <summary>
        /// Hardware ID'yi al (cache'li)
        /// </summary>
        internal static string GetHardwareId()
        {
            if (_cachedHardwareId == null)
                _cachedHardwareId = HardwareInfo.GetHardwareId();
            return _cachedHardwareId;
        }
        /// <summary>
        /// Kayıtlı lisans var mı kontrol et
        /// </summary>
        internal static async Task<LicenseInfo> GetSavedLicenseAsync()
        {
            try
            {
                string query = "SELECT LicenseKey, CompanyName, ExpiryDate, IsActive FROM LicenseInfo WHERE HardwareId = @hardwareId LIMIT 1";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@hardwareId", GetHardwareId() }
                };
                var dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    return new LicenseInfo
                    {
                        LicenseKey = dt.Rows[0]["LicenseKey"].ToString(),
                        CompanyName = dt.Rows[0]["CompanyName"].ToString(),
                        ExpiryDate = dt.Rows[0]["ExpiryDate"] != DBNull.Value
                            ? Convert.ToDateTime(dt.Rows[0]["ExpiryDate"])
                            : (DateTime?)null,
                        IsActive = Convert.ToBoolean(dt.Rows[0]["IsActive"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetSavedLicense hatası: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Lisansı local'e kaydet
        /// </summary>
        internal static async Task<bool> SaveLicenseAsync(string licenseKey, string companyName, DateTime? expiryDate)
        {
            try
            {
                string query = @"
                    INSERT OR REPLACE INTO LicenseInfo (HardwareId, LicenseKey, CompanyName, ExpiryDate, IsActive, LastCheckDate)
                    VALUES (@hardwareId, @licenseKey, @companyName, @expiryDate, 1, @lastCheckDate)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@hardwareId", GetHardwareId() },
                    { "@licenseKey", licenseKey },
                    { "@companyName", companyName },
                    { "@expiryDate", expiryDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value },
                    { "@lastCheckDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                return result.Success;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ SaveLicense hatası: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Lisansı sil
        /// </summary>
        internal static async Task<bool> DeleteLicenseAsync()
        {
            try
            {
                string query = "DELETE FROM LicenseInfo WHERE HardwareId = @hardwareId";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@hardwareId", GetHardwareId() }
                };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                return result.Success;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ DeleteLicense hatası: {ex.Message}");
                return false;
            }
        }
    }
    internal class LicenseInfo
    {
        public string LicenseKey { get; set; }
        public string CompanyName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}