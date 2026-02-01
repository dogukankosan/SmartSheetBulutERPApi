using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartSheetProject.Classes
{
    internal class LicenseApiClient
    {
        private const string API_BASE_URL = "http://188.132.128.186:1020/api/license";
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        /// <summary>
        /// Lisansı API'den aktive et
        /// </summary>
        internal static async Task<ApiResponse> ActivateLicenseAsync(string licenseKey, string companyName)
        {
            try
            {
                var request = new
                {
                    licenseKey = licenseKey,
                    hardwareId = LicenseManager.GetHardwareId(),
                    companyName = companyName
                };
                string json = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{API_BASE_URL}/activate", content);
                string responseBody = await response.Content.ReadAsStringAsync();
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ API bağlantı hatası (Activate): {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edin."
                };
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Aktivasyon hatası: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Hata: {ex.Message}"
                };
            }
        }
        /// <summary>
        /// Lisansı API'den doğrula
        /// </summary>
        internal static async Task<ApiResponse> ValidateLicenseAsync(string licenseKey)
        {
            try
            {
                var request = new
                {
                    licenseKey = licenseKey,
                    hardwareId = LicenseManager.GetHardwareId()
                };
                string json = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{API_BASE_URL}/validate", content);
                string responseBody = await response.Content.ReadAsStringAsync();
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ API bağlantı hatası (Validate): {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edin."
                };
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Doğrulama hatası: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Hata: {ex.Message}"
                };
            }
        }
        /// <summary>
        /// API sağlık kontrolü
        /// </summary>
        internal static async Task<bool> CheckApiHealthAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{API_BASE_URL}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    } 
    internal class ApiResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}