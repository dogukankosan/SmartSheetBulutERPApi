using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartSheetProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// SQL sorgusu çalıştırır - Token otomatik kontrol eder ve yeniler
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
                string jsonContent = JsonConvert.SerializeObject(requestBody);
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
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP API hatası - Status: {response.StatusCode}, Response: {jsonResponse}");
                    return (false, null, $"SQL sorgusu hatası: {response.StatusCode} - {response.ReasonPhrase}");
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

        #region Invoice Creation
        public static async Task<(bool Success, string LogoInvoiceNo, string ErrorMessage)> CreateInvoiceAsync(
            object invoiceData,
            int invoiceType = 4,
            string accessToken = null)
        {
            string faturaNo = "UNKNOWN";
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var tokenResult = await EnsureValidTokenAsync();
                    if (!tokenResult.Success)
                        return (false, null, tokenResult.ErrorMessage);
                    accessToken = tokenResult.AccessToken;
                }
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Culture = System.Globalization.CultureInfo.InvariantCulture
                };
                string jsonContent = JsonConvert.SerializeObject(invoiceData, jsonSettings);
                // Fatura no'sunu JSON'dan çek
                try
                {
                    JObject tempJson = JObject.Parse(jsonContent);
                    faturaNo = tempJson["no"]?.ToString() ?? "UNKNOWN";
                }
                catch { faturaNo = "UNKNOWN"; }
                // JSON'u JSONLog klasörüne yaz
                string appStartupPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "JSONLog"
                );
                // Klasör yoksa oluştur
                if (!Directory.Exists(appStartupPath))
                    Directory.CreateDirectory(appStartupPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/invoices/purchase?invoiceType={invoiceType}&lang=TRTR";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    // ❌ HATA DURUMUNDA - Mutlaka kaydet
                    string errorFileName = $"{timestamp}_HATALI_{faturaNo}.json";
                    string errorPath = Path.Combine(appStartupPath, errorFileName);
                    string errorLog = $@"/*
=================================================
HATA - LOGO FATURA AKTARIM HATASI
=================================================
Fatura No: {faturaNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
HTTP Status: {response.StatusCode}
HTTP Reason: {response.ReasonPhrase}
=================================================
*/

// ========== REQUEST JSON ==========
{jsonContent}

// ========== RESPONSE ERROR ==========
{responseContent}";
                    File.WriteAllText(errorPath, errorLog);
                    await TextLog.LogToSQLiteAsync($"❌ Logo Fatura POST hatası - Status: {response.StatusCode}, Response: {responseContent}");
                    return (false, null, $"HTTP {response.StatusCode}: {responseContent}");
                }
                JObject responseJson = JObject.Parse(responseContent);
                string invoiceNo = responseJson["no"]?.ToString();
                // ✅ BAŞARILI DURUMDA - Response ile kaydet
                string successFileName = $"{timestamp}_{faturaNo}.json";
                string successPath = Path.Combine(appStartupPath, successFileName);
                string successLog = $@"/*
=================================================
BAŞARILI - LOGO FATURA AKTARIM
=================================================
Fatura No: {faturaNo}
Logo Fiş No: {invoiceNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
Invoice Type: {invoiceType}
=================================================
*/

// ========== REQUEST JSON ==========
{jsonContent}

// ========== RESPONSE SUCCESS ==========
{responseContent}";
                File.WriteAllText(successPath, successLog);
                await TextLog.LogToSQLiteAsync($"✅ Logo'ya fatura oluşturuldu: {invoiceNo}");
                return (true, invoiceNo, null);
            }
            catch (Exception ex)
            {
                // ❌ EXCEPTION DURUMUNDA - Mutlaka kaydet
                string appStartupPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "JSONLog"
                );
                if (!Directory.Exists(appStartupPath))
                   Directory.CreateDirectory(appStartupPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exceptionFileName = $"{timestamp}_HATALI_EXCEPTION_{faturaNo}.json";
                string exceptionPath = Path.Combine(appStartupPath, exceptionFileName);
                string exceptionLog = $@"/*
=================================================
EXCEPTION - LOGO FATURA AKTARIM HATASI
=================================================
Fatura No: {faturaNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
Exception: {ex.Message}
StackTrace: {ex.StackTrace}
=================================================
*/";
                File.WriteAllText(exceptionPath, exceptionLog);
                await TextLog.LogToSQLiteAsync($"❌ CreateInvoiceAsync exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region Malzeme Kontrol
        /// <summary>
        /// Malzeme kodunun Logo'da olup olmadığını ve CARDTYPE'ını kontrol eder
        /// </summary>
        /// <returns>(Success, CARDTYPE, ErrorMessage)</returns>
        public static async Task<(bool Success, int? CardType, string ErrorMessage)> GetMalzemeCardTypeAsync(string malzemeKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(malzemeKodu))
                    return (false, null, "Malzeme kodu boş olamaz");
                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);
                string sqlQuery = $"SELECT CARDTYPE FROM U_$V(firm)_ITEMS WHERE BOSTATUS<>1 AND CODE='{malzemeKodu.Replace("'", "''")}'";
                var result = await ExecuteSelectQueryAsync(sqlQuery, tokenResult.AccessToken, 1);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);
                if (result.Data == null || result.Data.Count == 0)
                    return (false, null, $"Logo'da '{malzemeKodu}' malzeme kodu bulunamadı!");
                object cardTypeObj = result.Data[0].ContainsKey("CARDTYPE") ? result.Data[0]["CARDTYPE"] : null;
                if (cardTypeObj == null)
                    return (false, null, "CARDTYPE alanı boş");
                int cardType = Convert.ToInt32(cardTypeObj);
                return (true, cardType, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetMalzemeCardTypeAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region Invoice Conversion
        public static async Task<(bool Success, object InvoiceData, int InvoiceType, string ErrorMessage)> ConvertGroupedExpenseToInvoiceAsync(
            GroupedExpenseModel group,
            string cariKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(group.LogoReference))
                    return (false, null, 0, "LOGO Accounting Reference # alanı boş - fatura oluşturulamaz!");
                List<object> itemList = new List<object>();
                HashSet<int> cardTypes = new HashSet<int>();
                List<string> hatalar = new List<string>();
                decimal toplamKdvsiz = 0;
                decimal toplamKdv = 0;
                // İlk satırın fatura açıklamasını al
                string faturaGenelAciklama = group.Items.FirstOrDefault()?.FaturaAciklamasi ?? "Smartsheet";
                foreach (ExpenseModel item in group.Items)
                {
                    string malzemeKodu = item.MalzemeListesi ?? "";
                    if (malzemeKodu.Contains("---"))
                        malzemeKodu = malzemeKodu.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();
                    if (string.IsNullOrWhiteSpace(malzemeKodu))
                    {
                        hatalar.Add($"Satır '{group.SirketAdi}': Malzeme kodu boş!");
                        continue;
                    }
                    var malzemeResult = await GetMalzemeCardTypeAsync(malzemeKodu);
                    if (!malzemeResult.Success)
                    {
                        hatalar.Add($"Malzeme '{malzemeKodu}': {malzemeResult.ErrorMessage}");
                        continue;
                    }
                    int cardType = malzemeResult.CardType.Value;
                    cardTypes.Add(cardType);
                    decimal satirToplam = item.SatirToplamTutar ?? 0;
                    decimal kdvOrani = item.KDVOrani ?? 0;
                    bool kdvVar = item.KDV?.ToUpper() == "VAR";
                    decimal kdvsizTutar;
                    decimal kdvTutari;
                    if (kdvVar && kdvOrani > 0)
                    {
                        kdvsizTutar = Math.Round(satirToplam * 100 / (100 + kdvOrani), 2);
                        kdvTutari = Math.Round(satirToplam - kdvsizTutar, 2);
                    }
                    else
                    {
                        kdvsizTutar = satirToplam;
                        kdvTutari = 0;
                        kdvOrani = 0;
                    }
                    toplamKdvsiz += kdvsizTutar;
                    toplamKdv += kdvTutari;
                    // ÖNEMLİ: CardType = 1 ise type = 0 (normal satış), değilse type = 4 (hizmet)
                    // YENİ:
                    int itemType;
                    if (cardType == 1 || cardType == 13)
                        itemType = 0;   // ticari mal / tüketim malı
                    else if (cardType == 4)
                        itemType = 8;   // varlık
                    else
                        itemType = 4;   // hizmet
                    var itemTransaction = new
                    {
                        type = itemType,
                        code = malzemeKodu,
                        description = group.SirketAdi ?? "GİDER",
                        quantity = 1.0,
                        unit = 23,
                        unitCode = "ADET",
                        unitPrice = kdvsizTutar,
                        currencyTypeRC = 160,
                        vatratePercent = Math.Round(kdvOrani, 2),
                        vatamount = kdvTutari,
                        vatbase = kdvsizTutar,
                        amount = kdvsizTutar,
                        netAmount = kdvsizTutar,
                        orderDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        description2 = item.SirketAdi ?? "GİDER"
                    };
                   itemList.Add(itemTransaction);
                }
                if (hatalar.Count > 0)
                {
                    string hataMetni = string.Join("\n", hatalar);
                    await TextLog.LogToSQLiteAsync($"❌ Fatura oluşturulamadı - {group.FaturaNo}:\n{hataMetni}");
                    return (false, null, 0, hataMetni);
                }
                if (itemList.Count == 0)
                    return (false, null, 0, "Geçerli malzeme kalemi bulunamadı!");
                // FATURA TİPİ: 1 tane bile malzeme varsa SATINALMA (1), yoksa HİZMET (4)
                int invoiceType = (cardTypes.Contains(1) || cardTypes.Contains(13) || cardTypes.Contains(4)) ? 1 : 4;
                toplamKdvsiz = Math.Round(toplamKdvsiz, 2);
                toplamKdv = Math.Round(toplamKdv, 2);
                decimal toplamTutar = toplamKdvsiz + toplamKdv;
                var invoice = new
                {
                    inputServiceDistrbutor = false,
                    itemTransactionDTO = itemList.ToArray(),
                    masterDataDispatcDTO = new object[]  // ← DOLU GÖNDER
    {
        new
        {
            type = 1,
            date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
            documenDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
        }
    },
                  //  no = group.FaturaNo,
                    documentNo = group.FaturaNo,
                    date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    time = new
                    {
                        hour = group.FaturaTarihi.Value.Hour,
                        minute = group.FaturaTarihi.Value.Minute
                    },
                    auxCode5 = group.LogoReference ?? "",
                    auxCode = group.ProjeKodu ?? "",
                    documentDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    orgUnit = "01",
                    department = "0",
                    arap = cariKodu,
                    footnote = faturaGenelAciklama,
                    customer = cariKodu,
                    invoiceType = invoiceType,
                    grossinLC = toplamKdvsiz,
                    totalVATinLC = toplamKdv,
                    netTotalinLC = toplamTutar
                };
                return (true, invoice, invoiceType, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ConvertGroupedExpenseToInvoiceAsync hatası: {ex.Message}");
                return (false, null, 0, ex.Message);
            }
        }
        #endregion

        #region Invoice Check

        /// <summary>
        /// Logo'da belirtilen fatura numarası, cari kodu ve tarihin birebir uyuşup uyuşmadığını kontrol eder
        /// </summary>
        /// <param name="faturaNo">Kontrol edilecek fatura numarası</param>
        /// <param name="cariKodu">Kontrol edilecek cari kodu</param>
        /// <param name="faturaTarihi">Kontrol edilecek fatura tarihi</param>
        /// <returns>Fatura var mı yok mu bilgisi</returns>
        public static async Task<(bool Success, bool Exists, string ErrorMessage)> CheckInvoiceExistsAsync(
            string faturaNo,
            string cariKodu,
            DateTime faturaTarihi,
            string logoReference=null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(faturaNo))
                    return (false, false, "Fatura numarası boş!");
                if (string.IsNullOrWhiteSpace(cariKodu))
                    return (false, false, "Cari kodu boş!");
                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, false, tokenResult.ErrorMessage);
                // SQL sorgusu ile fatura kontrolü - Fatura no ve cari kodu uyuşmalı

                string logoCondition = !string.IsNullOrWhiteSpace(logoReference)
                    ? $"AND (INV.DOCODE = '{faturaNo.Replace("'", "''")}' AND INV.AUXCODE5 = '{logoReference.Replace("'", "''")}')"
                    : $"AND INV.DOCODE = '{faturaNo.Replace("'", "''")}'";

                string sqlQuery = $@"
    SELECT INV.LOGICALREF
    FROM U_$V(firm)_01_INVOICES INV
    JOIN U_$V(firm)_ARPS ARP ON ARP.LOGICALREF = INV.ARPREF
    WHERE ARP.CODE = '{cariKodu.Replace("'", "''")}'
    {logoCondition}".Trim();
                var result = await ExecuteSelectQueryAsync(sqlQuery, tokenResult.AccessToken, 1);
                if (!result.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ Fatura kontrol hatası ({faturaNo}): {result.ErrorMessage}");
                    return (false, false, result.ErrorMessage);
                }
                bool exists = result.Data != null && result.Data.Count > 0;
                return (true, exists, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CheckInvoiceExistsAsync exception: {ex.Message}");
                return (false, false, ex.Message);
            }
        }
        public static async Task<(bool Success, bool Exists, string ErrorMessage)> CheckProjeKoduExistsAsync(string projeKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projeKodu))
                    return (false, false, "Proje kodu boş");
                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, false, tokenResult.ErrorMessage);
                // ✅ Şartlar eklendi: CODETYPE=1 ve (USAGETYPE & 1) = 1
                string sqlQuery = $"SELECT AUXCODE FROM U_$V(firm)_AUXCODES WHERE AUXCODETYPE=22 AND AUXCODE='{projeKodu.Replace("'", "''")}' AND CODETYPE=1 AND (USAGETYPE & 1) = 1";
                var result = await ExecuteSelectQueryAsync(sqlQuery, tokenResult.AccessToken, 1);
                if (!result.Success)
                    return (false, false, result.ErrorMessage);
                bool exists = result.Data != null && result.Data.Count > 0;
                return (true, exists, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CheckProjeKoduExistsAsync hatası: {ex.Message}");
                return (false, false, ex.Message);
            }
        }
        #endregion

    }
}