using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class ErrorListForm : XtraForm
    {
        public ErrorListForm()
        {
            InitializeComponent();
        }
        private async void ErrorListForm_Load(object sender, EventArgs e)
        {
            // Başlangıç tarihi: 30 gün önce
            dateEditBaslangic.EditValue = DateTime.Now.AddDays(-30);
            // Bitiş tarihi: bugün
            dateEditBitis.EditValue = DateTime.Now;
            await LoadErrorLogsAsync();
        }
        private async Task LoadErrorLogsAsync()
        {
            try
            {
                // Tarih kontrolü
                if (dateEditBaslangic.EditValue == null || dateEditBitis.EditValue == null)
                {
                    // Tarihleri varsayılana çek
                    dateEditBaslangic.EditValue = DateTime.Now.AddDays(-30);
                    dateEditBitis.EditValue = DateTime.Now;
                }
                DateTime baslangic = Convert.ToDateTime(dateEditBaslangic.EditValue);
                DateTime bitis = Convert.ToDateTime(dateEditBitis.EditValue).AddDays(1);
                string query = @"
                    SELECT Id, Details, Date_ 
                    FROM ErrorLogs 
                    WHERE Date_ >= @baslangic AND Date_ <= @bitis
                    ORDER BY Id DESC";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@baslangic", baslangic.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "@bitis", bitis.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, parameters);
                gridControlErrors.DataSource = dt;
                labelControlToplam.Text = $"Toplam: {dt.Rows.Count} kayıt";
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"ErrorListForm LoadErrorLogs hatası: {ex.Message}");

                XtraMessageBox.Show($"Hata logları yüklenirken hata oluştu:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnYenile_Click(object sender, EventArgs e)
        {
            await LoadErrorLogsAsync();
        }
        private async void dateEdit_EditValueChanged(object sender, EventArgs e)
        {
            // Tarih değiştiğinde otomatik yenile (sadece her iki tarih de doluysa)
            if (dateEditBaslangic.EditValue != null && dateEditBitis.EditValue != null)
                await LoadErrorLogsAsync();
        }
        private void btnExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridViewErrors.RowCount == 0)
                {
                    XtraMessageBox.Show("Excel'e aktarılacak kayıt yok!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Excel'e Aktar",
                    FileName = $"HataLoglari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    gridViewErrors.ExportToXlsx(saveDialog.FileName);
                    // Dosyayı açmak ister misiniz?
                    DialogResult openResult = XtraMessageBox.Show(
                        "Veriler başarıyla Excel'e aktarıldı!\n\nDosyayı açmak ister misiniz?",
                        "Başarılı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (openResult == DialogResult.Yes)
                       Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Excel'e aktarma hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnTemizle_Click(object sender, EventArgs e)
        {
            DialogResult result = XtraMessageBox.Show(
                "TÜM HATA LOGLARI SİLİNECEK!\n\nEmin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM ErrorLogs";
                    await SQLiteCrud.InsertUpdateDeleteAsync(query);
                    XtraMessageBox.Show("Tüm loglar temizlendi!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadErrorLogsAsync();
                }
                catch (Exception ex)
                {
                    await TextLog.LogToSQLiteAsync($"ErrorListForm Temizle hatası: {ex.Message}");
                    XtraMessageBox.Show($"Temizleme hatası:\n{ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private async void btnSil_Click(object sender, EventArgs e)
        {
            try
            {
                int[] selectedRows = gridViewErrors.GetSelectedRows();
                if (selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen silmek istediğiniz kayıtları seçin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult result = XtraMessageBox.Show(
                    $"{selectedRows.Length} kayıt silinecek. Emin misiniz?",
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    foreach (int rowHandle in selectedRows)
                    {
                        int id = Convert.ToInt32(gridViewErrors.GetRowCellValue(rowHandle, "Id"));
                        string deleteQuery = "DELETE FROM ErrorLogs WHERE Id = @id";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                            { "@id", id }
                        };
                        await SQLiteCrud.InsertUpdateDeleteAsync(deleteQuery, parameters);
                    }
                    XtraMessageBox.Show($"{selectedRows.Length} kayıt silindi!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadErrorLogsAsync();
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"ErrorListForm Sil hatası: {ex.Message}");

                XtraMessageBox.Show($"Silme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ErrorListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}