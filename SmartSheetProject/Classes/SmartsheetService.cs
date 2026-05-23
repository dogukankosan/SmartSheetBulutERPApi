using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using SmartSheetProject.Models;

namespace SmartSheetProject.Classes
{
    internal class SmartsheetService
    {
        private static readonly long GIDER_SHEET_ID = 6658795850649476;
        private static readonly long GELIR_SHEET_ID = 4003473281470340;
        private static readonly long EXPENSES_SHEET_ID = 8931463861849988;
        private static readonly long CARI_VADE_SHEET_ID = 2209925892624260;

        #region Token Management
        public static async Task<(bool Success, string ErrorMessage)> SaveApiTokenAsync(string apiToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiToken))
                    return (false, "API Token boş olamaz!");
                string encryptedToken = EncryptionHelper.Encrypt(apiToken);
                if (string.IsNullOrWhiteSpace(encryptedToken))
                {
                    await TextLog.LogToSQLiteAsync("❌ Token şifreleme hatası");
                    return (false, "Token şifreleme hatası!");
                }
                string query = "UPDATE SmartsheetSettings SET ApiToken = @token WHERE Id = 1";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@token", encryptedToken }
                };
                var result = await SQLiteCrud.InsertUpdateDeleteAsync(query, parameters);
                if (!result.Success)
                    await TextLog.LogToSQLiteAsync($"❌ API Token kaydetme hatası: {result.ErrorMessage}");
                return result;
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ SaveApiTokenAsync hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }
        public static async Task<(bool Success, string Token, string ErrorMessage)> GetApiTokenAsync()
        {
            try
            {
                string query = "SELECT ApiToken FROM SmartsheetSettings WHERE Id = 1 LIMIT 1";
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query);
                if (dt.Rows.Count == 0)
                    return (false, null, "API Token bulunamadı!");
                string encryptedToken = dt.Rows[0]["ApiToken"].ToString();
                if (string.IsNullOrWhiteSpace(encryptedToken))
                    return (false, null, "API Token kayıtlı değil!");
                string decryptedToken = EncryptionHelper.Decrypt(encryptedToken);
                if (string.IsNullOrWhiteSpace(decryptedToken))
                {
                    await TextLog.LogToSQLiteAsync("❌ Token şifre çözme hatası");
                    return (false, null, "Token şifre çözme hatası!");
                }
                return (true, decryptedToken, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetApiTokenAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        public static async Task<bool> IsTokenSavedAsync()
        {
            var result = await GetApiTokenAsync();
            return result.Success && !string.IsNullOrWhiteSpace(result.Token);
        }
        #endregion

        #region Smartsheet SDK Operations
        public static async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, tokenResult.ErrorMessage);
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                Sheet sheet = await Task.Run(() =>
               smartsheet.SheetResources.GetSheet(
                   EXPENSES_SHEET_ID,
                   new List<SheetLevelInclusion>(),  // boş liste
                   null, null, null, null, null, null)
           );
                return (true, $"Bağlantı başarılı! Sheet: {sheet.Name}");
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ TestConnectionAsync hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }
        #endregion

        #region GİDER Sheet Operations
        private static List<Row> CreateGiderRows(List<GiderFaturaModel> faturalar)
        {
            List<Row> rows = new List<Row>();
            foreach (GiderFaturaModel fatura in faturalar)
            {
                Row newRow = new Row
                {
                    ToTop = true,
                    Cells = new List<Cell>
                    {
                        new Cell { ColumnId = 7349303226617732, Value = fatura.CARI_KODU ?? "" },
                        new Cell { ColumnId = 4684432181776260, Value = fatura.CARI_ACIKLAMASI ?? "" },
                        new Cell { ColumnId = 1554876251983748, Value = fatura.CARI_BAKIYESI },
                        new Cell { ColumnId = 1719803692404612, Value = fatura.PROJE_KODU ?? "" },
                        new Cell { ColumnId = 6223403319775108, Value = fatura.FATURA_NO ?? "" },
                        new Cell { ColumnId = 3971603506089860, Value = fatura.TARIHI?.ToString("yyyy-MM-dd") ?? "" },
                        new Cell { ColumnId = 4455123877842820, Value = fatura.FATURA_ACIKLAMASI ?? "" },
                        new Cell { ColumnId = 4402456749100932, Value = fatura.FATURA_VADE_TARIHI?.ToString("yyyy-MM-dd") ?? "" },
                        new Cell { ColumnId = 6995074985185156, Value = fatura.KUR },
                        new Cell { ColumnId = 8507499609804676, Value = fatura.PARA_BIRIMI == "TL" ? "TRY" : fatura.PARA_BIRIMI },
                        new Cell { ColumnId = 8906056376471428, Value = fatura.FATURA_TOPLAM_TUTAR_TL },
                        new Cell { ColumnId = 8958723505213316, Value = fatura.FATURA_TOPLAM_TUTAR_ID },
                        new Cell { ColumnId = 520388499689348, Value = fatura.KDV_TUTARI },
                        new Cell { ColumnId = 57077366738820, Value = fatura.FATURA_KDVSIZ_TUTAR },
                        new Cell { ColumnId = 6858151763332996, Value = fatura.MALZEME_BILGILERI ?? "" }
                    }
                };
                rows.Add(newRow);
            }
            return rows;
        }
        public static async Task<(bool Success, HashSet<string> FaturaKeys, string ErrorMessage)> GetGiderFaturaKeysAsync()
        {
            try
            {
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                Sheet sheet = await Task.Run(() =>
                    smartsheet.SheetResources.GetSheet(GIDER_SHEET_ID, null, null, null, null, null, null, null)
                );
                HashSet<string> faturaKeys = new HashSet<string>();
                long cariKoduColumnId = 7349303226617732;
                long faturaNoColumnId = 6223403319775108;
                foreach (var row in sheet.Rows)
                {
                    var cariKoduCell = row.Cells.FirstOrDefault(c => c.ColumnId == cariKoduColumnId);
                    var faturaNoCell = row.Cells.FirstOrDefault(c => c.ColumnId == faturaNoColumnId);
                    if (cariKoduCell?.Value != null && faturaNoCell?.Value != null)
                    {
                        string cariKodu = cariKoduCell.Value.ToString().Trim();
                        string faturaNo = faturaNoCell.Value.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(cariKodu) && !string.IsNullOrWhiteSpace(faturaNo))
                        {
                            string key = $"{cariKodu}|{faturaNo}";
                            faturaKeys.Add(key);
                        }
                    }
                }
                return (true, faturaKeys, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetGiderFaturaKeysAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        public static async Task<(bool Success, int Count, string ErrorMessage)> AddMultipleGiderFaturaAsync(List<GiderFaturaModel> faturalar)
        {
            try
            {
                if (faturalar == null || faturalar.Count == 0)
                    return (false, 0, "Fatura listesi boş!");
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, 0, tokenResult.ErrorMessage);
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                List<Row> rows = CreateGiderRows(faturalar);
                IList<Row> addedRows = await Task.Run(() =>
                    smartsheet.SheetResources.RowResources.AddRows(GIDER_SHEET_ID, rows)
                );
                return (true, addedRows.Count, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ AddMultipleGiderFaturaAsync hatası: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }
        #endregion

        #region GELİR Sheet Operations
        private static List<Row> CreateGelirRows(List<GelirFaturaModel> faturalar)
        {
            List<Row> rows = new List<Row>();
            foreach (GelirFaturaModel fatura in faturalar)
            {
                Row newRow = new Row
                {
                    ToTop = true,
                    Cells = new List<Cell>
                    {
                        new Cell { ColumnId = 2874156071473028, Value = fatura.CARI_KODU ?? "" },
                        new Cell { ColumnId = 7377755698843524, Value = fatura.CARI_ACIKLAMASI ?? "" },
                        new Cell { ColumnId = 1748256164630404, Value = fatura.PROJE_KODU ?? "" },
                        new Cell { ColumnId = 6251855792000900, Value = fatura.FATURA_NO ?? "" },
                        new Cell { ColumnId = 4000055978315652, Value = fatura.TARIHI?.ToString("yyyy-MM-dd") ?? "" },
                        new Cell { ColumnId = 8503655605686148, Value = fatura.FATURA_ACIKLAMASI ?? "" },
                        new Cell { ColumnId = 340881281077124, Value = fatura.FATURA_VADE_TARIHI?.ToString("yyyy-MM-dd") ?? "" },
                        new Cell { ColumnId = 2592681094762372, Value = fatura.KUR },
                        new Cell { ColumnId = 7096280722132868, Value = fatura.PARA_BIRIMI == "TL" ? "TRY" : fatura.PARA_BIRIMI },
                        new Cell { ColumnId = 1466781187919748, Value = fatura.CARI_BAKIYESI },
                        new Cell { ColumnId = 5970380815290244, Value = fatura.FATURA_TOPLAM_TUTAR_TL },
                        new Cell { ColumnId = 3718581001604996, Value = fatura.FATURA_TOPLAM_TUTAR_ID },
                        new Cell { ColumnId = 8222180628975492, Value = fatura.KDV_TUTARI },
                        new Cell { ColumnId = 903831234498436, Value = fatura.FATURA_KDVSIZ_TUTAR },
                        new Cell { ColumnId = 5407430861868932, Value = fatura.MALZEME_BILGILERI ?? "" }
                    }
                };
                rows.Add(newRow);
            }
            return rows;
        }
        public static async Task<(bool Success, HashSet<string> FaturaKeys, string ErrorMessage)> GetGelirFaturaKeysAsync()
        {
            try
            {
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                Sheet sheet = await Task.Run(() =>
                    smartsheet.SheetResources.GetSheet(GELIR_SHEET_ID, null, null, null, null, null, null, null)
                );
                HashSet<string> faturaKeys = new HashSet<string>();
                long cariKoduColumnId = 2874156071473028;
                long faturaNoColumnId = 6251855792000900;
                foreach (var row in sheet.Rows)
                {
                    var cariKoduCell = row.Cells.FirstOrDefault(c => c.ColumnId == cariKoduColumnId);
                    var faturaNoCell = row.Cells.FirstOrDefault(c => c.ColumnId == faturaNoColumnId);
                    if (cariKoduCell?.Value != null && faturaNoCell?.Value != null)
                    {
                        string cariKodu = cariKoduCell.Value.ToString().Trim();
                        string faturaNo = faturaNoCell.Value.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(cariKodu) && !string.IsNullOrWhiteSpace(faturaNo))
                        {
                            string key = $"{cariKodu}|{faturaNo}";
                            faturaKeys.Add(key);
                        }
                    }
                }
                return (true, faturaKeys, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetGelirFaturaKeysAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        public static async Task<(bool Success, int Count, string ErrorMessage)> AddMultipleGelirFaturaAsync(List<GelirFaturaModel> faturalar)
        {
            try
            {
                if (faturalar == null || faturalar.Count == 0)
                    return (false, 0, "Fatura listesi boş!");
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, 0, tokenResult.ErrorMessage);
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                List<Row> rows = CreateGelirRows(faturalar);
                IList<Row> addedRows = await Task.Run(() =>
                    smartsheet.SheetResources.RowResources.AddRows(GELIR_SHEET_ID, rows)
                );
                return (true, addedRows.Count, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ AddMultipleGelirFaturaAsync hatası: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }
        #endregion

        #region Expenses Sheet Operations
        public static async Task<(bool Success, List<GroupedExpenseModel> GroupedExpenses, string ErrorMessage)> GetGroupedApprovedExpensesAsync()
        {
            try
            {
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);

                List<ExpenseModel> allExpenses = new List<ExpenseModel>();

                long uidColId = 158077037531012;
                long kayitTarihiColId = 4661676664901508;
                long kayitEdenColId = 2409876851216260;
                long sirketAdiColId = 6913476478586756;
                long faturaTarihiColId = 3535776758058884;
                long faturaNoColId = 8039376385429380;
                long projeKoduColId = 5224626618322820;
                long faturaAciklamaColId = 1846926897794948;
                long dovizTuruColId = 6350526525165444;
                long amountColId = 8602326338850692;
                long malzemeListesiColId = 7125346611318660;
                long kdvColId = 439552014241668;
                long kdvOraniColId = 1035061510754180;
                long birimFiyatColId = 2240014585646980;
                long satirToplamColId = 8471080124239748;
                long muhasebeOnayColId = 7194951455297412;
                long yoneticiOnayColId = 6069051548454788;
                long supervisorApprovalColId = 1565451921084292;
                long archiveColId = 3817251734769540;
                long logoRefColId = 8320851362140036;
                long paymentTypeColId = 1283976944373636;
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
                    string getUrl = $"https://api.smartsheet.com/2.0/sheets/{EXPENSES_SHEET_ID}";
                    HttpResponseMessage getResponse = await http.GetAsync(getUrl);
                    string getJson = await getResponse.Content.ReadAsStringAsync();

                    if (!getResponse.IsSuccessStatusCode)
                    {
                        await TextLog.LogToSQLiteAsync($"❌ Smartsheet sheet çekme hatası: {getJson}");
                        return (false, null, $"Smartsheet sheet çekme hatası: {getResponse.StatusCode}");
                    }

                    JObject sheetObj = JObject.Parse(getJson);
                    JArray rows = sheetObj["rows"] as JArray ?? new JArray();

                    foreach (var row in rows)
                    {
                        long rowId = row["id"]?.Value<long>() ?? 0;
                        JArray cells = row["cells"] as JArray ?? new JArray();
                        string GetCellValue(long columnId)
                        {
                            var token = cells.FirstOrDefault(c => c["columnId"]?.Value<long>() == columnId)?["value"];
                            if (token == null) return null;

                            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                                return token.Value<double>().ToString(System.Globalization.CultureInfo.InvariantCulture);

                            // String ise virgülü noktaya çevir
                            return token.ToString().Replace(",", ".");
                        }
                        string muhasebeOnay = GetCellValue(muhasebeOnayColId);
                        string yoneticiOnay = GetCellValue(yoneticiOnayColId);
                        string supervisorOnay = GetCellValue(supervisorApprovalColId);

                        if (muhasebeOnay == "Rejected" || yoneticiOnay == "Rejected" || supervisorOnay == "Rejected")
                            continue;
                        if (muhasebeOnay != "Approved" || yoneticiOnay != "Approved" || supervisorOnay != "Approved")
                            continue;

                        ExpenseModel expense = new ExpenseModel
                        {
                            SmartsheetRowId = rowId,
                            UID = GetCellValue(uidColId),
                            KayitEdenKullanici = GetCellValue(kayitEdenColId),
                            SirketAdi = GetCellValue(sirketAdiColId),
                            FaturaNo = GetCellValue(faturaNoColId),
                            ProjeKodu = GetCellValue(projeKoduColId),
                            FaturaAciklamasi = GetCellValue(faturaAciklamaColId),
                            DovizTuru = GetCellValue(dovizTuruColId),
                            MalzemeListesi = GetCellValue(malzemeListesiColId),
                            KDV = GetCellValue(kdvColId),
                            MuhasebeOnay = muhasebeOnay,
                            SupervisorApproval = supervisorOnay,
                            YoneticiOnay = yoneticiOnay,
                            LogoReference = GetCellValue(logoRefColId)  ,
                            PaymentType = GetCellValue(paymentTypeColId)  // ← EKLENDİ
                        };

                        if (DateTime.TryParse(GetCellValue(kayitTarihiColId), out DateTime kayitTarihi))
                            expense.KayitTarihi = kayitTarihi;
                        if (DateTime.TryParse(GetCellValue(faturaTarihiColId), out DateTime faturaTarihi))
                            expense.FaturaTarihi = faturaTarihi;
                        if (decimal.TryParse(GetCellValue(amountColId), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount))
                            expense.Amount = amount;
                        if (decimal.TryParse(GetCellValue(kdvOraniColId), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal kdvOrani))
                            expense.KDVOrani = kdvOrani;
                        if (decimal.TryParse(GetCellValue(birimFiyatColId), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal birimFiyat))
                            expense.BirimFiyat = birimFiyat;
                        if (decimal.TryParse(GetCellValue(satirToplamColId), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal satirToplam))
                            expense.SatirToplamTutar = satirToplam;

                        string archiveVal = GetCellValue(archiveColId);
                        expense.Archive = archiveVal?.ToLower() == "true";

                        allExpenses.Add(expense);
                    }
                }

                // GRUPLAMA
                var grouped = allExpenses
                    .GroupBy(e => e.LogoReference?.Trim() ?? "")
                    .Select(g => new GroupedExpenseModel
                    {
                        LogoReference = g.Key,
                        FaturaNo = g.First().FaturaNo?.Trim() ?? "",
                        FaturaTarihi = g.First().FaturaTarihi,
                        FaturaAciklamasi = g.First().FaturaAciklamasi,
                        KayitEdenKullanici = g.First().KayitEdenKullanici?.Trim() ?? "",
                        SirketAdi = g.First().SirketAdi,
                        ProjeKodu = g.First().ProjeKodu,
                        DovizTuru = g.First().DovizTuru,
                        PaymentType = g.First().PaymentType,  // ← EKLENDİ
                        Items = g.ToList(),
                        ToplamTutar = g.Sum(x => x.SatirToplamTutar ?? 0)
                    })
                    .ToList();

                // ── BULK EMAIL → CARİ KODU ────────────────────────────────────────
                List<string> emailler = grouped
                    .Select(g => g.KayitEdenKullanici?.Trim() ?? "")
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .ToList();

                Dictionary<string, (string CariKodu, string Doviz)> emailCariMap =
                    new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

                if (emailler.Count > 0)
                {
                    var bulkCari = await BulutERPService.GetCariKodulariBulkAsync(emailler);
                    if (bulkCari.Success)
                        emailCariMap = bulkCari.EmailCariMap;
                    else
                        await TextLog.LogToSQLiteAsync($"❌ Bulk cari sorgusu hatası: {bulkCari.ErrorMessage}");
                }

                // ── BULK MALZEME SORGUSU ──────────────────────────────────────────
                List<string> malzemeKodlari = grouped
                    .SelectMany(g => g.Items)
                    .Select(i => {
                        string kod = i.MalzemeListesi ?? "";
                        if (kod.Contains("---"))
                            kod = kod.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();
                        return kod;
                    })
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct()
                    .ToList();

                Dictionary<string, int> malzemeMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (malzemeKodlari.Count > 0)
                {
                    var bulkMalzeme = await BulutERPService.GetMalzemeCardTypeBulkAsync(malzemeKodlari);
                    if (bulkMalzeme.Success)
                        malzemeMap = bulkMalzeme.MalzemeMap;
                    else
                        await TextLog.LogToSQLiteAsync($"❌ Bulk malzeme sorgusu hatası: {bulkMalzeme.ErrorMessage}");
                }

                // ── BULK PROJE KODU SORGUSU ───────────────────────────────────────
                List<string> projeKodlari = grouped
                    .Select(g => g.ProjeKodu?.Trim() ?? "")
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct()
                    .ToList();

                HashSet<string> gecerliProjeler = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (projeKodlari.Count > 0)
                {
                    var bulkProje = await BulutERPService.GetProjeKoduBulkAsync(projeKodlari);
                    if (bulkProje.Success)
                        gecerliProjeler = bulkProje.GecerliProjeler;
                    else
                        await TextLog.LogToSQLiteAsync($"❌ Bulk proje kodu sorgusu hatası: {bulkProje.ErrorMessage}");
                }

                // ── VALİDASYON ────────────────────────────────────────────────────
                foreach (GroupedExpenseModel grup in grouped)
                {
                    HashSet<string> malzemeHatalari = new HashSet<string>();

                    if (string.IsNullOrWhiteSpace(grup.LogoReference))
                        malzemeHatalari.Add("LOGO Accounting Reference # boş - bu satırlar hangi fişe ait bilinemiyor");

                    // Cari kodu ve döviz eşleşme kontrolü
                    string email = grup.KayitEdenKullanici?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        malzemeHatalari.Add("Email adresi boş!");
                    }
                    else if (emailCariMap.TryGetValue(email, out var cariInfo))
                    {
                        string smartsheetDoviz = grup.DovizTuru?.Trim().ToUpper() ?? "TRY";
                        string cariDoviz = cariInfo.Doviz;

                        bool eslesme = false;
                        if (smartsheetDoviz == "TRY" && cariDoviz == "TRY")
                            eslesme = true;
                        else if (smartsheetDoviz == "USD" && cariDoviz == "USD")
                            eslesme = true;
                        else if (smartsheetDoviz == "EURO" && cariDoviz == "EURO")
                            eslesme = true;

                        if (eslesme)
                        {
                            grup.CariKodu = cariInfo.CariKodu;
                            grup.CariDoviz = cariDoviz;
                        }
                        else
                            malzemeHatalari.Add($"Fatura dövizi '{smartsheetDoviz}' ile cari dövizi '{cariDoviz}' uyuşmuyor! Lütfen Logo'da doğru cari kartını kullanın.");
                    }
                    else
                    {
                        malzemeHatalari.Add($"Logo'da '{email}' email adresli cari bulunamadı. Lütfen Logo'da EMAIL alanını doldurun!");
                    }

                    // Proje kodu kontrolü
                    string projeKodu = grup.ProjeKodu?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(projeKodu) && !gecerliProjeler.Contains(projeKodu))
                        malzemeHatalari.Add($"Proje Kodu '{projeKodu}' Logo'da bulunamadı");

                    // Malzeme kontrolleri
                    foreach (ExpenseModel item in grup.Items)
                    {
                        string malzemeKodu = item.MalzemeListesi ?? "";
                        if (malzemeKodu.Contains("---"))
                            malzemeKodu = malzemeKodu.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();

                        if (string.IsNullOrWhiteSpace(malzemeKodu))
                            malzemeHatalari.Add("Malzeme seçilmemiş");
                        else if (!malzemeMap.ContainsKey(malzemeKodu))
                            malzemeHatalari.Add($"Malzeme '{malzemeKodu}' Logo'da yok");
                    }

                    grup.MalzemeHatalari = malzemeHatalari.ToList();
                }

                return (true, grouped, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetGroupedApprovedExpensesAsync hatası: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, null, ex.Message);
            }
        }
        #endregion


        public static async Task<(bool Success, int UpdatedCount, string ErrorMessage)> MarkAsTransferredToLogoAsync(List<long> rowIds)
        {
            try
            {
                if (rowIds == null || rowIds.Count == 0)
                    return (false, 0, "Row ID listesi boş!");

                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, 0, tokenResult.ErrorMessage);

                long logoyaGonderildiColumnId = 4239569086795652;

                var rowsToUpdate = rowIds.Select(rowId => new
                {
                    id = rowId,
                    cells = new[]
                    {
                new { columnId = logoyaGonderildiColumnId, value = true }
            }
                }).ToList();

                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
                    string putJson = JsonConvert.SerializeObject(rowsToUpdate);
                    StringContent content = new StringContent(putJson, System.Text.Encoding.UTF8, "application/json");
                    string putUrl = $"https://api.smartsheet.com/2.0/sheets/{EXPENSES_SHEET_ID}/rows";
                    HttpResponseMessage putResponse = await http.PutAsync(putUrl, content);
                    string putResult = await putResponse.Content.ReadAsStringAsync();
                    if (!putResponse.IsSuccessStatusCode)
                    {
                        await TextLog.LogToSQLiteAsync($"❌ Smartsheet PUT hatası: {putResult}");
                        return (false, 0, putResult);
                    }
                    await TextLog.LogToSQLiteAsync($"✅ Smartsheet checkbox güncellendi: {rowIds.Count} satır");
                    return (true, rowIds.Count, null);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ MarkAsTransferredToLogoAsync hatası: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }


        #region Cari Vade Bakiye Sheet Operations
        private static readonly long CVB_COL_CARI_KODU = 7906095132266372;
        private static readonly long CVB_COL_CARI_UNVAN = 587745737789316;
        private static readonly long CVB_COL_BAKIYE_TL = 5091345365159812;
        private static readonly long CVB_COL_BAKIYE_USD = 822369617399684;
        private static readonly long CVB_COL_BAKIYE_EUR = 5325969244770180;
        private static readonly long CVB_COL_BAKIYE_GBP = 4200069337927556;
        private static readonly long CVB_COL_VADE_TL = 3074169431084932;
        private static readonly long CVB_COL_VADE_USD = 7577769058455428;
        private static readonly long CVB_COL_VADE_EUR = 1948269524242308;
        private static readonly long CVB_COL_VADE_GBP = 6451869151612804;
        // Taylan Bey Onay: 540894640689028 — dokunmuyoruz
        public static async Task<(bool Success, int InsertCount, int UpdateCount, string ErrorMessage)>
            UpsertCariVadeBakiyeAsync(List<Dictionary<string, object>> kayitlar)
        {
            try
            {
                if (kayitlar == null || kayitlar.Count == 0)
                    return (false, 0, 0, "Kayıt listesi boş!");
                var tokenResult = await GetApiTokenAsync();
                if (!tokenResult.Success)
                    return (false, 0, 0, tokenResult.ErrorMessage);
                Dictionary<string, long> mevcutSatirlar = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                List<long> silinecekSatirlar = new List<long>();
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
                    // Sheet'i REST ile çek — SDK boş satırları atlıyor
                    string getUrl = $"https://api.smartsheet.com/2.0/sheets/{CARI_VADE_SHEET_ID}";
                    HttpResponseMessage getResponse = await http.GetAsync(getUrl);
                    string getJson = await getResponse.Content.ReadAsStringAsync();
                    JObject sheetObj = JObject.Parse(getJson);
                    JArray rows = sheetObj["rows"] as JArray ?? new JArray();
                    foreach (var row in rows)
                    {
                        long rowId = row["id"].Value<long>();
                        JArray cells = row["cells"] as JArray;
                        var cariKoduCell = cells?.FirstOrDefault(c => c["columnId"]?.Value<long>() == CVB_COL_CARI_KODU);
                        string ck = cariKoduCell?["value"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(ck))
                            silinecekSatirlar.Add(rowId);
                        else if (!mevcutSatirlar.ContainsKey(ck))
                            mevcutSatirlar[ck] = rowId;
                        else
                            silinecekSatirlar.Add(rowId); // duplicate
                    }
                    // Boş ve duplicate satırları sil
                    if (silinecekSatirlar.Count > 0)
                    {
                        int batchSize = 450;
                        for (int i = 0; i < silinecekSatirlar.Count; i += batchSize)
                        {
                            var batch = silinecekSatirlar.Skip(i).Take(batchSize).ToList();
                            string ids = string.Join(",", batch);
                            string deleteUrl = $"https://api.smartsheet.com/2.0/sheets/{CARI_VADE_SHEET_ID}/rows?ids={ids}&ignoreRowsNotFound=true";
                            await http.DeleteAsync(deleteUrl);
                        }
                        await TextLog.LogToSQLiteAsync($"🗑️ {silinecekSatirlar.Count} boş/duplicate satır silindi");
                    }
                    // Upsert
                    List<Row> rowsToInsert = new List<Row>();
                    List<object> rowsToUpdate = new List<object>();
                    foreach (var kayit in kayitlar)
                    {
                        string cariKodu = GetVal(kayit, "CARIKOD", "").ToString().Trim();
                        if (string.IsNullOrWhiteSpace(cariKodu)) continue;
                        var cells2 = new[]
                        {
                    new { columnId = CVB_COL_CARI_KODU,  value = (object)cariKodu },
                    new { columnId = CVB_COL_CARI_UNVAN, value = (object)GetVal(kayit, "CARIACIKLAMA", "").ToString().Trim() },
                    new { columnId = CVB_COL_BAKIYE_TL,  value = (object)ToDecimal(GetVal(kayit, "CARIBAKIYE", 0)) },
                    new { columnId = CVB_COL_BAKIYE_USD, value = (object)ToDecimal(GetVal(kayit, "CARIUSD", 0)) },
                    new { columnId = CVB_COL_BAKIYE_EUR, value = (object)ToDecimal(GetVal(kayit, "CARIEURO", 0)) },
                    new { columnId = CVB_COL_BAKIYE_GBP, value = (object)ToDecimal(GetVal(kayit, "CARIGBP", 0)) },
                    new { columnId = CVB_COL_VADE_TL,    value = (object)ToDecimal(GetVal(kayit, "VADESIGECMISBAKIYE", 0)) },
                    new { columnId = CVB_COL_VADE_USD,   value = (object)ToDecimal(GetVal(kayit, "VADESIGECMISUSD", 0)) },
                    new { columnId = CVB_COL_VADE_EUR,   value = (object)ToDecimal(GetVal(kayit, "VADESIGECMISEURO", 0)) },
                    new { columnId = CVB_COL_VADE_GBP,   value = (object)ToDecimal(GetVal(kayit, "VADESIGECMISGBP", 0)) }
                };

                        if (mevcutSatirlar.TryGetValue(cariKodu, out long rowId))
                            rowsToUpdate.Add(new { id = rowId, cells = cells2 });
                        else
                            rowsToInsert.Add(new Row { Cells = cells2.Select(c => new Cell { ColumnId = c.columnId, Value = c.value }).ToList(), ToBottom = true });
                    }
                    int updateCount = 0;
                    int insertCount = 0;
                    // UPDATE — REST
                    if (rowsToUpdate.Count > 0)
                    {
                        int batchSize = 500;
                        for (int i = 0; i < rowsToUpdate.Count; i += batchSize)
                        {
                            var batch = rowsToUpdate.Skip(i).Take(batchSize).ToList();
                            string putJson = Newtonsoft.Json.JsonConvert.SerializeObject(batch);
                            StringContent content = new StringContent(putJson, System.Text.Encoding.UTF8, "application/json");
                            HttpResponseMessage putResponse = await http.PutAsync($"https://api.smartsheet.com/2.0/sheets/{CARI_VADE_SHEET_ID}/rows", content);
                            if (putResponse.IsSuccessStatusCode)
                                updateCount += batch.Count;
                            else
                            {
                                string err = await putResponse.Content.ReadAsStringAsync();
                                await TextLog.LogToSQLiteAsync($"❌ CariVadeBakiye UPDATE hatası: {err}");
                            }
                        }
                        await TextLog.LogToSQLiteAsync($"✅ CariVadeBakiye UPDATE: {updateCount} satır");
                    }
                    // INSERT — SDK
                    if (rowsToInsert.Count > 0)
                    {
                        SmartsheetClient smartsheet = new SmartsheetBuilder().SetAccessToken(tokenResult.Token).Build();
                        int batchSize = 500;
                        for (int i = 0; i < rowsToInsert.Count; i += batchSize)
                        {
                            var batch = rowsToInsert.Skip(i).Take(batchSize).ToList();
                            var added = await Task.Run(() =>
                                smartsheet.SheetResources.RowResources.AddRows(CARI_VADE_SHEET_ID, batch)
                            );
                            insertCount += added.Count;
                        }
                        await TextLog.LogToSQLiteAsync($"✅ CariVadeBakiye INSERT: {insertCount} satır");
                    }
                    await TextLog.LogToSQLiteAsync($"✅ CariVadeBakiye tamamlandı — INSERT: {insertCount}, UPDATE: {updateCount}");
                    return (true, insertCount, updateCount, null);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ UpsertCariVadeBakiyeAsync hatası: {ex.Message}");
                return (false, 0, 0, ex.Message);
            }
        }
        private static object GetVal(Dictionary<string, object> row, string key, object defaultVal)
        {
            string match = row.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            return match != null && row[match] != null ? row[match] : defaultVal;
        }
        private static decimal ToDecimal(object val)
        {
            if (val == null) return 0m;
            try { return Convert.ToDecimal(val, System.Globalization.CultureInfo.InvariantCulture); }
            catch
            {
                return decimal.TryParse(val.ToString(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal d) ? d : 0m;
            }
        }
        #endregion

    }
}