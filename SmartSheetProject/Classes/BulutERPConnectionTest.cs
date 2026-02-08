using Newtonsoft.Json.Linq;
using SmartSheetProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartSheetProject.Classes
{
    internal class BulutERPConnectionTest
    {
        private static readonly string TOKEN_URL = "https://idm.logo.cloud/legacy/sts/api/oauth/token";
        private static readonly HttpClient httpClient = new HttpClient();

        #region Settings Management

        /// <summary>
        /// Bulut ERP ayarlarını şifreli olarak kaydeder
        /// </summary>
        public static async Task<(bool Success, string ErrorMessage)> SaveSettingsAsync(
            string clientId,
            string clientSecret,
            string username,
            string password,
            string firmNr,
            string serverUrl,
            string machineID)
        {
            try
            {
                // Validasyon
                var validation = BulutERPValidator.ValidateSettings(clientId, clientSecret, username, password, firmNr, serverUrl, machineID);
                if (!validation.IsValid)
                    return (false, validation.ErrorMessage);
                // Tüm değerleri şifrele
                string encryptedClientId = EncryptionHelper.Encrypt(clientId);
                string encryptedClientSecret = EncryptionHelper.Encrypt(clientSecret);
                string encryptedUsername = EncryptionHelper.Encrypt(username);
                string encryptedPassword = EncryptionHelper.Encrypt(password);
                string encryptedFirmNr = EncryptionHelper.Encrypt(firmNr);
                string encryptedServerUrl = EncryptionHelper.Encrypt(serverUrl);
                string encryptedMachineID = EncryptionHelper.Encrypt(machineID);
                // Şifreleme hatası kontrolü
                if (string.IsNullOrWhiteSpace(encryptedClientId) ||
                    string.IsNullOrWhiteSpace(encryptedClientSecret) ||
                    string.IsNullOrWhiteSpace(encryptedUsername) ||
                    string.IsNullOrWhiteSpace(encryptedPassword) ||
                    string.IsNullOrWhiteSpace(encryptedFirmNr) ||
                    string.IsNullOrWhiteSpace(encryptedServerUrl) ||
                    string.IsNullOrWhiteSpace(encryptedMachineID))
                {
                    await TextLog.LogToSQLiteAsync("❌ BulutERP ayarları şifreleme hatası");
                    return (false, "Şifreleme hatası oluştu!");
                }
                // Önce mevcut kayıt var mı kontrol et
                string checkQuery = "SELECT COUNT(*) FROM BulutERPSettings";
                DataTable checkDt = await SQLiteCrud.GetDataFromSQLiteAsync(checkQuery);
                bool recordExists = checkDt.Rows.Count > 0 && Convert.ToInt32(checkDt.Rows[0][0]) > 0;
                string query;
                if (recordExists)
                {
                    // UPDATE - Token bilgilerini sil
                    query = @"UPDATE BulutERPSettings SET 
                              ClientId = @clientId,
                              ClientSecret = @clientSecret,
                              Username = @username,
                              Password = @password,
                              FirmNr = @firmNr,
                              ServerUrl = @serverUrl,
                              MachineID = @machineID,
                              AccessToken = NULL,
                              RefreshToken = NULL,
                              TokenExpireDate = NULL";
                }
                else
                {
                    // INSERT
                    query = @"INSERT INTO BulutERPSettings 
                              (ClientId, ClientSecret, Username, Password, FirmNr, ServerUrl, MachineID, AccessToken, RefreshToken, TokenExpireDate) 
                              VALUES (@clientId, @clientSecret, @username, @password, @firmNr, @serverUrl, @machineID, NULL, NULL, NULL)";
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@clientId", encryptedClientId },
                    { "@clientSecret", encryptedClientSecret },
                    { "@username", encryptedUsername },
                    { "@password", encryptedPassword },
                    { "@firmNr", encryptedFirmNr },
                    { "@serverUrl", encryptedServerUrl },
                    { "@machineID", encryptedMachineID }
                };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                if (!result.Success)
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP ayarları kaydetme hatası: {result.ErrorMessage}");
                return result;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ SaveSettingsAsync hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Bulut ERP ayarlarını çözerek getirir
        /// </summary>
        public static async Task<(bool Success, BulutERPSettings Settings, string ErrorMessage)> GetSettingsAsync()
        {
            try
            {
                string query = "SELECT * FROM BulutERPSettings LIMIT 1";
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query);
                if (dt.Rows.Count == 0)
                    return (false, null, "Bulut ERP ayarları bulunamadı!");
                DataRow row = dt.Rows[0];
                // Şifreli değerleri çöz
                BulutERPSettings settings = new BulutERPSettings
                {
                    ClientId = EncryptionHelper.Decrypt(row["ClientId"].ToString()),
                    ClientSecret = EncryptionHelper.Decrypt(row["ClientSecret"].ToString()),
                    Username = EncryptionHelper.Decrypt(row["Username"].ToString()),
                    Password = EncryptionHelper.Decrypt(row["Password"].ToString()),
                    FirmNr = EncryptionHelper.Decrypt(row["FirmNr"].ToString()),
                    ServerUrl = EncryptionHelper.Decrypt(row["ServerUrl"].ToString()),
                    MachineID = EncryptionHelper.Decrypt(row["MachineID"].ToString()),
                    AccessToken = row["AccessToken"] != DBNull.Value ? row["AccessToken"].ToString() : null,
                    RefreshToken = row["RefreshToken"] != DBNull.Value ? row["RefreshToken"].ToString() : null,
                    TokenExpireDate = row["TokenExpireDate"] != DBNull.Value ? row["TokenExpireDate"].ToString() : null
                };
                // Şifre çözme kontrolü
                if (string.IsNullOrWhiteSpace(settings.ClientId) ||
                    string.IsNullOrWhiteSpace(settings.ClientSecret) ||
                    string.IsNullOrWhiteSpace(settings.Username) ||
                    string.IsNullOrWhiteSpace(settings.Password) ||
                    string.IsNullOrWhiteSpace(settings.MachineID))
                {
                    await TextLog.LogToSQLiteAsync("❌ BulutERP ayarları şifre çözme hatası");
                    return (false, null, "Ayarlar çözülürken hata oluştu!");
                }
                return (true, settings, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetSettingsAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        /// <summary>
        /// Ayarları tamamen sil (sadece TestConnection için kullanılacak)
        /// </summary>
        public static async Task<(bool Success, string ErrorMessage)> DeleteSettingsAsync()
        {
            try
            {
                string query = "DELETE FROM BulutERPSettings";
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query);
                if (!result.Success)
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP ayarları silme hatası: {result.ErrorMessage}");
                return result;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ DeleteSettingsAsync hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }
        #endregion

        #region Token Management

        /// <summary>
        /// OAuth ile token alır ve kaydeder
        /// </summary>
        public static async Task<(bool Success, string AccessToken, string ErrorMessage)> GetTokenAsync()
        {
            try
            {
                var settingsResult = await GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                // Token request body
                Dictionary<string, string> requestBody = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", settings.Username },
                    { "password", settings.Password },
                    { "client_id", settings.ClientId },
                    { "client_secret", settings.ClientSecret },
                    { "lang", "TRTR" }
                };
                FormUrlEncodedContent content = new FormUrlEncodedContent(requestBody);
                HttpResponseMessage response = await httpClient.PostAsync(TOKEN_URL, content);
                if (!response.IsSuccessStatusCode)
                {
                    string error = $"Token alma hatası: {response.StatusCode} - {response.ReasonPhrase}";
                    await TextLog.LogToSQLiteAsync($"❌ {error}");
                    return (false, null, error);
                }
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(jsonResponse);
                string accessToken = json["access_token"]?.ToString();
                string refreshToken = json["refresh_token"]?.ToString();
                int expiresIn = json["expires_in"] != null ? (int)json["expires_in"] : 3600;
                string expireDate = DateTime.Now.AddSeconds(expiresIn).ToString("yyyy-MM-dd HH:mm:ss");
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    await TextLog.LogToSQLiteAsync("❌ Token response'da access_token bulunamadı");
                    return (false, null, "Token alınamadı!");
                }
                // Token'ı veritabanına kaydet
                await SaveTokenAsync(accessToken, refreshToken, expireDate);
                return (true, accessToken, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetTokenAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        /// <summary>
        /// Token'ı veritabanına kaydeder
        /// </summary>
        private static async Task SaveTokenAsync(string accessToken, string refreshToken, string expireDate)
        {
            try
            {
                string query = @"UPDATE BulutERPSettings SET 
                                 AccessToken = @accessToken,
                                 RefreshToken = @refreshToken,
                                 TokenExpireDate = @expireDate";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@accessToken", accessToken },
                    { "@refreshToken", refreshToken ?? "" },
                    { "@expireDate", expireDate }
                };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                if (!result.Success)
                    await TextLog.LogToSQLiteAsync($"❌ Token kaydetme hatası: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ SaveTokenAsync hatası: {ex.Message}");
            }
        }
        #endregion

        #region API Operations

        /// <summary>
        /// Test bağlantısı yapar - SADECE FORM İÇİN KULLANILIR
        /// Başarısız olursa ayarları direk siler
        /// </summary>
        public static async Task<(bool Success, string ErrorMessage)> TestConnectionAsync()
        {
            try
            {
                var settingsResult = await GetSettingsAsync();
                if (!settingsResult.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ TestConnection - Ayarlar bulunamadı");
                    return (false, settingsResult.ErrorMessage);
                }
                BulutERPSettings settings = settingsResult.Settings;
                // 1. TOKEN AL
                var tokenResult = await GetTokenAsync();
                if (!tokenResult.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ TestConnection - Token alınamadı, ayarlar siliniyor: {tokenResult.ErrorMessage}");
                    await DeleteSettingsAsync();
                    return (false, $"Token alınamadı: {tokenResult.ErrorMessage}");
                }
                // 2. SQL SORGUSU TEST ET
                string testSql = "SELECT LOGICALREF FROM U_$V(firm)_ARPS";
                var result = await BulutERPService.ExecuteSelectQueryAsync(
                    testSql,
                    tokenResult.AccessToken,
                    1
                );
                if (!result.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ TestConnection - SQL hatası, ayarlar siliniyor: {result.ErrorMessage}");
                    await DeleteSettingsAsync();
                    return (false, $"SQL sorgusu hatası: {result.ErrorMessage}");
                }
                // Sonuç var mı kontrol et
                if (result.Data == null || result.Data.Count == 0)
                {
                    await TextLog.LogToSQLiteAsync("❌ TestConnection - Sonuç yok, ayarlar siliniyor");
                    await DeleteSettingsAsync();
                    return (false, "Test sorgusu sonuç döndürmedi! Firma numarası veya tablo adı hatalı olabilir.");
                }
                // ✅ HER ŞEY BAŞARILI - Log yok
                return (true, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ TestConnection - Exception, ayarlar siliniyor: {ex.Message}");
                await DeleteSettingsAsync();
                return (false, $"Beklenmeyen hata: {ex.Message}");
            }
        }
        #endregion

    }
}