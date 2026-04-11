using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using SmartSheetProject.Classes;
using SmartSheetProject.Models;

namespace SmartSheetProject.Forms
{
    public partial class PurInvoiceForm : XtraForm
    {
        private const int PAGE_SIZE = 1000;
        private List<PurInvoiceMasterModel> tumFaturalar = new List<PurInvoiceMasterModel>();
        public PurInvoiceForm()
        {
            InitializeComponent();
        }
        private enum GroupMode { None, ByCari, ByProduct }
        private GroupMode currentGroupMode = GroupMode.None;
        private async void PurInvoiceForm_Load(object sender, EventArgs e)
        {
            try
            {
                var bulutERPSettingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!bulutERPSettingsResult.Success)
                {
                    XtraMessageBox.Show("Bulut ERP ayarları bulunamadı!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
                ConfigureMasterDetailGrid();
                dateBaslangic.EditValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateBitis.EditValue = DateTime.Now.Date;
                btnFiltrele.Click += async (s, ev) => await LoadDataAsync();
                btnYenile.ItemClick += async (s, ev) => await LoadDataAsync();
                btnExcel.ItemClick += BtnExcel_ItemClick;
                btnGrupla.ItemClick += BtnGrupla_ItemClick;
                btnGrubuCoz.ItemClick += BtnGrubuCoz_ItemClick;
                btn_GrpProduct.ItemClick += btn_GrpProduct_ItemClick;
               // await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ PurInvoiceForm_Load hatası: {ex.Message}");
                XtraMessageBox.Show($"Form yüklenirken hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        #region Grid Yapılandırma

        private void ConfigureMasterDetailGrid()
        {
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowAutoFilterRow = true;
            gridView1.OptionsView.ShowFooter = true;
            gridView1.OptionsView.ShowGroupPanel = true;
            gridView1.OptionsDetail.EnableMasterViewMode = true;
            gridView1.OptionsDetail.ShowDetailTabs = false;
            gridView1.OptionsDetail.SmartDetailExpand = false;
            gridViewDetay.OptionsView.ColumnAutoWidth = false;
            gridViewDetay.OptionsBehavior.Editable = false;
            gridViewDetay.OptionsView.ShowFooter = true;
            gridViewDetay.OptionsView.ShowGroupPanel = false;
            gridViewDetay.Appearance.HeaderPanel.BackColor = Color.FromArgb(0, 114, 198);
            gridViewDetay.Appearance.HeaderPanel.ForeColor = Color.White;
            gridViewDetay.Appearance.HeaderPanel.Options.UseBackColor = true;
            gridViewDetay.Appearance.HeaderPanel.Options.UseForeColor = true;
            var levelNode = new DevExpress.XtraGrid.GridLevelNode();
            levelNode.LevelTemplate = gridViewDetay;
            levelNode.RelationName = "Detaylar";
            gridControl1.LevelTree.Nodes.Add(levelNode);
        }
        private void ConfigureMasterColumns()
        {
            var kolonlar = new (string Field, string Caption, int Idx, int Width)[]
            {
                ("FATURA_TIPI",            "Fatura Tipi",        0, 140),
                ("FATURA_NO",              "Fatura No",          1, 150),
                ("TARIHI",                 "Tarih",              2,  95),
                ("FATURA_VADE_TARIHI",     "Vade Tarihi",        3,  95),
                ("CARI_KODU",              "Cari Kodu",          4, 120),
                ("CARI_ACIKLAMASI",        "Cari Adı",           5, 220),
                ("PROJE_KODU",             "Proje",              6, 100),
                ("PARA_BIRIMI",            "Para Birimi",        7,  80),
                ("KUR",                    "Kur",                8,  75),
              ("FATURA_KDVSIZ_TUTAR",    "KDV'siz Tutar",      9, 120),
("KDV_TUTARI",             "KDV",               10, 100),
("KDV_TUTARI_DOVIZ",       "KDV (Döviz)",       11, 120),  // ← 11'e çek
("FATURA_TOPLAM_TUTAR_TL", "Toplam (TL)",       12, 120),
("FATURA_TOPLAM_TUTAR_ID", "Toplam (Döviz)",    13, 120),
("FATURA_ACIKLAMASI",      "Fatura Açıklaması", 14, 250),
("FATURALOGICALREF",       "Fatura ID",         15,  80),
            };
            foreach (var k in kolonlar)
            {
                GridColumn col = gridView1.Columns[k.Field];
                if (col == null) continue;
                col.Caption = k.Caption;
                col.VisibleIndex = k.Idx;
                col.Width = k.Width;

                if (k.Field.Contains("TUTAR") || k.Field == "KDV_TUTARI" || k.Field == "KUR")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "N2";
                }
                if (k.Field.Contains("TARIH"))
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    col.DisplayFormat.FormatString = "dd.MM.yyyy";
                }
                if (k.Field == "KDV_TUTARI_DOVIZ")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "N2";
                }
            }
            foreach (GridColumn col in gridView1.Columns)
            {
                if (col.FieldName == "FATURA_KDVSIZ_TUTAR" ||
                    col.FieldName == "KDV_TUTARI" ||
                    col.FieldName == "FATURA_TOPLAM_TUTAR_TL")
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2} TL";
                }
            }
            // ConfigureMasterColumns içinde, diğer summary'lerin yanına:
            if (gridView1.Columns["KDV_TUTARI_DOVIZ"] != null)
            {
                gridView1.Columns["KDV_TUTARI_DOVIZ"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["KDV_TUTARI_DOVIZ"].SummaryItem.DisplayFormat = "{0:N2}";
            }
            if (gridView1.Columns["FATURA_NO"] != null)
            {
                gridView1.Columns["FATURA_NO"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
                gridView1.Columns["FATURA_NO"].SummaryItem.DisplayFormat = "Toplam: {0} Fatura";
            }
        }
        private void ConfigureDetailColumns(GridView detailView)
        {
            var kolonlar = new (string Field, string Caption, int Idx, int Width)[]
            {
        ("MALZEME_KODU",       "Malzeme Kodu",     0, 150),
        ("MALZEME_ADI",        "Malzeme Adı",      1, 250),
        ("SATIR_ACIKLAMASI",   "Satır Açıklaması", 2, 200),
        ("MIKTAR",             "Miktar",           3,  90),
        ("BIRIM",              "Birim",            4,  70),
        ("BIRIM_FIYAT",        "Birim Fiyat (TL)", 5, 120),
        ("SATIR_TOPLAM_TL",    "Toplam (TL)",      6, 120),
        ("SATIR_KDV_ORANI",    "KDV %",            7,  65),
        ("SATIR_KDV_TL",       "KDV (TL)",         8, 110),
        ("SATIR_KDV_DOVIZ",    "KDV (Döviz)",      9, 110),
        ("SATIR_DOVIZ",        "Döviz",           10,  70),
        ("SATIR_KUR",          "Kur",             11,  80),
        ("SATIR_TOPLAM_DOVIZ", "Toplam (Döviz)",  12, 120),
            };
            foreach (var k in kolonlar)
            {
                GridColumn col = detailView.Columns[k.Field];  // ← detailView
                if (col == null) continue;
                col.Caption = k.Caption;
                col.VisibleIndex = k.Idx;  // ← Idx kullan
                col.Width = k.Width;

                if (k.Field == "MIKTAR" || k.Field == "BIRIM_FIYAT" ||
                    k.Field == "SATIR_TOPLAM_TL" || k.Field == "SATIR_TOPLAM_DOVIZ" ||
                    k.Field == "SATIR_KDV_TL" || k.Field == "SATIR_KDV_ORANI" ||
                    k.Field == "SATIR_KUR" || k.Field == "SATIR_KDV_DOVIZ")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "N2";
                }
                if (k.Field == "SATIR_TOPLAM_TL")
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2} TL";
                }
                if (k.Field == "SATIR_KDV_TL")
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2} TL";
                }
                if (k.Field == "SATIR_KDV_DOVIZ")
                {
                    col.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                    col.SummaryItem.DisplayFormat = "{0:N2}";
                }
            }
        }
        #endregion

        #region Gruplama
        private void BtnGrupla_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.Columns["CARI_ACIKLAMASI"] == null) return;
            gridView1.ClearGrouping();
            gridView1.Columns["CARI_ACIKLAMASI"].GroupIndex = 0;
            gridView1.ExpandAllGroups();
            currentGroupMode = GroupMode.ByCari;
            btnGrupla.Enabled = false;
            btn_GrpProduct.Enabled = false;
            btnGrubuCoz.Enabled = true;
        }
        private void BtnGrubuCoz_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gridView1.ClearGrouping();
            // Her iki moddan da temiz çıkış
            gridView1.OptionsDetail.EnableMasterViewMode = true;
            gridView1.Columns.Clear();
            DataSet ds = BuildDataSet(tumFaturalar);
            gridControl1.DataSource = ds.Tables["Faturalar"];
            ConfigureMasterColumns();
            ConfigureDetailColumns(gridViewDetay);
            currentGroupMode = GroupMode.None;
            btnGrupla.Enabled = true;
            btn_GrpProduct.Enabled = true;
            btnGrubuCoz.Enabled = false;
        }
        #endregion

        #region Veri Yükleme

        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                gridControl1.DataSource = null;
                lblKayitSayisi.Text = "Yükleniyor...";
                var tokenResult = await BulutERPService.EnsureValidTokenAsync();
                if (!tokenResult.Success)
                {
                    XtraMessageBox.Show($"Token alınamadı:\n{tokenResult.ErrorMessage}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DateTime baslangic = dateBaslangic.EditValue != null
                    ? Convert.ToDateTime(dateBaslangic.EditValue).Date : DateTime.Now.Date;
                DateTime bitis = dateBitis.EditValue != null
                    ? Convert.ToDateTime(dateBitis.EditValue).Date : DateTime.Now.Date;

                List<PurInvoiceFaturaModel> hamVeriler =
                    await FetchAllPagesAsync(tokenResult.AccessToken, baslangic, bitis);
                if (hamVeriler == null) return;
                tumFaturalar = GroupToMasterDetail(hamVeriler);
                DataSet ds = BuildDataSet(tumFaturalar);
                gridControl1.DataSource = ds.Tables["Faturalar"];
                ConfigureMasterColumns();
                ConfigureDetailColumns(gridViewDetay);
                if (currentGroupMode == GroupMode.ByCari && gridView1.Columns["CARI_ACIKLAMASI"] != null)
                {
                    gridView1.ClearGrouping();
                    gridView1.Columns["CARI_ACIKLAMASI"].GroupIndex = 0;
                    gridView1.ExpandAllGroups();
                }
                lblKayitSayisi.Text = $"{tumFaturalar.Count} fatura  |  {hamVeriler.Count} malzeme satırı";
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ PurInvoiceForm LoadDataAsync: {ex.Message}");
                XtraMessageBox.Show($"Veri yüklenirken hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private async Task<List<PurInvoiceFaturaModel>> FetchAllPagesAsync(
            string accessToken, DateTime baslangic, DateTime bitis)
        {
            List<PurInvoiceFaturaModel> tumSatirlar = new List<PurInvoiceFaturaModel>();
            int offset = 0, sayfaNo = 1;
            while (true)
            {
                lblKayitSayisi.Text = $"Sayfa {sayfaNo} yükleniyor... ({tumSatirlar.Count} satır)";
                Application.DoEvents();
                string sql = BuildSql(baslangic, bitis, offset, PAGE_SIZE);
                var result = await BulutERPService.ExecuteSelectQueryAsync(sql, accessToken, PAGE_SIZE);
                if (!result.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ Sayfa hatası (offset={offset}): {result.ErrorMessage}");
                    XtraMessageBox.Show($"Veri çekilirken hata (sayfa {sayfaNo}):\n{result.ErrorMessage}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
               if (result.Data == null || result.Data.Count == 0) break;
                tumSatirlar.AddRange(ConvertToModel(result.Data));
                if (result.Data.Count < PAGE_SIZE) break;
                offset += PAGE_SIZE;
                sayfaNo++;
            }
            return tumSatirlar;
        }
        private string BuildSql(DateTime baslangic, DateTime bitis, int offset, int limit)
        {
            return
$@"SELECT INV.LOGICALREF AS FATURALOGICALREF,
ARP.CODE AS CARI_KODU,
ARP.DESCRIPTION AS CARI_ACIKLAMASI,
INV.AUXCODE AS PROJE_KODU,
INV.SLIPNR AS FATURA_NO,
INV.SLIPDATE::DATE AS TARIHI,
INV.GENEXP AS FATURA_ACIKLAMASI,
INV.SLIPDATE::DATE + COALESCE(PAY.PAYDAY::INTEGER, 0) AS FATURA_VADE_TARIHI,
MMT.TCRATE AS KUR,
CASE WHEN INV.TCTYPE = 0  THEN 'TL'
     WHEN INV.TCTYPE = 1  THEN 'USD'
     WHEN INV.TCTYPE = 20 THEN 'EURO'
     ELSE 'KONTROL_EDILECEK' END AS PARA_BIRIMI,
CASE WHEN INV.SLIPTYPE = 1  THEN 'SATIN ALMA'
     WHEN INV.SLIPTYPE = 3  THEN 'TOPTAN SATIS IADE'
     WHEN INV.SLIPTYPE = 4  THEN 'ALINAN HIZMET'
   WHEN INV.SLIPTYPE = 13  THEN 'FİYAT FARKI FATURASI'
     WHEN INV.SLIPTYPE = 12  THEN 'SATIN ALMA VADE FARKI'
     WHEN INV.SLIPTYPE = 26  THEN 'MÜSTAHSİL MAK'
     WHEN INV.SLIPTYPE = 6  THEN 'SATIN ALMA IADE'
     WHEN INV.SLIPTYPE = 21 THEN 'VARLIK FATURASI'
     ELSE 'DIGER' END AS FATURA_TIPI,
CASE WHEN INV.SLIPTYPE = 6 THEN -INV.NETTOTAL   ELSE INV.NETTOTAL   END AS FATURA_TOPLAM_TUTAR_TL,
CASE WHEN INV.SLIPTYPE = 6 THEN -INV.TCNET      ELSE INV.TCNET      END AS FATURA_TOPLAM_TUTAR_ID,
CASE WHEN INV.SLIPTYPE = 6 THEN -INV.GROSSTOTAL ELSE INV.GROSSTOTAL END AS FATURA_KDVSIZ_TUTAR,
CASE WHEN INV.SLIPTYPE = 6 THEN -INV.TOTALVAT   ELSE INV.TOTALVAT   END AS KDV_TUTARI,
ITM.CODE        AS MALZEME_KODU,
ITM.DESCRIPTION AS MALZEME_ADI,
CASE WHEN INV.SLIPTYPE = 6 THEN -MMT.QUANTITY ELSE MMT.QUANTITY END AS MIKTAR,
COALESCE(UNT.CODE, '') AS BIRIM,
CASE WHEN INV.SLIPTYPE = 6 THEN -MMT.PRICE   ELSE MMT.PRICE   END AS BIRIM_FIYAT,
CASE WHEN INV.SLIPTYPE = 6 THEN -MMT.TOTAL   ELSE MMT.TOTAL   END AS SATIR_TOPLAM_TL,
CASE WHEN INV.SLIPTYPE = 6 THEN -MMT.TCTOTAL ELSE MMT.TCTOTAL END AS SATIR_TOPLAM_DOVIZ,
CASE WHEN MMT.TCTYPE = 0  THEN 'TL'
     WHEN MMT.TCTYPE = 1  THEN 'USD'
     WHEN MMT.TCTYPE = 20 THEN 'EURO'
     ELSE 'DIGER' END AS SATIR_DOVIZ,
CASE WHEN MMT.TCRATE > 0 
     THEN ROUND(MMT.VATAMNT / MMT.TCRATE, 2)
     ELSE 0 
END AS SATIR_KDV_DOVIZ,
MMT.TCRATE  AS SATIR_KUR,
MMT.VATRATE AS SATIR_KDV_ORANI,
CASE WHEN INV.SLIPTYPE = 6 THEN -MMT.VATAMNT ELSE MMT.VATAMNT END AS SATIR_KDV_TL,
COALESCE(MMT.LINEEXP, '') AS SATIR_ACIKLAMASI
FROM U_$V(firm)_01_INVOICES INV
LEFT JOIN U_$V(firm)_01_MMTRANS MMT ON MMT.INVOICEREF = INV.LOGICALREF
LEFT JOIN U_$V(firm)_ITEMS      ITM ON ITM.LOGICALREF = MMT.ITEMREF
LEFT JOIN U_$V(firm)_UNITS      UNT ON UNT.LOGICALREF = MMT.UOMREF
LEFT JOIN U_$V(firm)_ARPS       ARP ON ARP.LOGICALREF = INV.ARPREF
LEFT JOIN U_$V(firm)_PAYPLANLNS PAY ON PAY.LOGICALREF = INV.PAYPLANREF
WHERE INV.SLIPTYPE IN (1,  4, 6,12,13,26, 21) AND INV.BOSTATUS  IN (1,2)
AND INV.SLIPDATE::DATE >= '{baslangic:yyyy-MM-dd}'
AND INV.SLIPDATE::DATE <= '{bitis:yyyy-MM-dd}'
ORDER BY INV.SLIPDATE DESC, INV.LOGICALREF, MMT.LOGICALREF
LIMIT {limit} OFFSET {offset}";
        }
        #endregion

        #region Yardımcı

        private static T SafeVal<T>(Dictionary<string, object> dict, string key, T def)
        {
            if (!dict.ContainsKey(key) || dict[key] == null) return def;
            try { return (T)Convert.ChangeType(dict[key], typeof(T)); }
            catch { return def; }
        }
        #endregion

        #region Model Dönüşüm
        private List<PurInvoiceFaturaModel> ConvertToModel(List<Dictionary<string, object>> data)
        {
            List<PurInvoiceFaturaModel> list = new List<PurInvoiceFaturaModel>();
            foreach (var dict in data)
            {
                try
                {
                    list.Add(new PurInvoiceFaturaModel
                    {
                        FATURALOGICALREF = SafeVal<int>(dict, "FATURALOGICALREF", 0),
                        FATURA_NO = SafeVal<string>(dict, "FATURA_NO", ""),
                        TARIHI = dict.ContainsKey("TARIHI") && dict["TARIHI"] != null
                                                    ? Convert.ToDateTime(dict["TARIHI"]) : (DateTime?)null,
                        FATURA_VADE_TARIHI = dict.ContainsKey("FATURA_VADE_TARIHI") && dict["FATURA_VADE_TARIHI"] != null
                                                    ? Convert.ToDateTime(dict["FATURA_VADE_TARIHI"]) : (DateTime?)null,
                        CARI_KODU = SafeVal<string>(dict, "CARI_KODU", ""),
                        CARI_ACIKLAMASI = SafeVal<string>(dict, "CARI_ACIKLAMASI", ""),
                        PROJE_KODU = SafeVal<string>(dict, "PROJE_KODU", ""),
                        PARA_BIRIMI = SafeVal<string>(dict, "PARA_BIRIMI", ""),
                        KUR = SafeVal<decimal>(dict, "KUR", 0m),
                        FATURA_TIPI = SafeVal<string>(dict, "FATURA_TIPI", ""),
                        FATURA_KDVSIZ_TUTAR = SafeVal<decimal>(dict, "FATURA_KDVSIZ_TUTAR", 0m),
                        KDV_TUTARI = SafeVal<decimal>(dict, "KDV_TUTARI", 0m),
                        FATURA_TOPLAM_TUTAR_TL = SafeVal<decimal>(dict, "FATURA_TOPLAM_TUTAR_TL", 0m),
                        FATURA_TOPLAM_TUTAR_ID = SafeVal<decimal>(dict, "FATURA_TOPLAM_TUTAR_ID", 0m),
                        FATURA_ACIKLAMASI = SafeVal<string>(dict, "FATURA_ACIKLAMASI", ""),
                        MALZEME_KODU = SafeVal<string>(dict, "MALZEME_KODU", ""),
                        MALZEME_ADI = SafeVal<string>(dict, "MALZEME_ADI", ""),
                        SATIR_KDV_DOVIZ = SafeVal<decimal>(dict, "SATIR_KDV_DOVIZ", 0m),
                        MIKTAR = SafeVal<decimal>(dict, "MIKTAR", 0m),
                        BIRIM = SafeVal<string>(dict, "BIRIM", ""),
                        BIRIM_FIYAT = SafeVal<decimal>(dict, "BIRIM_FIYAT", 0m),
                        SATIR_TOPLAM_TL = SafeVal<decimal>(dict, "SATIR_TOPLAM_TL", 0m),
                        SATIR_TOPLAM_DOVIZ = SafeVal<decimal>(dict, "SATIR_TOPLAM_DOVIZ", 0m),
                        SATIR_DOVIZ = SafeVal<string>(dict, "SATIR_DOVIZ", ""),
                        SATIR_KUR = SafeVal<decimal>(dict, "SATIR_KUR", 0m),
                        SATIR_KDV_ORANI = SafeVal<decimal>(dict, "SATIR_KDV_ORANI", 0m),
                        SATIR_KDV_TL = SafeVal<decimal>(dict, "SATIR_KDV_TL", 0m),
                        SATIR_ACIKLAMASI = SafeVal<string>(dict, "SATIR_ACIKLAMASI", ""),
                    });
                }
                catch (Exception ex)
                {
                    TextLog.LogToSQLiteAsync($"❌ ConvertToModel satır hatası: {ex.Message}").Wait();
                }
            }
            return list;
        }
        private List<PurInvoiceMasterModel> GroupToMasterDetail(List<PurInvoiceFaturaModel> satirlar)
        {
            return satirlar
                .GroupBy(s => s.FATURALOGICALREF)
                .Select(g =>
                {
                    PurInvoiceFaturaModel ilk = g.First();
                    return new PurInvoiceMasterModel
                    {
                        FATURALOGICALREF = ilk.FATURALOGICALREF,
                        FATURA_NO = ilk.FATURA_NO,
                        TARIHI = ilk.TARIHI,
                        FATURA_VADE_TARIHI = ilk.FATURA_VADE_TARIHI,
                        CARI_KODU = ilk.CARI_KODU,
                        CARI_ACIKLAMASI = ilk.CARI_ACIKLAMASI,
                        PROJE_KODU = ilk.PROJE_KODU,
                        KDV_TUTARI_DOVIZ = g.Sum(s => s.SATIR_KDV_DOVIZ),
                        PARA_BIRIMI = ilk.PARA_BIRIMI,
                        KUR = ilk.KUR,
                        FATURA_TIPI = ilk.FATURA_TIPI,
                        FATURA_KDVSIZ_TUTAR = ilk.FATURA_KDVSIZ_TUTAR,
                        KDV_TUTARI = ilk.KDV_TUTARI,
                        FATURA_TOPLAM_TUTAR_TL = ilk.FATURA_TOPLAM_TUTAR_TL,
                        FATURA_TOPLAM_TUTAR_ID = ilk.FATURA_TOPLAM_TUTAR_ID,
                        FATURA_ACIKLAMASI = ilk.FATURA_ACIKLAMASI,
                        Detaylar = g.Select(s => new PurInvoiceDetayModel
                        {
                            FATURALOGICALREF = s.FATURALOGICALREF,
                            MALZEME_KODU = s.MALZEME_KODU,
                            MALZEME_ADI = s.MALZEME_ADI,
                            MIKTAR = s.MIKTAR,
                            BIRIM = s.BIRIM,
                            BIRIM_FIYAT = s.BIRIM_FIYAT,
                            SATIR_KDV_DOVIZ = s.SATIR_KDV_DOVIZ,
                            SATIR_TOPLAM_TL = s.SATIR_TOPLAM_TL,
                            SATIR_TOPLAM_DOVIZ = s.SATIR_TOPLAM_DOVIZ,
                            SATIR_DOVIZ = s.SATIR_DOVIZ,
                            SATIR_KUR = s.SATIR_KUR,
                            SATIR_KDV_ORANI = s.SATIR_KDV_ORANI,
                            SATIR_KDV_TL = s.SATIR_KDV_TL,
                            SATIR_ACIKLAMASI = s.SATIR_ACIKLAMASI,
                        }).ToList()
                    };
                })
                .ToList();
        }
        private DataSet BuildDataSet(List<PurInvoiceMasterModel> faturalar)
        {
            DataSet ds = new DataSet();
            DataTable masterTable = new DataTable("Faturalar");
            masterTable.Columns.Add("FATURALOGICALREF", typeof(int));
            masterTable.Columns.Add("FATURA_NO", typeof(string));
            masterTable.Columns.Add("TARIHI", typeof(DateTime));
            masterTable.Columns.Add("FATURA_VADE_TARIHI", typeof(DateTime));
            masterTable.Columns.Add("CARI_KODU", typeof(string));
            masterTable.Columns.Add("CARI_ACIKLAMASI", typeof(string));
            masterTable.Columns.Add("PROJE_KODU", typeof(string));
            masterTable.Columns.Add("PARA_BIRIMI", typeof(string));
            masterTable.Columns.Add("KUR", typeof(decimal));
            masterTable.Columns.Add("KDV_TUTARI_DOVIZ", typeof(decimal));
            masterTable.Columns.Add("FATURA_TIPI", typeof(string));
            masterTable.Columns.Add("FATURA_KDVSIZ_TUTAR", typeof(decimal));
            masterTable.Columns.Add("KDV_TUTARI", typeof(decimal));
            masterTable.Columns.Add("FATURA_TOPLAM_TUTAR_TL", typeof(decimal));
            masterTable.Columns.Add("FATURA_TOPLAM_TUTAR_ID", typeof(decimal));
            masterTable.Columns.Add("FATURA_ACIKLAMASI", typeof(string));
            ds.Tables.Add(masterTable);
            DataTable detailTable = new DataTable("Detaylar");
            detailTable.Columns.Add("FATURALOGICALREF", typeof(int));
            detailTable.Columns.Add("MALZEME_KODU", typeof(string));
            detailTable.Columns.Add("MALZEME_ADI", typeof(string));
            detailTable.Columns.Add("SATIR_ACIKLAMASI", typeof(string));
            detailTable.Columns.Add("MIKTAR", typeof(decimal));
            detailTable.Columns.Add("BIRIM", typeof(string));
            detailTable.Columns.Add("BIRIM_FIYAT", typeof(decimal));
            detailTable.Columns.Add("SATIR_TOPLAM_TL", typeof(decimal));
            detailTable.Columns.Add("SATIR_KDV_ORANI", typeof(decimal));
            detailTable.Columns.Add("SATIR_KDV_TL", typeof(decimal));
            detailTable.Columns.Add("SATIR_KDV_DOVIZ", typeof(decimal));  // ← BURAYA
            detailTable.Columns.Add("SATIR_DOVIZ", typeof(string));
            detailTable.Columns.Add("SATIR_KUR", typeof(decimal));
            detailTable.Columns.Add("SATIR_TOPLAM_DOVIZ", typeof(decimal));

            ds.Tables.Add(detailTable);
            ds.Relations.Add("Detaylar",
                masterTable.Columns["FATURALOGICALREF"],
                detailTable.Columns["FATURALOGICALREF"]);
            foreach (PurInvoiceMasterModel f in faturalar)
            {
                DataRow mr = masterTable.NewRow();
                mr["FATURALOGICALREF"] = f.FATURALOGICALREF;
                mr["FATURA_NO"] = f.FATURA_NO ?? "";
                mr["TARIHI"] = f.TARIHI.HasValue ? (object)f.TARIHI.Value : DBNull.Value;
                mr["FATURA_VADE_TARIHI"] = f.FATURA_VADE_TARIHI.HasValue ? (object)f.FATURA_VADE_TARIHI.Value : DBNull.Value;
                mr["CARI_KODU"] = f.CARI_KODU ?? "";
                mr["CARI_ACIKLAMASI"] = f.CARI_ACIKLAMASI ?? "";
                mr["PROJE_KODU"] = f.PROJE_KODU ?? "";
                mr["KDV_TUTARI_DOVIZ"] = f.KDV_TUTARI_DOVIZ;
                mr["PARA_BIRIMI"] = f.PARA_BIRIMI ?? "";
                mr["KUR"] = f.KUR;
                mr["FATURA_TIPI"] = f.FATURA_TIPI ?? "";
                mr["FATURA_KDVSIZ_TUTAR"] = f.FATURA_KDVSIZ_TUTAR;
                mr["KDV_TUTARI"] = f.KDV_TUTARI;
                mr["FATURA_TOPLAM_TUTAR_TL"] = f.FATURA_TOPLAM_TUTAR_TL;
                mr["FATURA_TOPLAM_TUTAR_ID"] = f.FATURA_TOPLAM_TUTAR_ID;
                mr["FATURA_ACIKLAMASI"] = f.FATURA_ACIKLAMASI ?? "";
                masterTable.Rows.Add(mr);
                foreach (PurInvoiceDetayModel d in f.Detaylar)
                {
                    DataRow dr = detailTable.NewRow();
                    dr["FATURALOGICALREF"] = f.FATURALOGICALREF;
                    dr["MALZEME_KODU"] = d.MALZEME_KODU ?? "";
                    dr["MALZEME_ADI"] = d.MALZEME_ADI ?? "";
                    dr["SATIR_ACIKLAMASI"] = d.SATIR_ACIKLAMASI ?? "";
                    dr["MIKTAR"] = d.MIKTAR;
                    dr["BIRIM"] = d.BIRIM ?? "";
                    dr["SATIR_KDV_DOVIZ"] = d.SATIR_KDV_DOVIZ;
                    dr["BIRIM_FIYAT"] = d.BIRIM_FIYAT;
                    dr["SATIR_TOPLAM_TL"] = d.SATIR_TOPLAM_TL;
                    dr["SATIR_KDV_ORANI"] = d.SATIR_KDV_ORANI;
                    dr["SATIR_KDV_TL"] = d.SATIR_KDV_TL;
                    dr["SATIR_DOVIZ"] = d.SATIR_DOVIZ ?? "";
                    dr["SATIR_KUR"] = d.SATIR_KUR;
                    dr["SATIR_TOPLAM_DOVIZ"] = d.SATIR_TOPLAM_DOVIZ;
                    detailTable.Rows.Add(dr);
                }
            }
            return ds;
        }
        #endregion

        private void ExportCariBazliOzet(string filePath)
        {
            var ozet = tumFaturalar
                .GroupBy(f => new { f.CARI_KODU, f.CARI_ACIKLAMASI })
                .Select(g => new
                {
                    CariKodu = g.Key.CARI_KODU,
                    CariAdi = g.Key.CARI_ACIKLAMASI,
                    FaturaSayisi = g.Count(),
                    KDVsizToplam = g.Sum(f => f.FATURA_KDVSIZ_TUTAR),
                    KDVToplam = g.Sum(f => f.KDV_TUTARI),
                    GenelToplam = g.Sum(f => f.FATURA_TOPLAM_TUTAR_TL),
                    DovizToplam = g.Sum(f => f.FATURA_TOPLAM_TUTAR_ID),
                })
                .OrderByDescending(x => x.GenelToplam)
                .ToList();
            string[] headers = { "Cari Kodu", "Cari Adı", "Fatura Sayısı", "KDV'siz Tutar", "KDV Tutarı", "Toplam (TL)", "Toplam (Döviz)" };
            int[] colWidths = { 18, 45, 14, 20, 18, 20, 20 };
            string baslik = "CARİ BAZLI SATIN ALMA ANALİZİ";
            string bilgi = $"Toplam {tumFaturalar.Count} fatura  |  {ozet.Count} farklı cari  (iadeler düşülmüştür)";
           var satirlar = ozet.Select(x => new object[]
            {
        x.CariKodu,
        x.CariAdi,
        x.FaturaSayisi,
        (double)x.KDVsizToplam,
        (double)x.KDVToplam,
        (double)x.GenelToplam,
        (double)x.DovizToplam,
            }).ToList();
            decimal[] toplamlar =
        {
    (decimal)ozet.Sum(x => x.FaturaSayisi),
    ozet.Sum(x => x.KDVsizToplam),
    ozet.Sum(x => x.KDVToplam),
    ozet.Sum(x => x.GenelToplam),
    ozet.Sum(x => x.DovizToplam),
};
            // Toplam kolonun başladığı indeks (0-based, sayısal başlangıcı)
            int toplamBaslangicIdx = 2; // FaturaSayisi'ndan itibaren
            using (XLWorkbook wb = new XLWorkbook())
            {
                ExportOzetToWorkbook(wb, "Cari Özeti", baslik, bilgi, headers, colWidths, satirlar, toplamlar, toplamBaslangicIdx);
                ExportDetayliExcelToWorkbook(wb);
                wb.SaveAs(filePath);
            }
        }
        private void ExportOzetToWorkbook(
    ClosedXML.Excel.XLWorkbook wb,
    string sheetName,
    string baslik,
    string bilgi,
    string[] headers,
    int[] colWidths,
    List<object[]> satirlar,
    decimal[] toplamlar,
    int toplamBaslangicIdx) // kaçıncı kolondan itibaren sayısal (0-based)
        {
            var ws = wb.Worksheets.Add(sheetName);
            int colCount = headers.Length;
            // Ana başlık
            var titleRange = ws.Range(1, 1, 1, colCount);
            titleRange.Merge();
            titleRange.Value = baslik;
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            titleRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(31, 78, 121);
            titleRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 28;
            // Bilgi satırı
            var infoRange = ws.Range(2, 1, 2, colCount);
            infoRange.Merge();
            infoRange.Value = bilgi;
            infoRange.Style.Font.Italic = true;
            infoRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromArgb(89, 89, 89);
            ws.Row(2).Height = 16;
           // Kolon başlıkları
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(31, 78, 121);
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Column(i + 1).Width = colWidths[i];
            }
            ws.Row(3).Height = 22;
            // Veri satırları
            for (int i = 0; i < satirlar.Count; i++)
            {
                int dataRow = i + 4;
                var vals = satirlar[i];
                var bgColor = i % 2 == 0
                    ? ClosedXML.Excel.XLColor.FromArgb(214, 228, 240)
                    : ClosedXML.Excel.XLColor.White;
                // Genel toplam negatifse iade ağırlıklı — kırmızı
                bool isNegative = vals.OfType<double>().Any(v => v < -0.01);
                if (isNegative)
                    bgColor = ClosedXML.Excel.XLColor.FromArgb(255, 230, 230);
                for (int c = 0; c < vals.Length; c++)
                {
                    var cell = ws.Cell(dataRow, c + 1);
                    bool isNum = c >= toplamBaslangicIdx;
                    if (isNum && vals[c] is double dv) cell.Value = dv;
                    else if (isNum && vals[c] is int iv) cell.Value = iv;
                    else cell.Value = vals[c]?.ToString() ?? "";
                    cell.Style.Fill.BackgroundColor = bgColor;
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromArgb(191, 191, 191);
                    cell.Style.Font.FontSize = 9;
                    if (isNum && !(vals[c] is int))
                    {
                        cell.Style.NumberFormat.Format = "#,##0.00";
                        cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                    }
                    else if (vals[c] is int)
                        cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }
            }
            // Genel toplam satırı
            int totalRow = satirlar.Count + 4;
            // Metin kısmını merge et
            if (toplamBaslangicIdx > 0)
            {
                var totalLabel = ws.Range(totalRow, 1, totalRow, toplamBaslangicIdx);
                totalLabel.Merge();
                totalLabel.Value = "GENEL TOPLAM";
                totalLabel.Style.Font.Bold = true;
                totalLabel.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                totalLabel.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(46, 117, 182);
                totalLabel.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }
            // Sayısal toplamlar
            for (int t = 0; t < toplamlar.Length; t++)
            {
                var cell = ws.Cell(totalRow, toplamBaslangicIdx + 1 + t);
                cell.Value = (double)toplamlar[t];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(46, 117, 182);
                cell.Style.NumberFormat.Format = "#,##0.00";
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }
            ws.SheetView.FreezeRows(3);
            ws.Range(3, 1, 3, colCount).SetAutoFilter();
        }
        private void ExportProjeBazliOzet(string filePath)
        {
            var ozet = tumFaturalar
                .GroupBy(f => string.IsNullOrWhiteSpace(f.PROJE_KODU) ? "(Proje Yok)" : f.PROJE_KODU)
                .Select(g => new
                {
                    ProjeKodu = g.Key,
                    FaturaSayisi = g.Count(),
                    KDVsizToplam = g.Sum(f => f.FATURA_KDVSIZ_TUTAR),
                    KDVToplam = g.Sum(f => f.KDV_TUTARI),
                    GenelToplam = g.Sum(f => f.FATURA_TOPLAM_TUTAR_TL),
                    DovizToplam = g.Sum(f => f.FATURA_TOPLAM_TUTAR_ID),
                })
                .OrderByDescending(x => x.GenelToplam)
                .ToList();
            string[] headers = { "Proje Kodu", "Fatura Sayısı", "KDV'siz Tutar", "KDV Tutarı", "Toplam (TL)", "Toplam (Döviz)" };
            int[] colWidths = { 25, 14, 20, 18, 20, 20 };
            string baslik = "PROJE BAZLI SATIN ALMA ANALİZİ";
            string bilgi = $"Toplam {tumFaturalar.Count} fatura  |  {ozet.Count} farklı proje  (iadeler düşülmüştür)";
            var satirlar = ozet.Select(x => new object[]
            {
        x.ProjeKodu,
        x.FaturaSayisi,
        (double)x.KDVsizToplam,
        (double)x.KDVToplam,
        (double)x.GenelToplam,
        (double)x.DovizToplam,
            }).ToList();
            decimal[] toplamlar =
        {
    (decimal)ozet.Sum(x => x.FaturaSayisi),
    ozet.Sum(x => x.KDVsizToplam),
    ozet.Sum(x => x.KDVToplam),
    ozet.Sum(x => x.GenelToplam),
    ozet.Sum(x => x.DovizToplam),
};
            int toplamBaslangicIdx = 1; // FaturaSayisi'ndan itibaren
            using (XLWorkbook wb = new XLWorkbook())
            {
                ExportOzetToWorkbook(wb, "Proje Özeti", baslik, bilgi, headers, colWidths, satirlar, toplamlar, toplamBaslangicIdx);
                ExportDetayliExcelToWorkbook(wb);
                wb.SaveAs(filePath);
            }
        }

        #region Excel Export
        private async void BtnExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (tumFaturalar.Count == 0)
                {
                    XtraMessageBox.Show("Excel'e aktarılacak veri yok!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int secim = ShowExcelChoiceDialog();
                if (secim == 0) return; // İptal
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                    FileName = $"Satin_Alma_Faturalari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                if (saveDialog.ShowDialog() != DialogResult.OK) return;
                switch (secim)
                {
                    case 1: gridView1.ExportToXlsx(saveDialog.FileName); break;
                    case 2: ExportDetayliExcel(saveDialog.FileName); break;
                    case 3: ExportMalzemeBazliOzet(saveDialog.FileName); break;
                    case 4: ExportCariBazliOzet(saveDialog.FileName); break;
                    case 5: ExportProjeBazliOzet(saveDialog.FileName); break;
                }
                if (XtraMessageBox.Show("Excel dosyası oluşturuldu!\nAçmak ister misiniz?",
                    "Başarılı", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    Process.Start(saveDialog.FileName);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ PurInvoiceForm Excel hatası: {ex.Message}");
                XtraMessageBox.Show($"Excel hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static int ShowExcelChoiceDialog()
        {
            using (XtraForm frm = new XtraForm())
            {
                frm.Text = "Excel Export Seçeneği";
                frm.Size = new Size(420, 310);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                LabelControl lbl = new LabelControl
                {
                    Text = "Excel'e nasıl aktarmak istersiniz?",
                    Location = new Point(20, 20),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(380, 20)
                };
                var rb1 = new RadioButton { Text = "Sadece fatura başlıkları", Location = new Point(20, 55), AutoSize = true, Checked = true };
                var rb2 = new RadioButton { Text = "Tüm malzeme detayları (düz liste)", Location = new Point(20, 85), AutoSize = true };
                var rb3 = new RadioButton { Text = "Malzeme bazlı toplam özet", Location = new Point(20, 115), AutoSize = true };
                var rb4 = new RadioButton { Text = "Cari bazlı toplam özet", Location = new Point(20, 145), AutoSize = true };
                var rb5 = new RadioButton { Text = "Proje bazlı toplam özet", Location = new Point(20, 175), AutoSize = true };
                int secim = 0;
                SimpleButton btnOK = new SimpleButton
                {
                    Text = "Tamam",
                    Size = new Size(90, 30),
                    Location = new Point(215, 235),
                    DialogResult = DialogResult.OK
                };
                SimpleButton btnIptal = new SimpleButton
                {
                    Text = "İptal",
                    Size = new Size(90, 30),
                    Location = new Point(313, 235),
                    DialogResult = DialogResult.Cancel
                };
                btnOK.Click += (s, ev) =>
                {
                    secim = rb1.Checked ? 1
                          : rb2.Checked ? 2
                          : rb3.Checked ? 3
                          : rb4.Checked ? 4
                          : 5;
                    frm.Close();
                };
                btnIptal.Click += (s, ev) => frm.Close();
                frm.Controls.AddRange(new Control[] { lbl, rb1, rb2, rb3, rb4, rb5, btnOK, btnIptal });
                frm.AcceptButton = btnOK;
                frm.CancelButton = btnIptal;
                frm.ShowDialog();
                return secim;
            }
        }
        private void ExportDetayliExcel(string filePath)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                ExportDetayliExcelToWorkbook(wb);
                wb.SaveAs(filePath);
            }
        }
        private void ExportDetayliExcelToWorkbook(ClosedXML.Excel.XLWorkbook wb)
        {
            var ws = wb.Worksheets.Add("Fatura Detayları");
            string[] headers =
   {
    "Fatura Tipi", "Fatura No", "Tarih", "Vade Tarihi", "Cari Kodu", "Cari Adı",
    "Proje", "Para Birimi", "Fatura Kur", "Fatura KDV'siz", "Fatura KDV", "Fatura KDV (Döviz)",
    "Fatura Toplam TL", "Fatura Toplam Döviz", "Fatura Açıklaması",
    "Malzeme Kodu", "Malzeme Adı", "Satır Açıklaması", "Miktar", "Birim",
    "Birim Fiyat TL", "Satır Toplam TL", "KDV Oranı %", "KDV TL", "KDV (Döviz)",
    "Satır Döviz", "Satır Kur", "Satır Toplam Döviz"
};
            // Başlık satırı
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(1, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(68, 84, 106);
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }
            // Veri satırları
            int row = 2;
            foreach (PurInvoiceMasterModel f in tumFaturalar)
            {
                foreach (PurInvoiceDetayModel d in f.Detaylar)
                {
                    bool isIade = f.FATURA_TIPI == "SATIN ALMA IADE";
                    var bgColor = isIade
                        ? XLColor.FromArgb(255, 230, 230) // iade satırları açık kırmızı
                        : XLColor.NoColor;
                    object[] vals =
     {
    f.FATURA_TIPI ?? "",
    f.FATURA_NO   ?? "",
    f.TARIHI.HasValue ? f.TARIHI.Value.ToString("dd.MM.yyyy") : "",
    f.FATURA_VADE_TARIHI.HasValue ? f.FATURA_VADE_TARIHI.Value.ToString("dd.MM.yyyy") : "",
    f.CARI_KODU         ?? "",
    f.CARI_ACIKLAMASI   ?? "",
    f.PROJE_KODU        ?? "",
    f.PARA_BIRIMI       ?? "",
    (double)f.KUR,                        // "Fatura Kur"
    (double)f.FATURA_KDVSIZ_TUTAR,        // "Fatura KDV'siz"
    (double)f.KDV_TUTARI,                 // "Fatura KDV"
    (double)f.KDV_TUTARI_DOVIZ,           // "Fatura KDV (Döviz)"
    (double)f.FATURA_TOPLAM_TUTAR_TL,     // "Fatura Toplam TL"
    (double)f.FATURA_TOPLAM_TUTAR_ID,     // "Fatura Toplam Döviz"
    f.FATURA_ACIKLAMASI ?? "",            // "Fatura Açıklaması"
    d.MALZEME_KODU      ?? "",
    d.MALZEME_ADI       ?? "",
    d.SATIR_ACIKLAMASI  ?? "",
    (double)d.MIKTAR,
    d.BIRIM             ?? "",
    (double)d.BIRIM_FIYAT,
    (double)d.SATIR_TOPLAM_TL,
    (double)d.SATIR_KDV_ORANI,
    (double)d.SATIR_KDV_TL,              // "KDV TL"
    (double)d.SATIR_KDV_DOVIZ,           // "KDV (Döviz)"
    d.SATIR_DOVIZ       ?? "",
    (double)d.SATIR_KUR,
    (double)d.SATIR_TOPLAM_DOVIZ,
};
                    for (int c = 0; c < vals.Length; c++)
                    {
                        var cell = ws.Cell(row, c + 1);
                        if (vals[c] is double dv) cell.Value = dv;
                        else cell.Value = vals[c]?.ToString() ?? "";
                        if (isIade)
                            cell.Style.Fill.BackgroundColor = bgColor;
                        // Sayısal format
                        if (vals[c] is double)
                        {
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                        }
                    }
                    row++;
                }
            }
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();
        }
        private void ExportMalzemeBazliOzet(string filePath)
        {
            var satirlar = tumFaturalar
                .SelectMany(f => f.Detaylar.Select(d => new
                {
                    d.MALZEME_KODU,
                    d.MALZEME_ADI,
                    d.MIKTAR,
                    d.SATIR_TOPLAM_TL,
                    d.SATIR_KDV_TL,
                    d.SATIR_TOPLAM_DOVIZ,
                })).ToList();
            var ozet = satirlar
                .GroupBy(s => new { s.MALZEME_KODU, s.MALZEME_ADI })
                .Select(g => new
                {
                    MalzemeKodu = g.Key.MALZEME_KODU,
                    MalzemeAdi = g.Key.MALZEME_ADI,
                    SatirSayisi = g.Count(),
                    MiktarToplam = g.Sum(x => x.MIKTAR),
                    TutarKDVsiz = g.Sum(x => x.SATIR_TOPLAM_TL),
                    KDVToplam = g.Sum(x => x.SATIR_KDV_TL),
                    KDVDahil = g.Sum(x => x.SATIR_TOPLAM_TL) + g.Sum(x => x.SATIR_KDV_TL),
                })
                .OrderByDescending(x => x.TutarKDVsiz)
                .ToList();
            string[] headers = { "Malzeme Kodu", "Malzeme Adı", "Satır Sayısı", "Miktar Toplamı", "Tutar (KDV Hariç)", "KDV Tutarı", "Tutar (KDV Dahil)" };
            int[] colWidths = { 22, 45, 14, 16, 20, 18, 20 };
            var exportSatirlar = ozet.Select(x => new object[]
            {
        x.MalzemeKodu, x.MalzemeAdi,
        x.SatirSayisi,
        (double)x.MiktarToplam,
        (double)x.TutarKDVsiz,
        (double)x.KDVToplam,
        (double)x.KDVDahil,
            }).ToList();
            decimal[] toplamlar =
           {
    (decimal)ozet.Sum(x => x.SatirSayisi),  // ← bu da int
    ozet.Sum(x => x.MiktarToplam),
    ozet.Sum(x => x.TutarKDVsiz),
    ozet.Sum(x => x.KDVToplam),
    ozet.Sum(x => x.KDVDahil),
};
            using (XLWorkbook wb = new XLWorkbook())
            {
                ExportOzetToWorkbook(wb, "Malzeme Özeti",
                    "MALZEME BAZLI SATIN ALMA ANALİZİ",
                    $"Toplam {tumFaturalar.Count} fatura  |  {satirlar.Count} satır  |  {ozet.Count} malzeme  (iadeler düşülmüştür)",
                    headers, colWidths, exportSatirlar, toplamlar,
                    toplamBaslangicIdx: 2); // Malzeme Kodu, Malzeme Adı metin — 3. kolondan sayısal
                ExportDetayliExcelToWorkbook(wb);
                wb.SaveAs(filePath);
            }
        }
        #endregion

        #region Gruplama (btn_GrpProduct)
        private void btn_GrpProduct_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataTable dt = new DataTable("MalzemeListe");
            dt.Columns.Add("MALZEME_KODU", typeof(string));
            dt.Columns.Add("MALZEME_ADI", typeof(string));
            dt.Columns.Add("SATIR_ACIKLAMASI", typeof(string));
            dt.Columns.Add("MIKTAR", typeof(decimal));
            dt.Columns.Add("BIRIM", typeof(string));
            dt.Columns.Add("BIRIM_FIYAT", typeof(decimal));
            dt.Columns.Add("SATIR_TOPLAM_TL", typeof(decimal));
            dt.Columns.Add("SATIR_KDV_ORANI", typeof(decimal));
            dt.Columns.Add("SATIR_KDV_TL", typeof(decimal));
            dt.Columns.Add("SATIR_KDV_DOVIZ", typeof(decimal));  // ← SATIR_KDV_TL'nin hemen ardına
            dt.Columns.Add("SATIR_DOVIZ", typeof(string));
            dt.Columns.Add("SATIR_KUR", typeof(decimal));
            dt.Columns.Add("SATIR_TOPLAM_DOVIZ", typeof(decimal));
            dt.Columns.Add("FATURA_NO", typeof(string));
            dt.Columns.Add("TARIHI", typeof(DateTime));
            dt.Columns.Add("CARI_ACIKLAMASI", typeof(string));
            dt.Columns.Add("FATURA_TIPI", typeof(string));
            dt.Columns.Add("PROJE_KODU", typeof(string));
            foreach (PurInvoiceMasterModel f in tumFaturalar)
                foreach (PurInvoiceDetayModel d in f.Detaylar)
                {
                    DataRow row = dt.NewRow();
                    row["MALZEME_KODU"] = d.MALZEME_KODU ?? "";
                    row["MALZEME_ADI"] = d.MALZEME_ADI ?? "";
                    row["SATIR_ACIKLAMASI"] = d.SATIR_ACIKLAMASI ?? "";
                    row["MIKTAR"] = d.MIKTAR;
                    row["BIRIM"] = d.BIRIM ?? "";
                    row["BIRIM_FIYAT"] = d.BIRIM_FIYAT;
                    row["SATIR_TOPLAM_TL"] = d.SATIR_TOPLAM_TL;
                    row["SATIR_KDV_ORANI"] = d.SATIR_KDV_ORANI;
                    row["SATIR_KDV_DOVIZ"] = d.SATIR_KDV_DOVIZ;
                    row["SATIR_KDV_TL"] = d.SATIR_KDV_TL;
                    row["SATIR_DOVIZ"] = d.SATIR_DOVIZ ?? "";
                    row["SATIR_KUR"] = d.SATIR_KUR;
                    row["SATIR_TOPLAM_DOVIZ"] = d.SATIR_TOPLAM_DOVIZ;
                    row["FATURA_NO"] = f.FATURA_NO ?? "";
                    row["TARIHI"] = f.TARIHI.HasValue ? (object)f.TARIHI.Value : DBNull.Value;
                    row["CARI_ACIKLAMASI"] = f.CARI_ACIKLAMASI ?? "";
                    row["FATURA_TIPI"] = f.FATURA_TIPI ?? "";
                    row["PROJE_KODU"] = f.PROJE_KODU ?? "";
                    dt.Rows.Add(row);
                }
            // Master-detail kapat, kolonları temizle
            gridView1.OptionsDetail.EnableMasterViewMode = false;
            gridView1.ClearGrouping();
            gridView1.Columns.Clear();
            gridControl1.DataSource = dt;
            gridView1.PopulateColumns();
            // Tüm kolonları önce gizle
            foreach (GridColumn col in gridView1.Columns)
                col.VisibleIndex = -1;
            // Sonra sırayla görünür yap
            var kolonlar = new (string Field, string Caption, int Width)[]
            {
    ("MALZEME_KODU",       "Malzeme Kodu",     130),
    ("MALZEME_ADI",        "Malzeme Adı",      250),
    ("SATIR_ACIKLAMASI",   "Satır Açıklaması", 200),
    ("FATURA_NO",          "Fatura No",        150),
    ("TARIHI",             "Tarih",             90),
    ("CARI_ACIKLAMASI",    "Cari Adı",         200),
    ("FATURA_TIPI",        "Fatura Tipi",      130),
    ("PROJE_KODU",         "Proje",            100),
    ("MIKTAR",             "Miktar",            80),
    ("BIRIM",              "Birim",             60),
    ("BIRIM_FIYAT",        "Birim Fiyat",      110),
    ("SATIR_TOPLAM_TL",    "Toplam TL",        110),
    ("SATIR_KDV_ORANI",    "KDV %",             60),
    ("SATIR_KDV_TL",       "KDV TL",           100),
    ("SATIR_KDV_DOVIZ",    "KDV (Döviz)",      100),
    ("SATIR_DOVIZ",        "Döviz",             60),
    ("SATIR_KUR",          "Kur",               75),
    ("SATIR_TOPLAM_DOVIZ", "Toplam Döviz",     110),
            };
            int idx = 0;
            foreach (var k in kolonlar)
            {
                GridColumn col = gridView1.Columns[k.Field];
                if (col == null) continue;
                col.Caption = k.Caption;
                col.Width = k.Width;
                col.VisibleIndex = idx++;
                if (k.Field == "MIKTAR" || k.Field == "BIRIM_FIYAT" ||
         k.Field == "SATIR_TOPLAM_TL" || k.Field == "SATIR_TOPLAM_DOVIZ" ||
         k.Field == "SATIR_KDV_TL" || k.Field == "SATIR_KDV_ORANI" ||
         k.Field == "SATIR_KUR" || k.Field == "SATIR_KDV_DOVIZ")  // ← EKLE
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "N2";
                }
                if (k.Field == "TARIHI")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    col.DisplayFormat.FormatString = "dd.MM.yyyy";
                }
            }
            if (gridView1.Columns["SATIR_TOPLAM_TL"] != null)
            {
                gridView1.Columns["SATIR_TOPLAM_TL"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["SATIR_TOPLAM_TL"].SummaryItem.DisplayFormat = "{0:N2} TL";
            }
            if (gridView1.Columns["SATIR_KDV_TL"] != null)
            {
                gridView1.Columns["SATIR_KDV_TL"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["SATIR_KDV_TL"].SummaryItem.DisplayFormat = "{0:N2} TL";
            }
            if (gridView1.Columns["SATIR_KDV_DOVIZ"] != null)
            {
                gridView1.Columns["SATIR_KDV_DOVIZ"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["SATIR_KDV_DOVIZ"].SummaryItem.DisplayFormat = "{0:N2}";
            }
            // Malzeme adına göre grupla
            gridView1.OptionsView.ShowGroupPanel = true;
            if (gridView1.Columns["MALZEME_ADI"] != null)
                gridView1.Columns["MALZEME_ADI"].GroupIndex = 0;
            gridView1.ExpandAllGroups();
            currentGroupMode = GroupMode.ByProduct;
            btn_GrpProduct.Enabled = false;
            btnGrupla.Enabled = false;
            btnGrubuCoz.Enabled = true;
        }
        #endregion

        #region Diğer
        private void PurInvoiceForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }
        #endregion

    }
}