using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using SmartSheetProject.Classes;
using System.Diagnostics;
using System.Data;
using System.Globalization;

namespace SmartSheetProject.Forms
{
    public partial class CustomerMaturityForm : XtraForm
    {
        private List<Dictionary<string, object>> tumKayitlar = new List<Dictionary<string, object>>();
        public CustomerMaturityForm()
        {
            InitializeComponent();
        }
        private async void CustomerMaturityForm_Load(object sender, EventArgs e)
        {
            try
            {
                var bulutERPSettingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!bulutERPSettingsResult.Success)
                {
                    await TextLog.LogToSQLiteAsync("CustomerMaturityForm - Bulut ERP ayarlari bulunamadi");
                    XtraMessageBox.Show("Bulut ERP ayarlari bulunamadi! Lutfen once Bulut ERP ayarlarini yapilandirin.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
                ConfigureGrid();
              //  await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync("CustomerMaturityForm_Load hatasi: " + ex.Message);
                XtraMessageBox.Show("Form yuklenirken hata olustu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        private void ConfigureGrid()
        {
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowAutoFilterRow = true;
            gridView1.OptionsView.ShowFooter = true;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView1.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DevExpress.Utils.DefaultBoolean.True;  
            gridView1.SelectionChanged += GridView1_SelectionChanged;
        }
        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                gridControl1.DataSource = null;
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    await TextLog.LogToSQLiteAsync("CustomerMaturityForm - Token alinamadi: " + tokenResult.ErrorMessage);
                    XtraMessageBox.Show("Token alinamadi:\n" + tokenResult.ErrorMessage, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // SQLite'tan hariç tutulacak cari kodlarını çek
                string notInClause = "";
                DataTable dtCodes = await SQLiteCrud.GetDataFromSQLiteAsync("SELECT CustomerCode FROM CustomerCodes");
                if (dtCodes.Rows.Count > 0)
                {
                    var kodlar = dtCodes.AsEnumerable()
                        .Select(r => "'" + r["CustomerCode"].ToString().Trim().Replace("'", "''") + "'")
                        .ToList();
                    notInClause = $"AND ARP.CODE NOT IN ({string.Join(",", kodlar)})";
                }
                string sqlVade = $@"SELECT ARP.CODE AS CariKod, ARP.DESCRIPTION AS CariAciklama, COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0) AS CariBakiye, ROUND(COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN (CASE WHEN ARPTRN.TRANSSIGN=1 THEN ARPTRN.TCNET*-1 ELSE ARPTRN.TCNET END) END),0), 2) AS CariUSD, ROUND(COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=20 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN (CASE WHEN ARPTRN.TRANSSIGN=1 THEN ARPTRN.TCNET*-1 ELSE ARPTRN.TCNET END) END),0), 2) AS CariEURO, ROUND(COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=17 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN (CASE WHEN ARPTRN.TRANSSIGN=1 THEN ARPTRN.TCNET*-1 ELSE ARPTRN.TCNET END) END),0), 2) AS CariGBP, CASE WHEN (SELECT COUNT(*) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.BOSTATUS IN (1,2)) = 0 THEN 0 ELSE ROUND(GREATEST((COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0)) - COALESCE((SELECT SUM(CASE WHEN INV2.SLIPTYPE IN (1,4,6,13) THEN INV2.NETTOTAL ELSE 0 END) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.BOSTATUS IN (1,2) AND (INV2.SLIPDATE::date + CAST(REGEXP_REPLACE(PP2.DESCRIPTION, '[^0-9]', '', 'g') AS INT)) > CURRENT_DATE),0), 0), 2) END AS VadesiGecmisTL, CASE WHEN (SELECT COUNT(*) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.BOSTATUS IN (1,2) AND INV2.TCTYPE=1) = 0 THEN 0 ELSE ROUND(GREATEST((COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=1 AND ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=1 AND ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0)) - COALESCE((SELECT SUM(CASE WHEN INV2.SLIPTYPE IN (1,4,6,13) THEN INV2.TCNET ELSE 0 END) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.TCTYPE=1 AND INV2.BOSTATUS IN (1,2) AND (INV2.SLIPDATE::date + CAST(REGEXP_REPLACE(PP2.DESCRIPTION, '[^0-9]', '', 'g') AS INT)) > CURRENT_DATE),0), 0), 2) END AS VadesiGecmisUSD, CASE WHEN (SELECT COUNT(*) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.BOSTATUS IN (1,2) AND INV2.TCTYPE=20) = 0 THEN 0 ELSE ROUND(GREATEST((COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=20 AND ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=20 AND ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0)) - COALESCE((SELECT SUM(CASE WHEN INV2.SLIPTYPE IN (1,4,6,13) THEN INV2.TCNET ELSE 0 END) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.TCTYPE=20 AND INV2.BOSTATUS IN (1,2) AND (INV2.SLIPDATE::date + CAST(REGEXP_REPLACE(PP2.DESCRIPTION, '[^0-9]', '', 'g') AS INT)) > CURRENT_DATE),0), 0), 2) END AS VadesiGecmisEURO, CASE WHEN (SELECT COUNT(*) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.BOSTATUS IN (1,2) AND INV2.TCTYPE=17) = 0 THEN 0 ELSE ROUND(GREATEST((COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=17 AND ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE=17 AND ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0)) - COALESCE((SELECT SUM(CASE WHEN INV2.SLIPTYPE IN (1,4,6,13) THEN INV2.TCNET ELSE 0 END) FROM U_$V(firm)_01_INVOICES INV2 JOIN U_$V(firm)_PAYPLANS PP2 ON PP2.LOGICALREF = INV2.PAYPLANREF WHERE INV2.ARPREF = ARP.LOGICALREF AND INV2.TCTYPE=17 AND INV2.BOSTATUS IN (1,2) AND (INV2.SLIPDATE::date + CAST(REGEXP_REPLACE(PP2.DESCRIPTION, '[^0-9]', '', 'g') AS INT)) > CURRENT_DATE),0), 0), 2) END AS VadesiGecmisGBP FROM U_$V(firm)_ARPS ARP LEFT JOIN U_$V(firm)_01_ARPTRANS ARPTRN ON ARPTRN.ARPREF=ARP.LOGICALREF WHERE ARP.CODE LIKE '320%' {notInClause} GROUP BY ARP.CODE, ARP.DESCRIPTION, ARP.LOGICALREF HAVING (COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.AMOUNT END),0)) != 0 OR (COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE IN (1,20,17) AND ARPTRN.TRANSSIGN=0 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0) - COALESCE(SUM(CASE WHEN ARPTRN.TCTYPE IN (1,20,17) AND ARPTRN.TRANSSIGN=1 AND ARPTRN.MODULENR IN (3,4,5,6,7,8,9,15,17) AND ARPTRN.NOTEFFECTSTOTALS=0 AND ARPTRN.BOSTATUS IN (1,2) THEN ARPTRN.TCNET END),0)) != 0 ORDER BY ARP.CODE";
                var vadeResult = await BulutERPService.ExecuteSelectQueryAsync(sqlVade, tokenResult.AccessToken, 100000);
                if (!vadeResult.Success)
                {
                    await TextLog.LogToSQLiteAsync("CustomerMaturityForm - Sorgu hatasi: " + vadeResult.ErrorMessage);
                    XtraMessageBox.Show("Veri cekilirken hata olustu:\n" + vadeResult.ErrorMessage, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tumKayitlar = new List<Dictionary<string, object>>();
                foreach (var row in vadeResult.Data)
                {
                    decimal cariBakiye = ToDecimal(GetVal(row, "CariBakiye", 0));
                    decimal cariUsd = ToDecimal(GetVal(row, "CariUSD", 0));
                    decimal cariEuro = ToDecimal(GetVal(row, "CariEURO", 0));
                    decimal cariGbp = ToDecimal(GetVal(row, "CariGBP", 0));
                    decimal vgTL = ToDecimal(GetVal(row, "VadesiGecmisTL", 0));
                    decimal vgUSD = ToDecimal(GetVal(row, "VadesiGecmisUSD", 0));
                    decimal vgEURO = ToDecimal(GetVal(row, "VadesiGecmisEURO", 0));
                    decimal vgGBP = ToDecimal(GetVal(row, "VadesiGecmisGBP", 0));

                    // Bakiye eksi ise vadesi geçmiş de eksi olacak (aynı işaret)
                    if (cariBakiye < 0) vgTL = -Math.Abs(vgTL);
                    else vgTL = Math.Abs(vgTL);

                    if (cariUsd < 0) vgUSD = -Math.Abs(vgUSD);
                    else vgUSD = Math.Abs(vgUSD);

                    if (cariEuro < 0) vgEURO = -Math.Abs(vgEURO);
                    else vgEURO = Math.Abs(vgEURO);

                    if (cariGbp < 0) vgGBP = -Math.Abs(vgGBP);
                    else vgGBP = Math.Abs(vgGBP);

                    Dictionary<string, object> satir = new Dictionary<string, object>
                    {
                        ["CARIKOD"] = GetVal(row, "CariKod", ""),
                        ["CARIACIKLAMA"] = GetVal(row, "CariAciklama", ""),
                        ["CARIBAKIYE"] = cariBakiye,
                        ["CARIUSD"] = cariUsd,
                        ["CARIEURO"] = cariEuro,
                        ["CARIGBP"] = cariGbp,
                        ["VADESIGECMISBAKIYE"] = vgTL,
                        ["VADESIGECMISUSD"] = vgUSD,
                        ["VADESIGECMISEURO"] = vgEURO,
                        ["VADESIGECMISGBP"] = vgGBP,
                    };
                    tumKayitlar.Add(satir);
                }

                DataTable dataTable = ConvertToDataTable(tumKayitlar);
                gridControl1.DataSource = dataTable;
                ConfigureColumns();
                SetFooterTotals();
                gridView1.BestFitColumns();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync("CustomerMaturityForm LoadDataAsync hatasi: " + ex.Message);
                XtraMessageBox.Show("Beklenmeyen hata:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
            try
            {
                // API double/float döndürüyor, direkt Convert kullan
                return Convert.ToDecimal(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                if (decimal.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
                    return d;
                return 0m;
            }
        }
        private DataTable ConvertToDataTable(List<Dictionary<string, object>> data)
        {
            DataTable dt = new DataTable();
            if (data == null || data.Count == 0) return dt;
            List<string> kolonSirasi = new List<string>
            {
                "CARIKOD", "CARIACIKLAMA",
                "CARIBAKIYE", "CARIUSD", "CARIEURO", "CARIGBP",
                "VADESIGECMISBAKIYE", "VADESIGECMISUSD", "VADESIGECMISEURO", "VADESIGECMISGBP"
            };
            foreach (string key in kolonSirasi)
                dt.Columns.Add(key, typeof(string));
            HashSet<string> textKolonlar = new HashSet<string> { "CARIKOD", "CARIACIKLAMA" };
            foreach (var row in data)
            {
                DataRow dr = dt.NewRow();
                foreach (var key in kolonSirasi)
                {
                    object val = GetVal(row, key, null);
                    if (textKolonlar.Contains(key))
                        dr[key] = val != null ? val.ToString() : "";
                    else
                        dr[key] = ToDecimal(val).ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        private void ConfigureColumns()
        {
            List<(string FieldName, string Caption, int Width)> kolonSirasi = new List<(string FieldName, string Caption, int Width)>
            {
                ("CARIKOD",            "Cari Kodu",           140),
                ("CARIACIKLAMA",       "Cari Adi",            230),
                ("CARIBAKIYE",         "Cari Bakiye (TL)",    150),
                ("CARIUSD",            "Cari Bakiye (USD)",   150),
                ("CARIEURO",           "Cari Bakiye (EUR)",   150),
                ("CARIGBP",            "Cari Bakiye (GBP)",   150),
                ("VADESIGECMISBAKIYE", "Vadesi Gecmis (TL)",  150),
                ("VADESIGECMISUSD",    "Vadesi Gecmis (USD)", 150),
                ("VADESIGECMISEURO",   "Vadesi Gecmis (EUR)", 150),
                ("VADESIGECMISGBP",    "Vadesi Gecmis (GBP)", 150),
            };
            gridView1.BeginUpdate();
            try
            {
                foreach (GridColumn col in gridView1.Columns)
                    col.VisibleIndex = -1;
                for (int i = 0; i < kolonSirasi.Count; i++)
                {
                    var k = kolonSirasi[i];
                    GridColumn col = gridView1.Columns[k.FieldName];
                    if (col == null) continue;
                    col.Caption = k.Caption;
                    col.Width = k.Width;
                    col.VisibleIndex = i;
                    bool isText = k.FieldName == "CARIKOD" || k.FieldName == "CARIACIKLAMA";
                    if (!isText)
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        col.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                    }
                }
            }
            finally
            {
                gridView1.EndUpdate();
            }
            gridView1.ClearSorting();
            if (gridView1.Columns["VADESIGECMISBAKIYE"] != null)
                gridView1.Columns["VADESIGECMISBAKIYE"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
        }
        private void SetFooterTotals()
        {
            string[] sumColumns = {
                "CARIBAKIYE", "CARIUSD", "CARIEURO", "CARIGBP",
                "VADESIGECMISBAKIYE", "VADESIGECMISUSD", "VADESIGECMISEURO", "VADESIGECMISGBP"
            };
            foreach (GridColumn col in gridView1.Columns)
            {
                if (Array.IndexOf(sumColumns, col.FieldName) >= 0)
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2}";
                }
            }
            if (gridView1.Columns["CARIKOD"] != null)
            {
                gridView1.Columns["CARIKOD"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
                gridView1.Columns["CARIKOD"].SummaryItem.DisplayFormat = "Toplam: {0} Cari";
            }
        }
        private void GridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int seciliSayi = gridView1.GetSelectedRows().Length;
            lblSeciliSayi.Text = "Secili: " + seciliSayi + " kayit";
        }
        private void btnTumunuSec_Click(object sender, EventArgs e) => gridView1.SelectAll();
        private void btnSecimiTemizle_Click(object sender, EventArgs e) => gridView1.ClearSelection();
        private async void btnYenile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await LoadDataAsync();
        }
        private async void btnExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (tumKayitlar.Count == 0)
                {
                    XtraMessageBox.Show("Excel'e aktarilacak veri yok!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                    FileName = "CariVadeAnalizi_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    gridView1.ExportToXlsx(saveDialog.FileName);
                    DialogResult openResult = XtraMessageBox.Show(
                        "Excel dosyasi basariyla olusturuldu!\n\nDosyayi acmak ister misiniz?",
                        "Basarili", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (openResult == DialogResult.Yes)
                        Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync("CustomerMaturityForm Excel hatasi: " + ex.Message);
                XtraMessageBox.Show("Excel aktarma hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e) => base.OnFormClosing(e);
        private void CustomerMaturityForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }
        private async void btn_Smartsheet_Click(object sender, EventArgs e)
        {
            try
            {
                int[] selectedRows = gridView1.GetSelectedRows();
                if (selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen aktarılacak kayıtları seçin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                List<Dictionary<string, object>> seciliKayitlar = new List<Dictionary<string, object>>();
                foreach (int rowHandle in selectedRows)
                {
                    if (rowHandle < 0) continue;
                    DataRowView drv = gridView1.GetRow(rowHandle) as DataRowView;
                    if (drv == null) continue;
                    // tumKayitlar listesinden al — DataTable değil orijinal dict
                    // Grid DataTable'a bağlı, satır sırasını eşleştir
                    string cariKodu = drv.Row["CARIKOD"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(cariKodu)) continue;
                    // tumKayitlar'dan bu cari koduna ait kaydı bul
                    var orijinal = tumKayitlar.FirstOrDefault(k =>
                    {
                        string match = k.Keys.FirstOrDefault(x => x.Equals("CARIKOD", StringComparison.OrdinalIgnoreCase));
                        return match != null && k[match]?.ToString()?.Trim() == cariKodu;
                    });
                    if (orijinal != null)
                        seciliKayitlar.Add(orijinal);
                }
                if (seciliKayitlar.Count == 0)
                {
                    XtraMessageBox.Show("Seçili geçerli kayıt bulunamadı!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult onay = XtraMessageBox.Show(
                    $"{seciliKayitlar.Count} kayıt Smartsheet'e aktarılacak.\n\n" +
                    "• Mevcut olanlar güncellenecek (bakiyeler + tarih)\n" +
                    "• Olmayanlar eklenecek\n\n" +
                    "Devam etmek istiyor musunuz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (onay != DialogResult.Yes) return;
                this.Cursor = Cursors.WaitCursor;
                var result = await SmartsheetService.UpsertCariVadeBakiyeAsync(seciliKayitlar);
                if (result.Success)
                {
                    XtraMessageBox.Show(
                        $"✅ Aktarım tamamlandı!\n\n➕ Yeni eklenen : {result.InsertCount} kayıt\n🔄 Güncellenen  : {result.UpdateCount} kayıt",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await TextLog.LogToSQLiteAsync($"❌ CustomerMaturityForm Smartsheet aktarım hatası: {result.ErrorMessage}");
                    XtraMessageBox.Show($"Aktarım hatası:\n{result.ErrorMessage}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ btn_Smartsheet_Click hatası: {ex.Message}");
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void btn_CustomerNotCode_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CustomerNotCodeForm frm = new CustomerNotCodeForm();
            frm.ShowDialog();
        }
    }
}