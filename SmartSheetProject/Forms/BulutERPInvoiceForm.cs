using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;
using SmartSheetProject.Models;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Diagnostics;
using System.ComponentModel;

namespace SmartSheetProject.Forms
{
    public partial class BulutERPInvoiceForm : XtraForm
    {
        private List<GroupedExpenseModel> tumExpenses = new List<GroupedExpenseModel>();
        private List<GroupedExpenseModel> grupluExpenses = new List<GroupedExpenseModel>();
        private HashSet<string> logoFaturaNumaralari = new HashSet<string>();
        private bool gruplama = true;
        public BulutERPInvoiceForm()
        {
            InitializeComponent();
        }
        private async void BulutERPInvoice_Load(object sender, EventArgs e)
        {
            try
            {
                // Bulut ERP ayarları kontrolü
                var bulutERPSettingsResult = await BulutERPConnectionTest.GetSettingsAsync();
                if (!bulutERPSettingsResult.Success)
                {
                    await TextLog.LogToSQLiteAsync("❌ BulutERPInvoice - Bulut ERP ayarları bulunamadı");
                    XtraMessageBox.Show(
                        "Bulut ERP ayarları bulunamadı! Lütfen önce Bulut ERP ayarlarını yapılandırın.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
                // Smartsheet token kontrolü
                bool smartsheetTokenKayitli = await SmartsheetService.IsTokenSavedAsync();
                if (!smartsheetTokenKayitli)
                {
                    await TextLog.LogToSQLiteAsync("❌ BulutERPInvoice - SmartSheet API Token kayıtlı değil");
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
                // Event'leri ekle
                btnYenile.ItemClick += BtnYenile_ItemClick;
                btnExcel.ItemClick += BtnExcel_ItemClick;
                btnAktarLogo.Click += BtnAktarLogo_Click;
                btnGrubuCoz.Click += BtnGrubuCoz_Click;
                btn_AktarilmayanlarinHepsiniSec.Click += btn_AktarilmayanlarinHepsiniSec_Click;
                btnTumunuSec.Click += BtnTumunuSec_Click;
                btnSecimiTemizle.Click += BtnSecimiTemizle_Click;
                btnFiltrele.Click += btnFiltrele_Click;
                gridView1.SelectionChanged += GridView1_SelectionChanged_UpdateLabel;
                gridView1.SelectionChanged += GridView1_SelectionChanged;
                gridView1.CustomDrawCell += GridView1_CustomDrawCell;
              //  await LoadDataAsync(applyDateFilter: true);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ BulutERPInvoice_Load hatası: {ex.Message}");
                XtraMessageBox.Show($"Form yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        private void ConfigureGrid()
        {
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowAutoFilterRow = true;
            gridView1.OptionsView.ShowFooter = true;
            gridView1.OptionsView.ShowGroupPanel = true;
            gridView1.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
        }
        private async Task LoadDataAsync(bool applyDateFilter = true)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                gridControl1.DataSource = null;
                if (dateBaslangic.EditValue == null || dateBitis.EditValue == null)
                {
                    XtraMessageBox.Show("Başlangıç ve bitiş tarihi seçilmeli!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var result = await SmartsheetService.GetGroupedApprovedExpensesAsync();
                if (!result.Success)
                {
                    await TextLog.LogToSQLiteAsync($"❌ BulutERPInvoice - Smartsheet veri çekme hatası: {result.ErrorMessage}");
                    XtraMessageBox.Show($"Smartsheet'ten veri çekilirken hata:\n{result.ErrorMessage}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (result.GroupedExpenses == null || result.GroupedExpenses.Count == 0)
                {
                    XtraMessageBox.Show("Onaylanmış expense bulunamadı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                tumExpenses = result.GroupedExpenses;
                // Tarih filtresi uygula
                DateTime baslangic = Convert.ToDateTime(dateBaslangic.EditValue).Date;
                DateTime bitis = Convert.ToDateTime(dateBitis.EditValue).Date;
                grupluExpenses = tumExpenses
                    .Where(g => g.FaturaTarihi.HasValue &&
                                g.FaturaTarihi.Value.Date >= baslangic &&
                                g.FaturaTarihi.Value.Date <= bitis)
                    .ToList();
                // Her grup için cari kodu çek 
                // Logo'da var olan faturaları kontrol et
                await CheckLogoInvoicesAsync();
                // Grid'e flat liste olarak yükle
                var flatList = grupluExpenses.SelectMany(g => g.Items.Select(item => new
                {
                    FaturaNo = g.FaturaNo,
                    FaturaTarihi = g.FaturaTarihi,
                    KayitEdenKullanici = g.KayitEdenKullanici,
                    SirketAdi = item.SirketAdi,
                    ProjeKodu = g.ProjeKodu,
                    DovizTuru = g.DovizTuru,
                    CariKodu = g.CariKodu,
                    TumHatalar = g.TumHatalar,
                    GrupToplamTutar = g.ToplamTutar,
                    LogoReference = g.LogoReference,
                    LogodaVar = logoFaturaNumaralari.Contains(g.LogoReference),
                    UID = item.UID,
                    FaturaAciklamasi = g.FaturaAciklamasi,
                    MalzemeListesi = item.MalzemeListesi,
                    KDV = item.KDV,
                    KDVOrani = item.KDVOrani,
                    BirimFiyat = item.BirimFiyat,
                    SatirToplamTutar = item.SatirToplamTutar,
                    MuhasebeOnay = item.MuhasebeOnay,
                    YoneticiOnay = item.YoneticiOnay,
                    SupervisorOnay = item.SupervisorApproval
                })).ToList();
                gridControl1.DataSource = flatList;
                ConfigureColumns();
                SetFooterTotals();
                // Gruplama durumuna göre ayarla
                gridView1.ClearGrouping();
                if (gruplama)
                {
                    gridView1.Columns["LogoReference"].GroupIndex = 0;
                    gridView1.Columns["FaturaNo"].GroupIndex = 1;
                    gridView1.Columns["FaturaTarihi"].GroupIndex = 2;
                    gridView1.Columns["KayitEdenKullanici"].GroupIndex = 3;
                    gridView1.ExpandAllGroups();
                }
                gridView1.BestFitColumns();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ BulutERPInvoice LoadDataAsync hatası: {ex.Message}\nStackTrace: {ex.StackTrace}");
                XtraMessageBox.Show($"Beklenmeyen hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private async Task CheckLogoInvoicesAsync()
        {
            logoFaturaNumaralari.Clear();
            foreach (GroupedExpenseModel grup in grupluExpenses)
            {
                if (string.IsNullOrWhiteSpace(grup.CariKodu))
                    continue;
                var checkResult = await BulutERPService.CheckInvoiceExistsAsync(
                    grup.FaturaNo,
                    grup.CariKodu,
                    grup.FaturaTarihi ?? DateTime.Now,
                    grup.LogoReference);
                if (checkResult.Success && checkResult.Exists)
                    logoFaturaNumaralari.Add(grup.LogoReference);
                else if (!checkResult.Success)
                    await TextLog.LogToSQLiteAsync($"⚠️ Logo kontrolü yapılamadı: {grup.FaturaNo} - {checkResult.ErrorMessage}");
            }
        }
        private void ConfigureColumns()
        {
            var kolonlar = new List<Tuple<string, string, int, int>>
            {
                Tuple.Create("FaturaNo", "Fatura No", 0, 120),
                Tuple.Create("FaturaTarihi", "Fatura Tarihi", 1, 100),
                Tuple.Create("KayitEdenKullanici", "Kayıt Eden", 2, 200),
                Tuple.Create("SirketAdi", "Şirket Adı", 3, 200),
                Tuple.Create("ProjeKodu", "Proje Kodu", 4, 120),
                Tuple.Create("DovizTuru", "Döviz", 5, 80),
                Tuple.Create("CariKodu", "Cari Kodu", 6, 120),
                Tuple.Create("TumHatalar", "Hatalar", 7, 300),
                Tuple.Create("GrupToplamTutar", "Grup Toplam", 8, 120),
                Tuple.Create("LogodaVar", "Logo'da Var?", 9, 100),
                Tuple.Create("UID", "UID", 10, 80),
                Tuple.Create("FaturaAciklamasi", "Fatura Genel Açıklaması", 11, 250),
                Tuple.Create("MalzemeListesi", "Malzeme", 12, 150),
                Tuple.Create("KDV", "KDV?", 13, 80),
                Tuple.Create("KDVOrani", "KDV %", 14, 80),
                Tuple.Create("SatirToplamTutar", "Satır Toplam", 15, 120),
                Tuple.Create("MuhasebeOnay", "Muh. Onay", 16, 100),
                Tuple.Create("YoneticiOnay", "Yön. Onay", 17, 100),
                Tuple.Create("SupervisorOnay", "Süpervizör Onay", 18, 120)
            };
            foreach (var kolon in kolonlar)
            {
                GridColumn col = gridView1.Columns[kolon.Item1];
                if (col != null)
                {
                    col.Caption = kolon.Item2;
                    col.VisibleIndex = kolon.Item3;
                    col.Width = kolon.Item4;
                    if (kolon.Item1.Contains("Tutar") || kolon.Item1 == "KDVOrani")
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "N2";
                    }
                    if (kolon.Item1.Contains("Tarihi"))
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        col.DisplayFormat.FormatString = "dd.MM.yyyy";
                    }
                    if (kolon.Item1 == "TumHatalar")
                        col.AppearanceCell.ForeColor = Color.Red;
                }
            }
            if (gridView1.Columns["BirimFiyat"] != null)
                gridView1.Columns["BirimFiyat"].Visible = false;
        }
        private void SetFooterTotals()
        {
            if (gridView1.Columns["SatirToplamTutar"] != null)
            {
                gridView1.Columns["SatirToplamTutar"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["SatirToplamTutar"].SummaryItem.DisplayFormat = "{0:N2}";
            }
            if (gridView1.Columns["GrupToplamTutar"] != null)
            {
                gridView1.Columns["GrupToplamTutar"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gridView1.Columns["GrupToplamTutar"].SummaryItem.DisplayFormat = "{0:N2}";
            }
            if (gridView1.Columns["UID"] != null)
            {
                gridView1.Columns["UID"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
                gridView1.Columns["UID"].SummaryItem.DisplayFormat = "Toplam: {0} Satır";
            }
        }
        private void GridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0) return;
            object row = gridView1.GetRow(e.RowHandle);
            if (row == null) return;
            var logodaVarProperty = row.GetType().GetProperty("LogodaVar");
            if (logodaVarProperty != null)
            {
                bool logodaVar = (bool)logodaVarProperty.GetValue(row, null);
                if (logodaVar)
                {
                    e.Appearance.BackColor = Color.LightGreen;
                    e.Appearance.BackColor2 = Color.White;
                }
            }
        }
        private void GridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Add && e.ControllerRow >= 0)
            {
                object selectedRow = gridView1.GetRow(e.ControllerRow);
                if (selectedRow != null)
                {
                    var logoRefProperty = selectedRow.GetType().GetProperty("LogoReference");
                    if (logoRefProperty != null)
                    {
                        string secilenLogoRef = logoRefProperty.GetValue(selectedRow, null) as string;
                        if (!string.IsNullOrWhiteSpace(secilenLogoRef))
                        {
                            for (int i = 0; i < gridView1.DataRowCount; i++)
                            {
                                object row = gridView1.GetRow(i);
                                if (row != null)
                                {
                                    var logoRef = logoRefProperty.GetValue(row, null) as string;
                                    if (logoRef == secilenLogoRef)
                                        gridView1.SelectRow(i);
                                }
                            }
                        }
                    }
                }
            }
        }
        private async void BtnYenile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await LoadDataAsync(applyDateFilter: true);
        }
        private async void BtnExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (grupluExpenses.Count == 0)
                {
                    XtraMessageBox.Show("Excel'e aktarılacak veri yok!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                    FileName = $"Smartsheet_Expenses_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    gridView1.ExportToXlsx(saveDialog.FileName);
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
                await TextLog.LogToSQLiteAsync($"❌ BulutERPInvoice Excel aktarma hatası: {ex.Message}");
                XtraMessageBox.Show($"Excel aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void BtnAktarLogo_Click(object sender, EventArgs e)
        {
            if (!btnAktarLogo.Enabled)
                return;
            btnAktarLogo.Enabled = false;
            try
            {
                var selectedRows = gridView1.GetSelectedRows();
                if (selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen aktarılacak faturaları seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                HashSet<string> secilenLogoRefler = new HashSet<string>();
                foreach (int rowHandle in selectedRows)
                {
                    if (rowHandle >= 0)
                    {
                        var row = gridView1.GetRow(rowHandle);
                        if (row != null)
                        {
                            var logoRefProperty = row.GetType().GetProperty("LogoReference");
                            if (logoRefProperty != null)
                            {
                                string logoRef = logoRefProperty.GetValue(row, null) as string;
                                if (!string.IsNullOrWhiteSpace(logoRef))
                                    secilenLogoRefler.Add(logoRef);
                            }
                        }
                    }
                }
                if (secilenLogoRefler.Count == 0)
                {
                    XtraMessageBox.Show("Geçerli fatura seçimi bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var aktarilacakGruplar = grupluExpenses
                    .Where(g => secilenLogoRefler.Contains(g.LogoReference))
                    .ToList();
                var logodaVarOlanlar = aktarilacakGruplar
                    .Where(g => logoFaturaNumaralari.Contains(g.LogoReference))
                    .ToList();
                if (logodaVarOlanlar.Count > 0)
                {
                    string mesaj = $"⚠️ {logodaVarOlanlar.Count} fatura zaten Logo'da mevcut:\n\n";
                    mesaj += string.Join("\n", logodaVarOlanlar.Select(x => $"• {x.LogoReference} ({x.FaturaNo})").Take(5));
                    if (logodaVarOlanlar.Count > 5)
                        mesaj += $"\n...ve {logodaVarOlanlar.Count - 5} fatura daha";
                    mesaj += "\n\nBu faturalar atlanacak. Devam etmek istiyor musunuz?";
                    DialogResult logodaVarOnay = XtraMessageBox.Show(mesaj, "Dikkat", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (logodaVarOnay != DialogResult.Yes)
                        return;
                }
                var hatalilar = aktarilacakGruplar
                    .Where(g => !string.IsNullOrWhiteSpace(g.TumHatalar))
                    .ToList();
                if (hatalilar.Count == aktarilacakGruplar.Count)
                {
                    XtraMessageBox.Show(
                        $"TÜM SEÇİLİ FATURALARDA HATA VAR!\n\nLütfen hataları düzeltin:\n\n{string.Join("\n", hatalilar.Select(x => $"• {x.LogoReference} ({x.FaturaNo}): {x.TumHatalar}").Take(5))}",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
               string onayMesaji = $"✅ {aktarilacakGruplar.Count - logodaVarOlanlar.Count - hatalilar.Count} fatura Logo'ya aktarılacak.";
                if (hatalilar.Count > 0)
                {
                    onayMesaji += $"\n⚠️ {hatalilar.Count} faturada hata var (atlanacak):\n";
                    onayMesaji += string.Join("\n", hatalilar.Select(x => $"• {x.LogoReference} ({x.FaturaNo}): {x.TumHatalar}").Take(3));
                    if (hatalilar.Count > 3)
                        onayMesaji += $"\n...ve {hatalilar.Count - 3} fatura daha";
                }
                onayMesaji += "\n\nDevam etmek istiyor musunuz?";
                DialogResult onay = XtraMessageBox.Show(onayMesaji, "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (onay != DialogResult.Yes)
                    return;
                // Progress panel
                DevExpress.XtraWaitForm.ProgressPanel progressPanel = new DevExpress.XtraWaitForm.ProgressPanel();
                progressPanel.AutoHeight = true;
                progressPanel.AutoWidth = true;
                progressPanel.Appearance.BackColor = System.Drawing.Color.Transparent;
                progressPanel.Appearance.Options.UseBackColor = true;
                progressPanel.AppearanceCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
                progressPanel.AppearanceCaption.Options.UseFont = true;
                progressPanel.AppearanceDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
                progressPanel.AppearanceDescription.Options.UseFont = true;
                progressPanel.Caption = "Logo'ya Aktarılıyor";
                progressPanel.Description = "Lütfen bekleyin...";
                progressPanel.Dock = DockStyle.Fill;
                XtraForm progressForm = new XtraForm
                {
                    Width = 500,
                    Height = 150,
                    Text = "Logo Aktarım",
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    ControlBox = false,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false
                };
                progressForm.Controls.Add(progressPanel);
                progressForm.Show();
                progressForm.Refresh();
                int basarili = 0;
                int hatali = 0;
                int atlandi = 0;
                List<string> hataMesajlari = new List<string>();
                int toplam = aktarilacakGruplar.Count;
                int simdiki = 0;
                foreach (GroupedExpenseModel grup in aktarilacakGruplar)
                {
                    simdiki++;
                    progressPanel.Caption = $"Logo'ya Aktarılıyor ({simdiki}/{toplam})";
                    progressPanel.Description = $"İşleniyor: {grup.LogoReference} ({grup.FaturaNo})";
                    progressForm.Refresh();
                    Application.DoEvents();
                    if (logoFaturaNumaralari.Contains(grup.LogoReference))
                    {
                        atlandi++;
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(grup.TumHatalar))
                    {
                        hatali++;
                        hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): {grup.TumHatalar}");
                        await TextLog.LogToSQLiteAsync($"❌ Atlandı (Hata var): {grup.LogoReference} - {grup.TumHatalar}");
                        continue;
                    }
                    bool isCreditCard = grup.PaymentType?.Trim() == "Credit Card";
                    if (isCreditCard)
                    {
                        var cariResult = await BulutERPService.GetCariAuxCode5Async(grup.CariKodu);
                        if (!cariResult.Success)
                        {
                            hatali++;
                            hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): Banka hesabı alınamadı - {cariResult.ErrorMessage}");
                            continue;
                        }
                        var convertSlipResult = await BulutERPService.ConvertGroupedExpenseToBankSlipAsync(grup, grup.CariKodu, cariResult.AuxCode5);
                        if (!convertSlipResult.Success)
                        {
                            hatali++;
                            hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): {convertSlipResult.ErrorMessage}");
                            continue;
                        }
                        var slipResult = await BulutERPService.CreateBankSlipAsync(convertSlipResult.SlipData);
                        if (slipResult.Success)
                        {
                            basarili++;
                            logoFaturaNumaralari.Add(grup.LogoReference);
                            var rowIds = grup.Items.Select(i => i.SmartsheetRowId).Where(id => id > 0).ToList();
                            if (rowIds.Count > 0)
                            {
                                var markResult = await SmartsheetService.MarkAsTransferredToLogoAsync(rowIds);
                                if (!markResult.Success)
                                    await TextLog.LogToSQLiteAsync($"⚠️ Checkbox güncellenemedi: {grup.LogoReference} - {markResult.ErrorMessage}");
                            }
                            else
                                await TextLog.LogToSQLiteAsync($"⚠️ SmartsheetRowId bulunamadı, checkbox atlandı: {grup.LogoReference}");
                        }
                        else
                        {
                            hatali++;
                            hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): {slipResult.ErrorMessage}");
                            await TextLog.LogToSQLiteAsync($"❌ Logo BankSlip hatası: {grup.LogoReference} - {slipResult.ErrorMessage}");
                        }
                    }
                    else
                    {
                        var convertInvoiceResult = await BulutERPService.ConvertGroupedExpenseToInvoiceAsync(grup, grup.CariKodu);
                        if (!convertInvoiceResult.Success)
                        {
                            hatali++;
                            hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): {convertInvoiceResult.ErrorMessage}");
                            continue;
                        }
                        var invoiceResult = await BulutERPService.CreateInvoiceAsync(convertInvoiceResult.InvoiceData, convertInvoiceResult.InvoiceType);
                        if (invoiceResult.Success)
                        {
                            basarili++;
                            logoFaturaNumaralari.Add(grup.LogoReference);
                            var rowIds = grup.Items.Select(i => i.SmartsheetRowId).Where(id => id > 0).ToList();
                            if (rowIds.Count > 0)
                            {
                                var markResult = await SmartsheetService.MarkAsTransferredToLogoAsync(rowIds);
                                if (!markResult.Success)
                                    await TextLog.LogToSQLiteAsync($"⚠️ Checkbox güncellenemedi: {grup.LogoReference} - {markResult.ErrorMessage}");
                            }
                            else
                                await TextLog.LogToSQLiteAsync($"⚠️ SmartsheetRowId bulunamadı, checkbox atlandı: {grup.LogoReference}");
                        }
                        else
                        {
                            hatali++;
                            hataMesajlari.Add($"{grup.LogoReference} ({grup.FaturaNo}): {invoiceResult.ErrorMessage}");
                            await TextLog.LogToSQLiteAsync($"❌ Logo aktarım hatası: {grup.LogoReference} - {invoiceResult.ErrorMessage}");
                        }
                    }
                }
                progressForm.Close();
                progressForm.Dispose();
                string sonucMesaji = $"🎉 Aktarım Tamamlandı!\n\n✅ Başarılı: {basarili} fatura\n❌ Hatalı: {hatali} fatura";
                if (atlandi > 0)
                    sonucMesaji += $"\n⏭️ Atlandı (Logo'da var): {atlandi} fatura";
                if (hataMesajlari.Count > 0)
                {
                    sonucMesaji += "\n\n❌ Hatalar:\n";
                    sonucMesaji += string.Join("\n", hataMesajlari.Take(5).Select(x => "  • " + x));
                    if (hataMesajlari.Count > 5)
                        sonucMesaji += $"\n  ...ve {hataMesajlari.Count - 5} hata daha";
                }
                XtraMessageBox.Show(
                    sonucMesaji,
                    "Aktarım Sonucu",
                    MessageBoxButtons.OK,
                    hatali > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ BtnAktarLogo_Click hatası: {ex.Message}");
                XtraMessageBox.Show(
                    $"Beklenmeyen hata oluştu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnAktarLogo.Enabled = true;
            }
        }
        private void BtnGrubuCoz_Click(object sender, EventArgs e)
        {
            try
            {
                if (gruplama)
                {
                    gridView1.ClearGrouping();
                    btnGrubuCoz.Text = "Grupla";
                    gruplama = false;
                }
                else
                {
                    gridView1.ClearGrouping();
                    gridView1.Columns["LogoReference"].GroupIndex = 0;
                    gridView1.Columns["FaturaNo"].GroupIndex = 1;
                    gridView1.Columns["FaturaTarihi"].GroupIndex = 2;
                    gridView1.Columns["KayitEdenKullanici"].GroupIndex = 3;
                    gridView1.ExpandAllGroups();
                    btnGrubuCoz.Text = "Grubu Çöz";
                    gruplama = true;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Gruplama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btn_AktarilmayanlarinHepsiniSec_Click(object sender, EventArgs e)
        {
            try
            {
                gridView1.ClearSelection();
                int secilenSayi = 0;
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    object row = gridView1.GetRow(i);
                    if (row != null)
                    {
                        var logodaVarProperty = row.GetType().GetProperty("LogodaVar");
                        if (logodaVarProperty != null)
                        {
                            bool logodaVar = (bool)logodaVarProperty.GetValue(row, null);
                            if (!logodaVar)
                            {
                                gridView1.SelectRow(i);
                                secilenSayi++;
                            }
                        }
                    }
                }
                XtraMessageBox.Show(
                    $"Aktarılmamış {secilenSayi} satır seçildi!",
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Seçim hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnTumunuSec_Click(object sender, EventArgs e)
        {
            gridView1.SelectAll();
        }
        private void BtnSecimiTemizle_Click(object sender, EventArgs e)
        {
            gridView1.ClearSelection();
        }
        private void GridView1_SelectionChanged_UpdateLabel(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int secilenSatirSayisi = gridView1.SelectedRowsCount;
            HashSet<string> secilenFaturalar = new HashSet<string>();
            foreach (int rowHandle in gridView1.GetSelectedRows())
            {
                if (rowHandle >= 0)
                {
                    object row = gridView1.GetRow(rowHandle);
                    if (row != null)
                    {
                        var faturaNoProperty = row.GetType().GetProperty("FaturaNo");
                        if (faturaNoProperty != null)
                        {
                            string faturaNo = faturaNoProperty.GetValue(row, null) as string;
                            if (!string.IsNullOrWhiteSpace(faturaNo))
                                secilenFaturalar.Add(faturaNo);
                        }
                    }
                }
            }
            lblSeciliSayi.Text = $"Seçili: {secilenFaturalar.Count} fatura ({secilenSatirSayisi} satır)";
        }
        private void BulutERPInvoice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
        private async void btnFiltrele_Click(object sender, EventArgs e)
        {
            try
            {
                if (dateBaslangic.EditValue == null || dateBitis.EditValue == null)
                {
                    XtraMessageBox.Show("Lütfen başlangıç ve bitiş tarihlerini seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DateTime baslangic = Convert.ToDateTime(dateBaslangic.EditValue).Date;
                DateTime bitis = Convert.ToDateTime(dateBitis.EditValue).Date;
                if (baslangic > bitis)
                {
                    XtraMessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadDataAsync(applyDateFilter: true);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ BtnFiltrele_Click hatası: {ex.Message}");
                XtraMessageBox.Show($"Filtreleme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnFiltreTemizle_Click(object sender, EventArgs e)
        {
            try
            {
                dateBaslangic.EditValue = DateTime.Now.Date;
                dateBitis.EditValue = DateTime.Now.Date;
                await LoadDataAsync(applyDateFilter: false);
                XtraMessageBox.Show("Tarih filtresi temizlendi, tüm veriler yüklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ BtnFiltreTemizle_Click hatası: {ex.Message}");
                XtraMessageBox.Show($"Filtre temizleme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}