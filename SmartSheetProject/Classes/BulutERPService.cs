using Newtonsoft.Json.Linq;
using SmartSheetProject.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartSheetProject.Classes
{
    /// <summary>
    /// Bulut ERP SQL sorguları ve token yönetimi için servis sınıfı
    /// </summary>
    internal class BulutERPService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        #region Token Management

        /// <summary>
        /// Geçerli token getirir, dolmuşsa veya 5 dakikadan az kaldıysa yeniler
        /// </summary>
        public static async Task<(bool Success, string AccessToken, string ErrorMessage)> EnsureValidTokenAsync()
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                // Token var mı?
                if (string.IsNullOrWhiteSpace(settings.AccessToken) ||
                    string.IsNullOrWhiteSpace(settings.TokenExpireDate))
                    return await BulutERPConnectionTest.GetTokenAsync();
                // Token süresi dolmuş mu? (5 dakika önceden kontrol)
                DateTime expireDate = DateTime.Parse(settings.TokenExpireDate);
               TimeSpan remaining = expireDate - DateTime.Now;
                if (remaining.TotalMinutes <= 5)
                    return await BulutERPConnectionTest.GetTokenAsync();
                // Token geçerli
                return (true, settings.AccessToken, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ EnsureValidTokenAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region API Operations

        /// <summary>
        /// SQL sorgusu çalıştırır
        /// Token otomatik kontrol eder ve yeniler
        /// </summary>
        /// <param name="sqlQuery">SQL sorgusu ($V(firm) kullanılabilir)</param>
        /// <param name="accessToken">Opsiyonel token (null ise otomatik alınır)</param>
        /// <param name="maxCount">Maksimum kayıt sayısı (varsayılan: 10000)</param>
        public static async Task<(bool Success, List<Dictionary<string, object>> Data, string ErrorMessage)> ExecuteSelectQueryAsync(
            string sqlQuery,
            string accessToken = null,
            int maxCount = 10000)
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                // Token kontrolü - parametre olarak gelmediyse otomatik yenile
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                // Request body
                var requestBody = new
                {
                    querySqlText = sqlQuery,
                    dataQueryParams = $"{{\"firm\":\"{settings.FirmNr.Trim()}\"}}",
                    jsonFormat = 1,
                    maxCount = maxCount
                };
                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                // Headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                // API URL
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/dataQuery/executeSelectQuery";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    string error = $"SQL sorgusu hatası: {response.StatusCode} - {response.ReasonPhrase}";
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP API hatası - Status: {response.StatusCode}, Response: {jsonResponse}");
                    return (false, null, error);
                }
                // Response parse et
                JObject json = JObject.Parse(jsonResponse);
                bool successful = json["successful"]?.Value<bool>() ?? false;
                if (!successful)
                {
                    string errorMsg = json["errorMessage"]?.ToString() ?? "Bilinmeyen hata";
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP executeSelectQuery başarısız: {errorMsg}");
                    return (false, null, errorMsg);
                }
                // rows array'i al
                JArray rowsArray = json["rows"] as JArray;
                if (rowsArray == null)
                    return (true, new List<Dictionary<string, object>>(), null);
                // Dictionary listesine dönüştür
                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
                foreach (JToken item in rowsArray)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    foreach (JProperty prop in ((JObject)item).Properties())
                        dict[prop.Name] = prop.Value.ToObject<object>();

                    resultList.Add(dict);
                }
                return (true, resultList, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ExecuteSelectQueryAsync exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

    }
}