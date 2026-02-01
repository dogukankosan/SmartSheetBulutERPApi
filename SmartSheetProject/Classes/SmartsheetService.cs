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
                {
                    await TextLog.LogToSQLiteAsync($"❌ API Token kaydetme hatası: {result.ErrorMessage}");
                }
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
                {
                    await TextLog.LogToSQLiteAsync("❌ API Token bulunamadı");
                    return (false, null, "API Token bulunamadı!");
                }
                string encryptedToken = dt.Rows[0]["ApiToken"].ToString();
                if (string.IsNullOrWhiteSpace(encryptedToken))
                {
                    await TextLog.LogToSQLiteAsync("❌ API Token kayıtlı değil");
                    return (false, null, "API Token kayıtlı değil!");
                }
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
            return await Task.Run(async () =>
            {
                try
                {
                    var tokenResult = await GetApiTokenAsync();
                    if (!tokenResult.Success)
                        return (false, tokenResult.ErrorMessage);
                    SmartsheetClient smartsheet = new SmartsheetBuilder()
                        .SetAccessToken(tokenResult.Token)
                        .Build();
                    Sheet sheet = smartsheet.SheetResources.GetSheet(GIDER_SHEET_ID, null, null, null, null, null, null, null);
                    return (true, $"Bağlantı başarılı! Sheet: {sheet.Name}");
                }
                catch (Exception ex)
                {
                    await TextLog.LogToSQLiteAsync($"❌ TestConnectionAsync hatası: {ex.Message}");
                    return (false, ex.Message);
                }
            });
        }
        #endregion

        #region GİDER Sheet Operations

        /// <summary>
        /// GİDER sheet için Row listesi oluşturur
        /// </summary>
        private static List<Row> CreateGiderRows(List<GiderFaturaModel> faturalar)
        {
            List<Row> rows = new List<Row>();
            foreach (var fatura in faturalar)
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
                        new Cell { ColumnId = 6274768167456644, Value = fatura.VADE_KALAN_GUN },
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

        /// <summary>
        /// GİDER sheet'teki tüm fatura no + cari kodu çifti çeker
        /// </summary>
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
                Sheet sheet = smartsheet.SheetResources.GetSheet(GIDER_SHEET_ID, null, null, null, null, null, null, null);
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
        /// <summary>
        /// Tüm faturaları GİDER sheet'ine ekler
        /// </summary>
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
                IList<Row> addedRows = smartsheet.SheetResources.RowResources.AddRows(GIDER_SHEET_ID, rows);
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

        /// <summary>
        /// GELİR sheet için Row listesi oluşturur
        /// </summary>
        private static List<Row> CreateGelirRows(List<GelirFaturaModel> faturalar)
        {
            List<Row> rows = new List<Row>();
            foreach (var fatura in faturalar)
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
                        new Cell { ColumnId = 4844480908447620, Value = fatura.VADE_KALAN_GUN },
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
        /// <summary>
        /// GELİR sheet'teki tüm fatura no + cari kodu çifti çeker
        /// </summary>
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
                Sheet sheet = smartsheet.SheetResources.GetSheet(GELIR_SHEET_ID, null, null, null, null, null, null, null);
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
        /// <summary>
        /// Tüm faturaları GELİR sheet'ine ekler
        /// </summary>
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
                IList<Row> addedRows = smartsheet.SheetResources.RowResources.AddRows(GELIR_SHEET_ID, rows);
                return (true, addedRows.Count, null);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ AddMultipleGelirFaturaAsync hatası: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }
        #endregion

    }
}