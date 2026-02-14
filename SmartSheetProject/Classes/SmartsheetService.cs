using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
                    smartsheet.SheetResources.GetSheet(GIDER_SHEET_ID, null, null, null, null, null, null, null)
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
                SmartsheetClient smartsheet = new SmartsheetBuilder()
                    .SetAccessToken(tokenResult.Token)
                    .Build();
                Sheet sheet = await Task.Run(() =>
                    smartsheet.SheetResources.GetSheet(EXPENSES_SHEET_ID, null, null, null, null, null, null, null)
                );
                List<ExpenseModel> allExpenses = new List<ExpenseModel>();
                // Column ID'ler
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
                foreach (var row in sheet.Rows)
                {
                    // 3 onay kontrolü
                    string muhasebeOnay = row.Cells.FirstOrDefault(c => c.ColumnId == muhasebeOnayColId)?.Value?.ToString();
                    string yoneticiOnay = row.Cells.FirstOrDefault(c => c.ColumnId == yoneticiOnayColId)?.Value?.ToString();
                    string supervisorOnay = row.Cells.FirstOrDefault(c => c.ColumnId == supervisorApprovalColId)?.Value?.ToString();
                    if (muhasebeOnay != "Approved" || yoneticiOnay != "Approved" || supervisorOnay != "Approved")
                        continue;
                    ExpenseModel expense = new ExpenseModel
                    {
                        UID = row.Cells.FirstOrDefault(c => c.ColumnId == uidColId)?.Value?.ToString(),
                        KayitEdenKullanici = row.Cells.FirstOrDefault(c => c.ColumnId == kayitEdenColId)?.Value?.ToString(),
                        SirketAdi = row.Cells.FirstOrDefault(c => c.ColumnId == sirketAdiColId)?.Value?.ToString(),
                        FaturaNo = row.Cells.FirstOrDefault(c => c.ColumnId == faturaNoColId)?.Value?.ToString(),
                        ProjeKodu = row.Cells.FirstOrDefault(c => c.ColumnId == projeKoduColId)?.Value?.ToString(),
                        FaturaAciklamasi = row.Cells.FirstOrDefault(c => c.ColumnId == faturaAciklamaColId)?.Value?.ToString(),
                        DovizTuru = row.Cells.FirstOrDefault(c => c.ColumnId == dovizTuruColId)?.Value?.ToString(),
                        MalzemeListesi = row.Cells.FirstOrDefault(c => c.ColumnId == malzemeListesiColId)?.Value?.ToString(),
                        KDV = row.Cells.FirstOrDefault(c => c.ColumnId == kdvColId)?.Value?.ToString(),
                        MuhasebeOnay = muhasebeOnay,
                        SupervisorApproval = supervisorOnay,
                        YoneticiOnay = yoneticiOnay,
                        LogoReference = row.Cells.FirstOrDefault(c => c.ColumnId == logoRefColId)?.Value?.ToString()
                    };
                    // Tarih parse
                    if (DateTime.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == kayitTarihiColId)?.Value?.ToString(), out DateTime kayitTarihi))
                        expense.KayitTarihi = kayitTarihi;
                    if (DateTime.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == faturaTarihiColId)?.Value?.ToString(), out DateTime faturaTarihi))
                        expense.FaturaTarihi = faturaTarihi;
                    // Decimal parse
                    if (decimal.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == amountColId)?.Value?.ToString(), out decimal amount))
                        expense.Amount = amount;
                    if (decimal.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == kdvOraniColId)?.Value?.ToString(), out decimal kdvOrani))
                        expense.KDVOrani = kdvOrani;
                    if (decimal.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == birimFiyatColId)?.Value?.ToString(), out decimal birimFiyat))
                        expense.BirimFiyat = birimFiyat;
                    if (decimal.TryParse(row.Cells.FirstOrDefault(c => c.ColumnId == satirToplamColId)?.Value?.ToString(), out decimal satirToplam))
                        expense.SatirToplamTutar = satirToplam;
                    var archiveCell = row.Cells.FirstOrDefault(c => c.ColumnId == archiveColId);
                    expense.Archive = archiveCell?.Value?.ToString()?.ToLower() == "true";
                    allExpenses.Add(expense);
                }
                // GRUPLAMA
                var grouped = allExpenses
                    .GroupBy(e => new
                    {
                        FaturaNo = e.FaturaNo?.Trim() ?? "",
                        KayitEden = e.KayitEdenKullanici?.Trim() ?? ""
                    })
                    .Select(g => new GroupedExpenseModel
                    {
                        FaturaNo = g.Key.FaturaNo,
                        FaturaTarihi = g.First().FaturaTarihi,
                        FaturaAciklamasi = g.First().FaturaAciklamasi,
                        KayitEdenKullanici = g.Key.KayitEden,
                        SirketAdi = g.First().SirketAdi,
                        ProjeKodu = g.First().ProjeKodu,
                        DovizTuru = g.First().DovizTuru,
                        Items = g.ToList(),
                        ToplamTutar = g.Sum(x => x.SatirToplamTutar ?? 0)
                    })
                    .ToList();
                // Malzeme ve PROJE KODU validasyonu
                foreach (GroupedExpenseModel grup in grouped)
                {
                    HashSet<string> malzemeHatalari = new HashSet<string>();
                    // 🔴 PROJE KODU KONTROLÜ (Grup seviyesinde - tüm satırlar aynı proje koduna sahip)
                    string projeKodu = grup.ProjeKodu?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(projeKodu))
                    {
                        var projeResult = await BulutERPService.CheckProjeKoduExistsAsync(projeKodu);
                        if (!projeResult.Success || !projeResult.Exists)
                        {
                            malzemeHatalari.Add($"Proje Kodu '{projeKodu}' Logo'da bulunamadı");
                        }
                    }
                    // Malzeme kontrolleri (her satır için)
                    foreach (ExpenseModel item in grup.Items)
                    {
                        string malzemeKodu = item.MalzemeListesi ?? "";
                        if (malzemeKodu.Contains("---"))
                            malzemeKodu = malzemeKodu.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();
                        if (string.IsNullOrWhiteSpace(malzemeKodu))
                            malzemeHatalari.Add("Malzeme seçilmemiş");
                        else
                        {
                            var malzemeResult = await BulutERPService.GetMalzemeCardTypeAsync(malzemeKodu);
                            if (!malzemeResult.Success)
                                malzemeHatalari.Add($"Malzeme '{malzemeKodu}' Logo'da yok");
                        }
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
        public static async Task<(bool IsValid, List<string> Errors)> ValidateExpenseAsync(ExpenseModel expense)
        {
            List<string> hatalar = new List<string>();
            // Fatura No kontrolü
            if (string.IsNullOrWhiteSpace(expense.FaturaNo))
                hatalar.Add("Fatura No boş");
            // Fatura Tarihi kontrolü
            if (!expense.FaturaTarihi.HasValue || expense.FaturaTarihi.Value == DateTime.MinValue)
                hatalar.Add("Fatura Tarihi geçersiz");
            // 🔴 PROJE KODU KONTROLÜ
            string projeKodu = expense.ProjeKodu?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(projeKodu))
            {
                var projeResult = await BulutERPService.CheckProjeKoduExistsAsync(projeKodu);
                if (!projeResult.Success || !projeResult.Exists)
                    hatalar.Add($"Proje Kodu '{projeKodu}' Logo'da bulunamadı");
            }
            // Malzeme Kodu kontrolü
            string malzemeKodu = expense.MalzemeListesi ?? "";
            if (malzemeKodu.Contains("---"))
                malzemeKodu = malzemeKodu.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();
            if (string.IsNullOrWhiteSpace(malzemeKodu))
                hatalar.Add("Malzeme seçilmemiş");
            else
            {
                var malzemeResult = await BulutERPService.GetMalzemeCardTypeAsync(malzemeKodu);
                if (!malzemeResult.Success)
                    hatalar.Add($"Malzeme '{malzemeKodu}' Logo'da yok");
            }
            // Toplam Tutar kontrolü
            if (!expense.SatirToplamTutar.HasValue || expense.SatirToplamTutar.Value <= 0)
                hatalar.Add("Satır Toplam Tutar sıfır veya boş");
            // Email kontrolü
            if (string.IsNullOrWhiteSpace(expense.KayitEdenKullanici))
                hatalar.Add("Email adresi boş");
            bool isValid = hatalar.Count == 0;
            return (isValid, hatalar);
        }
        public static async Task<(bool Success, string CariKodu, string ErrorMessage)> GetCariKoduByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return (false, null, "Email adresi boş olamaz");
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                    return (false, null, tokenResult.ErrorMessage);
                string sqlQuery = $"SELECT CODE FROM U_$V(firm)_ARPS WHERE BOSTATUS<>1 AND UPPER(EMAIL) = UPPER('{email.Replace("'", "''")}')";
                var result = await BulutERPService.ExecuteSelectQueryAsync(sqlQuery, tokenResult.AccessToken, 1);
                if (!result.Success)
                    return (false, null, result.ErrorMessage);
                if (result.Data == null || result.Data.Count == 0)
                    return (false, null, $"Logo'da '{email}' email adresli cari bulunamadı. Lütfen Logo'da EMAIL alanını doldurun!");
                string cariKodu = result.Data[0].ContainsKey("CODE") ? result.Data[0]["CODE"]?.ToString() : null;
                if (string.IsNullOrWhiteSpace(cariKodu))
                    return (false, null, "Cari kodu boş geldi");
                return (true, cariKodu, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GetCariKoduByEmailAsync hatası: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
        #endregion
    }
}