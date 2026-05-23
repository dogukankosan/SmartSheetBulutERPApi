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
    internal class BulutERPService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        #region Token Management
        public static async Task<(bool Success, string AccessToken, string ErrorMessage)> EnsureValidTokenAsync()
        {
            try
            {
                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;
                if (string.IsNullOrWhiteSpace(settings.AccessToken) ||
                    string.IsNullOrWhiteSpace(settings.TokenExpireDate))
                    return await BulutERPConnectionTest.GetTokenAsync();
                DateTime expireDate = DateTime.Parse(settings.TokenExpireDate);
                TimeSpan remaining = expireDate - DateTime.Now;
                if (remaining.TotalMinutes <= 5)
                    return await BulutERPConnectionTest.GetTokenAsync();
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
        public static async Task<(bool Success, List<Dictionary<string, object>> Data, string ErrorMessage)> ExecuteSelectQueryAsync(
            string sqlQuery,
            string accessToken = null,
            int maxCount = 5000)
        {
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
                var requestBody = new
                {
                    querySqlText = sqlQuery,
                    dataQueryParams = $"{{\"firm\":\"{settings.FirmNr.Trim()}\"}}",
                    jsonFormat = 1,
                    maxCount = maxCount
                };
                string jsonContent = JsonConvert.SerializeObject(requestBody);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/dataQuery/executeSelectQuery";
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP API hatası - Status: {response.StatusCode}, Response: {jsonResponse}");
                    return (false, null, $"SQL sorgusu hatası: {response.StatusCode} - {response.ReasonPhrase}");
                }
                JObject json = JObject.Parse(jsonResponse);
                bool successful = json["successful"]?.Value<bool>() ?? false;
                if (!successful)
                {
                    string errorMsg = json["errorMessage"]?.ToString() ?? "Bilinmeyen hata";
                    await TextLog.LogToSQLiteAsync($"❌ BulutERP executeSelectQuery başarısız: {errorMsg}");
                    return (false, null, errorMsg);
                }
                JArray rowsArray = json["rows"] as JArray;
                if (rowsArray == null)
                    return (true, new List<Dictionary<string, object>>(), null);
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
                try
                {
                    JObject tempJson = JObject.Parse(jsonContent);
                    faturaNo = tempJson["no"]?.ToString() ?? "UNKNOWN";
                }
                catch { faturaNo = "UNKNOWN"; }

                string appStartupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSONLog");
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
                    string errorPath = Path.Combine(appStartupPath, $"{timestamp}_HATALI_{faturaNo}.json");
                    File.WriteAllText(errorPath, $@"/*
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
{responseContent}");
                    await TextLog.LogToSQLiteAsync($"❌ Logo Fatura POST hatası - Status: {response.StatusCode}, Response: {responseContent}");
                    return (false, null, $"HTTP {response.StatusCode}: {responseContent}");
                }
                JObject responseJson = JObject.Parse(responseContent);
                string invoiceNo = responseJson["no"]?.ToString();
                string successPath = Path.Combine(appStartupPath, $"{timestamp}_{faturaNo}.json");
                File.WriteAllText(successPath, $@"/*
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
{responseContent}");
                await TextLog.LogToSQLiteAsync($"✅ Logo'ya fatura oluşturuldu: {invoiceNo}");
                return (true, invoiceNo, null);
            }
            catch (Exception ex)
            {
                string appStartupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSONLog");
                if (!Directory.Exists(appStartupPath))
                    Directory.CreateDirectory(appStartupPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.WriteAllText(Path.Combine(appStartupPath, $"{timestamp}_HATALI_EXCEPTION_{faturaNo}.json"), $@"/*
=================================================
EXCEPTION - LOGO FATURA AKTARIM HATASI
=================================================
Fatura No: {faturaNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
Exception: {ex.Message}
StackTrace: {ex.StackTrace}
=================================================
*/");
                await TextLog.LogToSQLiteAsync($"❌ CreateInvoiceAsync exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region Bulk Sorgular
        public static async Task<(bool Success, Dictionary<string, int> MalzemeMap, string ErrorMessage)> GetMalzemeCardTypeBulkAsync(List<string> malzemeler)
        {
            try
            {
                if (malzemeler == null || malzemeler.Count == 0)
                    return (true, new Dictionary<string, int>(), null);

                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                string inClause = string.Join(",", malzemeler.Select(k => $"'{k.Replace("'", "''")}'"));
                string sql = $"SELECT CODE, CARDTYPE FROM U_$V(firm)_ITEMS WHERE BOSTATUS<>1 AND CODE IN ({inClause})";

                var result = await ExecuteSelectQueryAsync(sql, tokenResult.AccessToken, malzemeler.Count + 10);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);

                var map = result.Data
                    .Where(r => r.ContainsKey("CODE") && r.ContainsKey("CARDTYPE") && r["CODE"] != null && r["CARDTYPE"] != null)
                    .ToDictionary(
                        r => r["CODE"].ToString().Trim(),
                        r => Convert.ToInt32(r["CARDTYPE"]),
                        StringComparer.OrdinalIgnoreCase);

                return (true, map, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetMalzemeCardTypeBulkAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }

        public static async Task<(bool Success, HashSet<string> GecerliProjeler, string ErrorMessage)> GetProjeKoduBulkAsync(List<string> projeKodlari)
        {
            try
            {
                if (projeKodlari == null || projeKodlari.Count == 0)
                    return (true, new HashSet<string>(), null);

                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                string inClause = string.Join(",", projeKodlari.Select(k => $"'{k.Replace("'", "''")}'"));
                string sql = $"SELECT AUXCODE FROM U_$V(firm)_AUXCODES WHERE AUXCODETYPE=22 AND CODETYPE=1 AND (USAGETYPE & 1)=1 AND AUXCODE IN ({inClause})";

                var result = await ExecuteSelectQueryAsync(sql, tokenResult.AccessToken, projeKodlari.Count + 10);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);

                var gecerliProjeler = new HashSet<string>(
                    result.Data.Select(r => r["AUXCODE"].ToString().Trim()),
                    StringComparer.OrdinalIgnoreCase);

                return (true, gecerliProjeler, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetProjeKoduBulkAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }

        public static async Task<(bool Success, Dictionary<string, (string CariKodu, string Doviz)> EmailCariMap, string ErrorMessage)> GetCariKodulariBulkAsync(List<string> emailler)
        {
            try
            {
                if (emailler == null || emailler.Count == 0)
                    return (true, new Dictionary<string, (string, string)>(), null);

                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                // Tüm torkcheck.com carileri tek seferde çek
                string sql = "SELECT CODE, EMAIL, DESCRIPTION FROM U_$V(firm)_ARPS WHERE BOSTATUS<>1 AND UPPER(EMAIL) LIKE '%@TORKCHECK.COM'";

                var result = await ExecuteSelectQueryAsync(sql, tokenResult.AccessToken, 5000);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);

                // Logo'dan gelen tüm carileri map'e al
                var logoMap = new Dictionary<string, (string CariKodu, string Doviz)>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in result.Data)
                {
                    if (r["EMAIL"] == null || r["CODE"] == null) continue;
                    string email = r["EMAIL"].ToString().Trim();
                    string kod = r["CODE"].ToString().Trim();
                    string desc = r["DESCRIPTION"]?.ToString()?.ToUpper() ?? "";

                    string doviz = "TRY";
                    if (desc.Contains("EURO")) doviz = "EURO";
                    else if (desc.Contains("USD")) doviz = "USD";

                    if (!logoMap.ContainsKey(email))
                        logoMap[email] = (kod, doviz);
                }

                // Smartsheet'ten gelenleri local'de eşleştir
                var map = emailler
                    .Where(e => logoMap.ContainsKey(e))
                    .ToDictionary(
                        e => e,
                        e => logoMap[e],
                        StringComparer.OrdinalIgnoreCase);

                return (true, map, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetCariKodulariBulkAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion

        #region Malzeme Kontrol
        public static async Task<(bool Success, int? CardType, string ErrorMessage)> GetMalzemeCardTypeAsync(string malzemeKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(malzemeKodu))
                    return (false, null, "Malzeme kodu boş olamaz");

                var settingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!settingsResult.Success)
                    return (false, null, settingsResult.ErrorMessage);
                BulutERPSettings settings = settingsResult.Settings;

                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                string url = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/items?itemCode={Uri.EscapeDataString(malzemeKodu)}&lang=TRTR";

                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("access-token", tokenResult.AccessToken);
                    http.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                    http.DefaultRequestHeaders.Add("Accept", "application/json");

                    HttpResponseMessage response = await http.GetAsync(url);
                    string json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        return (false, null, $"Malzeme '{malzemeKodu}' bulunamadı!");

                    JObject obj = JObject.Parse(json);
                    JArray message = obj["message"] as JArray;
                    if (message != null && message.Count > 0)
                        return (false, null, $"Logo'da '{malzemeKodu}' malzeme kodu bulunamadı!");

                    int? cardType = obj["cardType"]?.Value<int>();
                    if (cardType == null)
                        return (false, null, "cardType alanı boş geldi");

                    return (true, cardType, null);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetMalzemeCardTypeAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion
        public static async Task<(bool Success, decimal Rate, string ErrorMessage)> GetTCMBRateAsync(string currencyCode, DateTime faturaTarihi)
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    // Muhasebe standardı: fatura tarihinden bir önceki iş günü kuru
                    DateTime tarih = faturaTarihi.Date.AddDays(-1);

                    // Hafta sonu ise Cuma'ya çek
                    if (tarih.DayOfWeek == DayOfWeek.Saturday)
                        tarih = tarih.AddDays(-1);
                    if (tarih.DayOfWeek == DayOfWeek.Sunday)
                        tarih = tarih.AddDays(-2);

                    // Max 7 gün geriye git (resmi tatil vs.)
                    for (int i = 0; i < 7; i++)
                    {
                        if (tarih.DayOfWeek == DayOfWeek.Saturday) { tarih = tarih.AddDays(-1); continue; }
                        if (tarih.DayOfWeek == DayOfWeek.Sunday) { tarih = tarih.AddDays(-2); continue; }

                        string url = $"https://tcmb.gov.tr/kurlar/{tarih:yyyyMM}/{tarih:ddMMyyyy}.xml";
                        HttpResponseMessage response = await http.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            tarih = tarih.AddDays(-1);
                            continue;
                        }

                        string xml = await response.Content.ReadAsStringAsync();
                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        doc.LoadXml(xml);

                        string tcmbCode = currencyCode == "EURO" ? "EUR" : currencyCode;
                        string xpath = $"//Currency[@Kod='{tcmbCode}']/ForexBuying";
                        System.Xml.XmlNode node = doc.SelectSingleNode(xpath);

                        if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                            return (false, 0, $"{currencyCode} kuru bulunamadı!");

                        if (decimal.TryParse(node.InnerText, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                        {
                            await TextLog.LogToSQLiteAsync($"✅ TCMB kur: {currencyCode} = {rate}, Tarih: {tarih:dd.MM.yyyy} (Fatura: {faturaTarihi:dd.MM.yyyy})");
                            return (true, rate, null);
                        }

                        return (false, 0, "Kur parse edilemedi!");
                    }

                    return (false, 0, $"{currencyCode} için {faturaTarihi:dd.MM.yyyy} tarihli kur bulunamadı!");
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetTCMBRateAsync hatası: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }
        #region Invoice Conversion
        public static async Task<(bool Success, object InvoiceData, int InvoiceType, string ErrorMessage)> ConvertGroupedExpenseToInvoiceAsync(
          GroupedExpenseModel group,
          string cariKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(group.LogoReference))
                    return (false, null, 0, "LOGO Accounting Reference # alanı boş - fatura oluşturulamaz!");

                if (!group.FaturaTarihi.HasValue)
                    return (false, null, 0, "Fatura tarihi boş!");

                string dovizTuru = group.DovizTuru?.Trim().ToUpper() ?? "TRY";
                int currencyTypeRC;
                switch (dovizTuru)
                {
                    case "USD": currencyTypeRC = 1; break;
                    case "EURO": currencyTypeRC = 20; break;
                    default: currencyTypeRC = 160; break;
                }

                bool dovizli = currencyTypeRC != 160;
                decimal kur = 1m;

                if (dovizli)
                {
                    var kurResult = await GetTCMBRateAsync(dovizTuru, group.FaturaTarihi.Value);
                    if (!kurResult.Success)
                        return (false, null, 0, $"TCMB kur alınamadı: {kurResult.ErrorMessage}");
                    kur = kurResult.Rate;
                }

                List<object> itemList = new List<object>();
                HashSet<int> cardTypes = new HashSet<int>();
                List<string> hatalar = new List<string>();
                decimal toplamKdvsiz = 0;
                decimal toplamKdv = 0;
                decimal toplamKdvsizRC = 0;
                decimal toplamKdvRC = 0;
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

                    int itemType;
                    switch (cardType)
                    {
                        case 4: itemType = 8; break;  // Varlık
                        case 30: itemType = 4; break;  // Hizmet
                        default: itemType = 0; break;  // Malzeme (1), Tüketim Malı (13)
                    }


                    string vatExemptionCode = null;
                    string reasonForVATExemption = null;
                    if (!kdvVar || kdvOrani == 0)
                    {
                        vatExemptionCode = "351";
                        reasonForVATExemption = "351/KDV-İstisna olmayan Diğer";
                    }

                    if (dovizli)
                    {
                        decimal satirToplamRC = satirToplam;
                        decimal satirToplamLC = Math.Round(satirToplam * kur, 2);
                        decimal kdvsizTutarRC;
                        decimal kdvTutariRC;

                        if (kdvVar && kdvOrani > 0)
                        {
                            kdvsizTutarRC = Math.Round(satirToplamRC * 100 / (100 + kdvOrani), 2);
                            kdvTutariRC = Math.Round(satirToplamRC - kdvsizTutarRC, 2);
                            kdvsizTutar = Math.Round(kdvsizTutarRC * kur, 2);
                            kdvTutari = Math.Round(kdvTutariRC * kur, 2);
                        }
                        else
                        {
                            kdvsizTutarRC = satirToplamRC;
                            kdvTutariRC = 0;
                            kdvsizTutar = Math.Round(kdvsizTutarRC * kur, 2);
                            kdvTutari = 0;
                        }

                        toplamKdvsizRC += kdvsizTutarRC;
                        toplamKdvRC += kdvTutariRC;
                        toplamKdvsiz = Math.Round(toplamKdvsizRC * kur, 2);
                        toplamKdv = Math.Round(toplamKdvRC * kur, 2);

                        itemList.Add(new
                        {
                            type = itemType,
                            code = malzemeKodu,
                            description = group.SirketAdi ?? "GİDER",
                            quantity = 1.0,
                            unit = 23,
                            unitCode = "ADET",
                            unitPrice = kdvsizTutar,
                            currencyTypeRC = currencyTypeRC,
                            unitPriceInFCurrTC = kdvsizTutarRC,
                            warehouse = "01.0.0",                        // ← EKLENDİ
                            vatratePercent = Math.Round(kdvOrani, 2),
                            vatamount = kdvTutari,
                            vatbase = kdvsizTutar,
                            vatexemptionCode = vatExemptionCode,
                            reasonforVATExemption = reasonForVATExemption,
                            amount = kdvsizTutar,
                            amountinFCurrency = kdvsizTutarRC,
                            netAmount = kdvsizTutar,
                            orderDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                            description2 = item.SirketAdi ?? "GİDER"
                        });
                    }
                    else
                    {
                        itemList.Add(new
                        {
                            type = itemType,
                            code = malzemeKodu,
                            description = group.SirketAdi ?? "GİDER",
                            quantity = 1.0,
                            unit = 23,
                            unitCode = "ADET",
                            unitPrice = kdvsizTutar,
                            currencyTypeRC = currencyTypeRC,
                            warehouse = "01.0.0",                        // ← EKLENDİ
                            vatratePercent = Math.Round(kdvOrani, 2),
                            vatamount = kdvTutari,
                            vatbase = kdvsizTutar,
                            vatexemptionCode = vatExemptionCode,
                            reasonforVATExemption = reasonForVATExemption,
                            amount = kdvsizTutar,
                            netAmount = kdvsizTutar,
                            orderDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                            description2 = item.SirketAdi ?? "GİDER"
                        });
                    }
                }

                if (hatalar.Count > 0)
                {
                    string hataMetni = string.Join("\n", hatalar);
                    await TextLog.LogToSQLiteAsync($"❌ Fatura oluşturulamadı - {group.FaturaNo}:\n{hataMetni}");
                    return (false, null, 0, hataMetni);
                }

                if (itemList.Count == 0)
                    return (false, null, 0, "Geçerli malzeme kalemi bulunamadı!");

                int invoiceType = cardTypes.Any(ct => ct == 1 || ct == 4 || ct == 13) ? 1 : 4;


                toplamKdvsiz = Math.Round(toplamKdvsiz, 2);
                toplamKdv = Math.Round(toplamKdv, 2);
                toplamKdvsizRC = Math.Round(toplamKdvsizRC, 2);
                toplamKdvRC = Math.Round(toplamKdvRC, 2);
                decimal toplamTutar = toplamKdvsiz + toplamKdv;
                decimal toplamTutarRC = toplamKdvsizRC + toplamKdvRC;

                object invoice;

                if (dovizli)
                {
                    invoice = new
                    {
                        inputServiceDistrbutor = false,
                        itemTransactionDTO = itemList.ToArray(),
                        masterDataDispatcDTO = new object[]
                        {
                    new
                    {
                        type = 1,
                        date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        documenDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
                    }
                        },
                        documentNo = group.FaturaNo,
                        date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        time = new
                        {
                            hour = group.FaturaTarihi.Value.Hour,
                            minute = group.FaturaTarihi.Value.Minute
                        },
                        vatExemptionCode = toplamKdv == 0 ? "351" : null,
                        reasonforVATExemption = toplamKdv == 0 ? "351/KDV-İstisna olmayan Diğer" : null,
                        auxCode5 = group.LogoReference ?? "",
                        auxCode = group.ProjeKodu ?? "",
                        documentDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        orgUnit = "01",
                        warehouse = "01.0.0",                            // ← EKLENDİ
                        department = "0",
                        arap = cariKodu,
                        footnote = faturaGenelAciklama,
                        customer = cariKodu,
                        invoiceType = invoiceType,
                        exchRate = kur,
                        currencyType = currencyTypeRC,
                        currencyType2 = currencyTypeRC,
                        transactionCurrencyType = currencyTypeRC,
                        grossinLC = toplamKdvsiz,
                        grossinRC = toplamKdvsizRC,
                        totalVATinLC = toplamKdv,
                        netTotalinLC = toplamTutar,
                        netTotalinRC = toplamTutarRC
                    };
                }
                else
                {
                    invoice = new
                    {
                        inputServiceDistrbutor = false,
                        itemTransactionDTO = itemList.ToArray(),
                        masterDataDispatcDTO = new object[]
                        {
                    new
                    {
                        type = 1,
                        date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        documenDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
                    }
                        },
                        documentNo = group.FaturaNo,
                        date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        time = new
                        {
                            hour = group.FaturaTarihi.Value.Hour,
                            minute = group.FaturaTarihi.Value.Minute
                        },
                        vatExemptionCode = toplamKdv == 0 ? "351" : null,
                        reasonforVATExemption = toplamKdv == 0 ? "351/KDV-İstisna olmayan Diğer" : null,
                        auxCode5 = group.LogoReference ?? "",
                        auxCode = group.ProjeKodu ?? "",
                        documentDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        orgUnit = "01",
                        warehouse = "01.0.0",                            // ← EKLENDİ
                        department = "0",
                        arap = cariKodu,
                        footnote = faturaGenelAciklama,
                        customer = cariKodu,
                        invoiceType = invoiceType,
                        grossinLC = toplamKdvsiz,
                        totalVATinLC = toplamKdv,
                        netTotalinLC = toplamTutar
                    };
                }

                return (true, invoice, invoiceType, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ConvertGroupedExpenseToInvoiceAsync hatası: {ex.Message}");
                return (false, null, 0, ex.Message);
            }
        }
        #endregion
        #region BankSlip Creation
        public static async Task<(bool Success, string SlipNo, string ErrorMessage)> CreateBankSlipAsync(
            object slipData,
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
                string jsonContent = JsonConvert.SerializeObject(slipData, jsonSettings);

                try
                {
                    JObject tempJson = JObject.Parse(jsonContent);
                    faturaNo = tempJson["docNo"]?.ToString() ?? "UNKNOWN";
                }
                catch { faturaNo = "UNKNOWN"; }

                string appStartupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSONLog");
                if (!Directory.Exists(appStartupPath))
                    Directory.CreateDirectory(appStartupPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("access-token", accessToken);
                httpClient.DefaultRequestHeaders.Add("firm", settings.FirmNr.Trim());
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                string apiUrl = $"{settings.ServerUrl.TrimEnd('/')}/{settings.MachineID}/logo/restservices/rest/v2.0/bankslips/serviceinvoice?slipType=11&lang=TRTR";

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string errorPath = Path.Combine(appStartupPath, $"{timestamp}_BANKSLIP_HATALI_{faturaNo}.json");
                    File.WriteAllText(errorPath, $@"/*
=================================================
HATA - LOGO BANKSLIP AKTARIM HATASI
=================================================
Fatura No: {faturaNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
HTTP Status: {response.StatusCode}
=================================================
*/

// ========== REQUEST JSON ==========
{jsonContent}

// ========== RESPONSE ERROR ==========
{responseContent}");
                    await TextLog.LogToSQLiteAsync($"❌ Logo BankSlip POST hatası - Status: {response.StatusCode}, Response: {responseContent}");
                    return (false, null, $"HTTP {response.StatusCode}: {responseContent}");
                }

                JObject responseJson = JObject.Parse(responseContent);
                string slipNo = responseJson["slipNo"]?.ToString();

                string successPath = Path.Combine(appStartupPath, $"{timestamp}_BANKSLIP_{faturaNo}.json");
                File.WriteAllText(successPath, $@"/*
=================================================
BAŞARILI - LOGO BANKSLIP AKTARIM
=================================================
Fatura No: {faturaNo}
Logo Slip No: {slipNo}
Tarih: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
=================================================
*/

// ========== REQUEST JSON ==========
{jsonContent}

// ========== RESPONSE SUCCESS ==========
{responseContent}");

                await TextLog.LogToSQLiteAsync($"✅ Logo'ya BankSlip oluşturuldu: {slipNo}");
                return (true, slipNo, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ CreateBankSlipAsync exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion
        #region BankSlip Conversion (Credit Card)
        public static async Task<(bool Success, object SlipData, string ErrorMessage)> ConvertGroupedExpenseToBankSlipAsync(
            GroupedExpenseModel group,
            string cariKodu,
            string bankAccountCode)  // Cari kartın auxCode5'inden gelecek
        {
            try
            {
                if (!group.FaturaTarihi.HasValue)
                    return (false, null, "Fatura tarihi boş!");

                if (string.IsNullOrWhiteSpace(bankAccountCode))
                    return (false, null, "Banka hesap kodu boş - cari kartın Özel Kod 5 alanı kontrol edilmeli!");

                string dovizTuru = group.DovizTuru?.Trim().ToUpper() ?? "TRY";
                int currencyTypeRC;
                switch (dovizTuru)
                {
                    case "USD": currencyTypeRC = 1; break;
                    case "EURO": currencyTypeRC = 20; break;
                    default: currencyTypeRC = 160; break;
                }

                bool dovizli = currencyTypeRC != 160;
                decimal kur = 1m;

                if (dovizli)
                {
                    var kurResult = await GetTCMBRateAsync(dovizTuru, group.FaturaTarihi.Value);
                    if (!kurResult.Success)
                        return (false, null, $"TCMB kur alınamadı: {kurResult.ErrorMessage}");
                    kur = kurResult.Rate;
                }

                List<object> itemList = new List<object>();
                List<string> hatalar = new List<string>();
                decimal toplamKdvsiz = 0;
                decimal toplamKdv = 0;
                decimal toplamKdvsizRC = 0;
                decimal toplamKdvRC = 0;
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

                    if (dovizli)
                    {
                        decimal kdvsizTutarRC = kdvsizTutar;
                        decimal kdvTutariRC = kdvTutari;
                        kdvsizTutar = Math.Round(kdvsizTutarRC * kur, 2);
                        kdvTutari = Math.Round(kdvTutariRC * kur, 2);
                        toplamKdvsizRC += kdvsizTutarRC;
                        toplamKdvRC += kdvTutariRC;
                        toplamKdvsiz = Math.Round(toplamKdvsizRC * kur, 2);
                        toplamKdv = Math.Round(toplamKdvRC * kur, 2);

                        itemList.Add(new
                        {
                            type = 4,  // Credit Card → hep hizmet
                            code = malzemeKodu,
                            description = group.SirketAdi ?? "GİDER",
                            quantity = 1.0,
                            unit = 23,
                            unitCode = "ADET",
                            unitPrice = kdvsizTutar,
                            currencyTypeRC = currencyTypeRC,
                            unitPriceInFCurrTC = kdvsizTutarRC,
                            vatincluded = false,
                            vatratePercent = Math.Round(kdvOrani, 2),
                            vatamount = kdvTutari,
                            vatbase = kdvsizTutar,
                            amount = kdvsizTutar,
                            amountinFCurrency = kdvsizTutarRC,
                            netAmount = kdvsizTutar,
                            orderDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                            description2 = item.SirketAdi ?? "GİDER"
                        });
                    }
                    else
                    {
                        toplamKdvsiz += kdvsizTutar;
                        toplamKdv += kdvTutari;

                        itemList.Add(new
                        {
                            type = 4,  // Credit Card → hep hizmet
                            code = malzemeKodu,
                            description = group.SirketAdi ?? "GİDER",
                            quantity = 1.0,
                            unit = 23,
                            unitCode = "ADET",
                            unitPrice = kdvsizTutar,
                            currencyTypeRC = currencyTypeRC,
                            vatincluded = false,
                            vatratePercent = Math.Round(kdvOrani, 2),
                            vatamount = kdvTutari,
                            vatbase = kdvsizTutar,
                            amount = kdvsizTutar,
                            netAmount = kdvsizTutar,
                            orderDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                            description2 = item.SirketAdi ?? "GİDER"
                        });
                    }
                }

                if (hatalar.Count > 0)
                {
                    string hataMetni = string.Join("\n", hatalar);
                    await TextLog.LogToSQLiteAsync($"❌ BankSlip oluşturulamadı - {group.FaturaNo}:\n{hataMetni}");
                    return (false, null, hataMetni);
                }

                if (itemList.Count == 0)
                    return (false, null, "Geçerli malzeme kalemi bulunamadı!");

                toplamKdvsiz = Math.Round(toplamKdvsiz, 2);
                toplamKdv = Math.Round(toplamKdv, 2);
                decimal toplamTutar = toplamKdvsiz + toplamKdv;

                object slip = new
                {
                    itemTransactionDTO = itemList.ToArray(),
                    date = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    documentDate = group.FaturaTarihi.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    docNo = group.FaturaNo,
                    orgUnit = "01",
                    department = "0",
                    footnote = faturaGenelAciklama,
                    auxCode = group.ProjeKodu ?? "",
                    auxCode5 = group.LogoReference ?? "",
                    bankAccountCode = bankAccountCode,  // Cari kartın auxCode5'i
                    arap = cariKodu,
                    customer = cariKodu,
                    grossinLC = toplamKdvsiz,
                    totalVATinLC = toplamKdv,
                    netTotalinLC = toplamTutar
                };

                return (true, slip, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ ConvertGroupedExpenseToBankSlipAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion
        #region Invoice Check
        public static async Task<(bool Success, bool Exists, string ErrorMessage)> CheckInvoiceExistsAsync(
            string faturaNo,
            string cariKodu,
            DateTime faturaTarihi,
            string logoReference = null)
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


        #endregion
        #region Cari AuxCode5 (Banka Hesabı)
        public static async Task<(bool Success, string AuxCode5, string ErrorMessage)> GetCariAuxCode5Async(string cariKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cariKodu))
                    return (false, null, "Cari kodu boş!");

                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                string sql = $"SELECT AUXCODE5 FROM U_$V(firm)_ARPS WHERE BOSTATUS<>1 AND CODE = '{cariKodu.Replace("'", "''")}'";

                var result = await ExecuteSelectQueryAsync(sql, tokenResult.AccessToken, 1);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);

                if (result.Data == null || result.Data.Count == 0)
                    return (false, null, $"Cari '{cariKodu}' bulunamadı!");

                string auxCode5 = result.Data[0]["AUXCODE5"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(auxCode5))
                    return (false, null, $"Cari '{cariKodu}' kartında Özel Kod 5 (banka hesabı) boş!");

                await TextLog.LogToSQLiteAsync($"✅ Cari '{cariKodu}' banka hesabı: {auxCode5}");
                return (true, auxCode5, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetCariAuxCode5Async hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion
    }
}