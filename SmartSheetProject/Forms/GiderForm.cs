using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;
using SmartSheetProject.Models;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Diagnostics;

namespace SmartSheetProject.Forms
{
    public partial class GiderForm : XtraForm
    {
        private List<GiderFaturaModel> tumFaturalar = new List<GiderFaturaModel>();
        private HashSet<string> sheetFaturaKeys = new HashSet<string>();
        private Timer sheetCheckTimer;
        public GiderForm()
        {
            InitializeComponent();
            InitializeTimer();
        }
        private string CreateFaturaKey(string cariKodu, string faturaNo)
        {
            return $"{cariKodu?.Trim() ?? ""}|{faturaNo?.Trim() ?? ""}";
        }
        private void InitializeTimer()
        {
            sheetCheckTimer = new System.Windows.Forms.Timer();
            sheetCheckTimer.Interval = 30000;
            sheetCheckTimer.Tick += async (s, e) => await LoadSheetFaturaKeysAsync();
        }
        private async void GiderForm_Load(object sender, EventArgs e)
        {
            try
            {
                var bulutERPSettingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!bulutERPSettingsResult.Success)
                {
                    await TextLog.LogToSQLiteAsync("❌ GiderForm - Bulut ERP ayarları bulunamadı");
                    XtraMessageBox.Show(
                        "Bulut ERP ayarları bulunamadı! Lütfen önce Bulut ERP ayarlarını yapılandırın.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
                bool smartsheetTokenKayitli = await SmartsheetService.IsTokenSavedAsync();
                if (!smartsheetTokenKayitli)
                {
                    await TextLog.LogToSQLiteAsync("❌ GiderForm - SmartSheet API Token kayıtlı değil");
                    XtraMessageBox.Show(
                        "SmartSheet API Token kayıtlı değil! Lütfen önce SmartSheet ayarlarını yapılandırın.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
                ConfigureGrid();
                dateBaslangic.EditValue = DateTime.Now.Date;
                dateBitis.EditValue = DateTime.Now.Date;
                dateBaslangic.EditValueChanged += DateEdit_Changed;
                dateBitis.EditValueChanged += DateEdit_Changed;
                btnFiltrele.Click += BtnFiltrele_Click;
                btnYenile.ItemClick += BtnYenile_ItemClick;
                btnExcel.ItemClick += BtnExcel_ItemClick;
                gridView1.RowStyle += GridView1_RowStyle;
               gridView1.ColumnFilterChanged += (s, ev) => gridView1.RefreshData();
                sheetCheckTimer.Start();
                 await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GiderForm_Load hatası: {ex.Message}");
                XtraMessageBox.Show($"Form yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        private async void DateEdit_Changed(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }
        private void ConfigureGrid()
        {
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowAutoFilterRow = true;
            gridView1.OptionsView.ShowFooter = true;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            gridView1.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DevExpress.Utils.DefaultBoolean.True;
        }
        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                gridControl1.DataSource = null;
                await LoadSheetFaturaKeysAsync();
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ GiderForm - Token alınamadı: {tokenResult.ErrorMessage}");
                    XtraMessageBox.Show($"Token alınamadı:\n{tokenResult.ErrorMessage}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DateTime baslangicTarihi = dateBaslangic.EditValue != null
                    ? Convert.ToDateTime(dateBaslangic.EditValue).Date
                    : DateTime.Now.Date;
                DateTime bitisTarihi = dateBitis.EditValue != null
                    ? Convert.ToDateTime(dateBitis.EditValue).Date
                    : DateTime.Now.Date;
                string sqlQuery = "SELECT INV.LOGICALREF AS FATURALOGICALREF, ARP.CODE AS CARI_KODU, ARP.DESCRIPTION AS CARI_ACIKLAMASI, INV.AUXCODE AS PROJE_KODU, INV.SLIPNR AS FATURA_NO, INV.SLIPDATE::DATE AS TARIHI, INV.GENEXP AS FATURA_ACIKLAMASI, INV.SLIPDATE::DATE + COALESCE(PAY.PAYDAY::INTEGER, 0) AS FATURA_VADE_TARIHI, GREATEST(0, (INV.SLIPDATE::DATE + COALESCE(PAY.PAYDAY::INTEGER, 0)) - CURRENT_DATE) AS VADE_KALAN_GUN, ROUND(MAX(MMT.TCRATE)::NUMERIC, 4) AS KUR, CASE WHEN INV.TCTYPE = 0 THEN 'TL' WHEN INV.TCTYPE = 1 THEN 'USD' WHEN INV.TCTYPE = 20 THEN 'EURO' ELSE 'KONTROL_EDILECEK' END AS PARA_BIRIMI, COALESCE((SELECT SUM(CASE WHEN ARPTRN.TRANSSIGN=0 THEN ARPTRN.AMOUNT ELSE -ARPTRN.AMOUNT END) FROM U_$V(firm)_01_ARPTRANS ARPTRN WHERE  ARPTRN.ARPREF=ARP.LOGICALREF AND ARPTRN.TRANSDATE <= INV.SLIPDATE AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2)),0) AS CARI_BAKIYESI, ROUND(INV.NETTOTAL::NUMERIC, 2) AS FATURA_TOPLAM_TUTAR_TL, ROUND(INV.TCNET::NUMERIC, 2) AS FATURA_TOPLAM_TUTAR_ID, ROUND(INV.GROSSTOTAL::NUMERIC, 2) AS FATURA_KDVSIZ_TUTAR, ROUND(INV.TOTALVAT::NUMERIC, 2) AS KDV_TUTARI, STRING_AGG(ITM.CODE || ' - ' || ITM.DESCRIPTION || ' - Miktar: ' || ROUND(MMT.QUANTITY::NUMERIC, 2) || ' ' || COALESCE(UNT.CODE, '') || ' - Fiyat: ' || ROUND(MMT.PRICE::NUMERIC, 2) || ' TL - Toplam: ' || ROUND(MMT.TOTAL::NUMERIC, 2) || ' TL', ' | ') AS MALZEME_BILGILERI FROM U_$V(firm)_01_INVOICES INV LEFT JOIN U_$V(firm)_01_MMTRANS MMT ON MMT.INVOICEREF = INV.LOGICALREF LEFT JOIN U_$V(firm)_ITEMS ITM ON ITM.LOGICALREF = MMT.ITEMREF LEFT JOIN U_$V(firm)_UNITS UNT ON UNT.LOGICALREF = MMT.UOMREF LEFT JOIN U_$V(firm)_ARPS ARP ON ARP.LOGICALREF = MMT.ARPREF LEFT JOIN U_$V(firm)_PAYPLANLNS PAY ON PAY.LOGICALREF = INV.PAYPLANREF WHERE ARP.CODE NOT LIKE '195%' AND INV.SLIPTYPE IN (1,2,3,4,21) AND INV.SLIPDATE::DATE >= '" + baslangicTarihi.ToString("yyyy-MM-dd") + "' AND INV.SLIPDATE::DATE <= '" + bitisTarihi.ToString("yyyy-MM-dd") + "' GROUP BY INV.LOGICALREF, ARP.CODE, ARP.DESCRIPTION, ARP.LOGICALREF, INV.AUXCODE, INV.SLIPNR, INV.SLIPDATE, INV.GENEXP, PAY.PAYDAY, INV.NETTOTAL, INV.TCNET, INV.GROSSTOTAL, INV.TOTALVAT, INV.TCTYPE ORDER BY INV.SLIPDATE DESC";
                var result = await BulutERPService.ExecuteSelectQueryAsync(sqlQuery, tokenResult.AccessToken, 10000);
                if (!result.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ GiderForm - BulutERP veri çekme hatası: {result.ErrorMessage}");
                    XtraMessageBox.Show($"Veri çekilirken hata oluştu:\n{result.ErrorMessage}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tumFaturalar = ConvertToModel(result.Data);
                gridControl1.DataSource = tumFaturalar;
                ConfigureColumns();
                SetFooterTotals();
                gridView1.BestFitColumns();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GiderForm LoadDataAsync hatası: {ex.Message}");
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private async Task LoadSheetFaturaKeysAsync()
        {
            try
            {
                sheetFaturaKeys.Clear();
                var result = await SmartsheetService.GetGiderFaturaKeysAsync();
                if (result.Success && result.FaturaKeys != null)
                {
                    sheetFaturaKeys = result.FaturaKeys;
                    if (gridView1 != null)
                        gridView1.RefreshData();
                }
                else
                    await TextLog.LogToSQLiteAsync($"❌ GiderForm - Sheet fatura key çekme hatası: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GiderForm LoadSheetFaturaKeysAsync hatası: {ex.Message}");
            }
        }
        private void GridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var fatura = gridView1.GetRow(e.RowHandle) as GiderFaturaModel;
                if (fatura != null)
                {
                    string key = CreateFaturaKey(fatura.CARI_KODU, fatura.FATURA_NO);
                    if (sheetFaturaKeys.Contains(key))
                    {
                        e.Appearance.BackColor = Color.LightGreen;
                        e.Appearance.ForeColor = Color.Black;
                    }
                }
            }
        }
        private async void BtnFiltrele_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }
        private List<GiderFaturaModel> ConvertToModel(List<Dictionary<string, object>> data)
        {
            List<GiderFaturaModel> list = new List<GiderFaturaModel>();
            foreach (var dict in data)
            {
                try
                {
                    GiderFaturaModel model = new GiderFaturaModel
                    {
                        FATURALOGICALREF = dict.ContainsKey("FATURALOGICALREF") ? Convert.ToInt32(dict["FATURALOGICALREF"]) : 0,
                        FATURA_NO = dict.ContainsKey("FATURA_NO") ? dict["FATURA_NO"]?.ToString() : "",
                        TARIHI = dict.ContainsKey("TARIHI") && dict["TARIHI"] != null ? Convert.ToDateTime(dict["TARIHI"]) : (DateTime?)null,
                        FATURA_VADE_TARIHI = dict.ContainsKey("FATURA_VADE_TARIHI") && dict["FATURA_VADE_TARIHI"] != null ? Convert.ToDateTime(dict["FATURA_VADE_TARIHI"]) : (DateTime?)null,
                        VADE_KALAN_GUN = dict.ContainsKey("VADE_KALAN_GUN") ? Convert.ToInt32(dict["VADE_KALAN_GUN"]) : 0,
                        CARI_KODU = dict.ContainsKey("CARI_KODU") ? dict["CARI_KODU"]?.ToString() : "",
                        CARI_ACIKLAMASI = dict.ContainsKey("CARI_ACIKLAMASI") ? dict["CARI_ACIKLAMASI"]?.ToString() : "",
                        CARI_BAKIYESI = dict.ContainsKey("CARI_BAKIYESI") ? Convert.ToDecimal(dict["CARI_BAKIYESI"]) : 0,
                        PROJE_KODU = dict.ContainsKey("PROJE_KODU") ? dict["PROJE_KODU"]?.ToString() : "",
                        PARA_BIRIMI = dict.ContainsKey("PARA_BIRIMI") ? dict["PARA_BIRIMI"]?.ToString() : "",
                        KUR = dict.ContainsKey("KUR") ? Convert.ToDecimal(dict["KUR"]) : 0,
                        FATURA_KDVSIZ_TUTAR = dict.ContainsKey("FATURA_KDVSIZ_TUTAR") ? Convert.ToDecimal(dict["FATURA_KDVSIZ_TUTAR"]) : 0,
                        KDV_TUTARI = dict.ContainsKey("KDV_TUTARI") ? Convert.ToDecimal(dict["KDV_TUTARI"]) : 0,
                        FATURA_TOPLAM_TUTAR_TL = dict.ContainsKey("FATURA_TOPLAM_TUTAR_TL") ? Convert.ToDecimal(dict["FATURA_TOPLAM_TUTAR_TL"]) : 0,
                        FATURA_TOPLAM_TUTAR_ID = dict.ContainsKey("FATURA_TOPLAM_TUTAR_ID") ? Convert.ToDecimal(dict["FATURA_TOPLAM_TUTAR_ID"]) : 0,
                        FATURA_ACIKLAMASI = dict.ContainsKey("FATURA_ACIKLAMASI") ? dict["FATURA_ACIKLAMASI"]?.ToString() : "",
                        MALZEME_BILGILERI = dict.ContainsKey("MALZEME_BILGILERI") ? dict["MALZEME_BILGILERI"]?.ToString() : ""
                    };
                    list.Add(model);
                }
                catch (Exception ex)
                {
                    TextLog.LogToSQLiteAsync($"❌ GiderForm ConvertToModel hatası: {ex.Message}").Wait();
                }
            }
            return list;
        }
        private void ConfigureColumns()
        {
            var kolonlar = new List<(string FieldName, string Caption, int VisibleIndex, int Width)>
            {
                ("FATURALOGICALREF", "Fatura ID", 0, 80),
                ("FATURA_NO", "Fatura No", 1, 150),
                ("TARIHI", "Tarih", 2, 100),
                ("FATURA_VADE_TARIHI", "Vade Tarihi", 3, 100),
                ("VADE_KALAN_GUN", "Kalan Gün", 4, 80),
                ("CARI_KODU", "Cari Kodu", 5, 120),
                ("CARI_ACIKLAMASI", "Cari Adı", 6, 200),
                ("PROJE_KODU", "Proje", 7, 120),
                ("PARA_BIRIMI", "Para Birimi", 8, 80),
                ("KUR", "Kur", 9, 80),
                ("FATURA_KDVSIZ_TUTAR", "KDV'siz Tutar", 10, 120),
                ("KDV_TUTARI", "KDV", 11, 100),
                ("FATURA_TOPLAM_TUTAR_TL", "Toplam (TL)", 12, 120),
                ("FATURA_TOPLAM_TUTAR_ID", "Toplam (ID)", 13, 120),
                ("FATURA_ACIKLAMASI", "Fatura Açıklaması", 14, 250),
                ("MALZEME_BILGILERI", "Malzeme Bilgileri", 15, 400)
            };
            foreach (var kolon in kolonlar)
            {
                GridColumn col = gridView1.Columns[kolon.FieldName];
                if (col != null)
                {
                    col.Caption = kolon.Caption;
                    col.VisibleIndex = kolon.VisibleIndex;
                    col.Width = kolon.Width;
                    if (kolon.FieldName.Contains("TUTAR") || kolon.FieldName == "KDV_TUTARI" || kolon.FieldName == "KUR")
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "N2";
                    }
                    if (kolon.FieldName.Contains("TARIH"))
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        col.DisplayFormat.FormatString = "dd.MM.yyyy";
                    }
                }
            }
            if (gridView1.Columns["CARI_BAKIYESI"] != null)
                gridView1.Columns["CARI_BAKIYESI"].Visible = false;
        }
        private void SetFooterTotals()
        {
           string[] sumColumns = { "FATURA_KDVSIZ_TUTAR", "KDV_TUTARI", "FATURA_TOPLAM_TUTAR_TL" };
            foreach (GridColumn col in gridView1.Columns)
            {
                if (sumColumns.Contains(col.FieldName))
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2} TL";
                }
            }
            if (gridView1.Columns["FATURA_NO"] != null)
            {
                gridView1.Columns["FATURA_NO"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
                gridView1.Columns["FATURA_NO"].SummaryItem.DisplayFormat = "Toplam: {0} Fatura";
            }
        }
        private async void BtnYenile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await LoadDataAsync();
        }
        private async void BtnExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (tumFaturalar.Count == 0)
                {
                    XtraMessageBox.Show("Excel'e aktarılacak veri yok!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                    FileName = $"Gider_Faturalari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    gridView1.ExportToXlsx(saveDialog.FileName);
                    // Dosyayı açmak ister misiniz?
                    DialogResult openResult = XtraMessageBox.Show(
                        "Excel dosyası başarıyla oluşturuldu!\n\nDosyayı açmak ister misiniz?",
                        "Başarılı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (openResult == DialogResult.Yes)
                       Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GiderForm Excel aktarma hatası: {ex.Message}");
                XtraMessageBox.Show($"Excel aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btn_Aktar_Click(object sender, EventArgs e)
        {
            if (!btn_Aktar.Enabled)
                return;
            btn_Aktar.Enabled = false;
            try
            {
                int[] selectedRows = gridView1.GetSelectedRows();
                if (selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen aktarılacak faturaları seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                List<GiderFaturaModel> secilifaturalar = new List<GiderFaturaModel>();
                List<string> varOlanFaturalar = new List<string>();
                foreach (int rowHandle in selectedRows)
                {
                    if (rowHandle < 0) continue;
                    GiderFaturaModel fatura = gridView1.GetRow(rowHandle) as GiderFaturaModel;
                    if (fatura != null)
                    {
                        string key = CreateFaturaKey(fatura.CARI_KODU, fatura.FATURA_NO);
                        if (sheetFaturaKeys.Contains(key))
                            varOlanFaturalar.Add($"{fatura.CARI_KODU} - {fatura.FATURA_NO}");
                        else
                            secilifaturalar.Add(fatura);
                    }
                }
                if (secilifaturalar.Count == 0)
                {
                    XtraMessageBox.Show(
                        "Seçtiğiniz tüm faturalar zaten SmartSheet'te mevcut!\n\n" +
                        $"Mevcut faturalar ({varOlanFaturalar.Count} adet):\n{string.Join("\n", varOlanFaturalar.Take(10))}" +
                        (varOlanFaturalar.Count > 10 ? $"\n...ve {varOlanFaturalar.Count - 10} fatura daha" : ""),
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    gridView1.ClearSelection();
                    return;
                }
                string onayMesaji = $"{secilifaturalar.Count} adet yeni fatura GİDER SmartSheet'e aktarılacak.";
                if (varOlanFaturalar.Count > 0)
                {
                    onayMesaji += $"\n\n⚠️ {varOlanFaturalar.Count} fatura zaten mevcut (atlanacak):\n";
                    onayMesaji += string.Join("\n", varOlanFaturalar.Take(5));
                    if (varOlanFaturalar.Count > 5)
                        onayMesaji += $"\n...ve {varOlanFaturalar.Count - 5} fatura daha";
                }
                onayMesaji += "\n\nDevam etmek istiyor musunuz?";
                DialogResult onay = XtraMessageBox.Show(onayMesaji, "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (onay != DialogResult.Yes)
                    return;
                this.Cursor = Cursors.WaitCursor;
                var result = await SmartsheetService.AddMultipleGiderFaturaAsync(secilifaturalar);
                if (result.Success)
                {
                    await LoadSheetFaturaKeysAsync();

                    string basariMesaji = $"Aktarım başarılı!\n✅ {result.Count} fatura GİDER sheet'ine eklendi";
                    if (varOlanFaturalar.Count > 0)
                        basariMesaji += $"\n⏭️ {varOlanFaturalar.Count} fatura zaten mevcuttu (atlandı)";
                    XtraMessageBox.Show(basariMesaji, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await TextLog.LogToSQLiteAsync($"❌ GiderForm aktarım hatası: {result.ErrorMessage}");
                    XtraMessageBox.Show($"Aktarım hatası:\n{result.ErrorMessage}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                gridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ GiderForm btn_Aktar_Click hatası: {ex.Message}");
                XtraMessageBox.Show($"Aktarım hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btn_Aktar.Enabled = true;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (sheetCheckTimer != null)
            {
                sheetCheckTimer.Stop();
                sheetCheckTimer.Dispose();
            }
        }
        private void GiderForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}