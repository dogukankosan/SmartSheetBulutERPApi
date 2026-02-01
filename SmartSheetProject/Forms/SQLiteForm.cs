// SQLiteForm.cs
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;
using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSheetProject.Forms
{
    public partial class SQLiteForm : XtraForm
    {
        private string lastExecutedQuery = string.Empty;
        public SQLiteForm()
        {
            InitializeComponent();
        }
        private void SQLiteForm_Load(object sender, EventArgs e)
        {
            // GridView kopyalama ayarları
            gridView1.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsBehavior.CopyToClipboardWithColumnHeaders = false; // Sadece veriyi kopyala
            // Form yüklendiğinde örnek sorgu göster
            memoQuery.Focus();
            memoQuery.SelectionStart = memoQuery.Text.Length;
        }
        private async void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                string query = memoQuery.EditValue?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ShowResult("Lütfen bir sorgu girin!", false);
                    memoQuery.Focus();
                    return;
                }
                lastExecutedQuery = query;
                lblResult.Text = "Sorgu çalıştırılıyor...";
                lblResult.Appearance.ForeColor = System.Drawing.Color.Blue;
                Application.DoEvents();
                string queryUpper = query.ToUpper().TrimStart();
                bool isSelect = queryUpper.StartsWith("SELECT");
                if (isSelect)
                {
                    // Grid'i tamamen temizle
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                    Application.DoEvents();
                    DataTable dt = await ExecuteSelectQueryAsync(query);
                    if (dt != null)
                    {
                        // Yeni veriyi set et
                        gridControl1.DataSource = dt;
                        gridControl1.RefreshDataSource();
                        gridView1.BestFitColumns();

                        ShowResult($"Başarılı! {dt.Rows.Count} kayıt getirildi.", true);
                    }
                    else
                        ShowResult("Sorgu başarısız oldu. Loglara bakın.", false);
                }
                else
                {
                    DialogResult result = XtraMessageBox.Show(
                        $"Bu sorguyu çalıştırmak istediğinizden emin misiniz?\n\n{query}",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        ShowResult("İşlem iptal edildi.", false);
                        return;
                    }
                    int affectedRows = await ExecuteNonQueryAsync(query);
                    if (affectedRows >= 0)
                    {
                        ShowResult($"Sorgu başarıyla çalıştırıldı! {affectedRows} satır etkilendi.", true);
                        // Eğer daha önce SELECT çalıştırılmışsa refresh et
                        if (!string.IsNullOrWhiteSpace(lastExecutedQuery) &&
                            lastExecutedQuery.ToUpper().TrimStart().StartsWith("SELECT"))
                        {
                            await RefreshLastSelectQuery();
                        }
                    }
                    else
                    {
                        ShowResult("Sorgu başarısız oldu. Loglara bakın.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Hata: {ex.Message}", false);
                await TextLog.LogToSQLiteAsync($"SQLiteForm Execute hatası: {ex.Message}");
            }
        }
        private async Task<DataTable> ExecuteSelectQueryAsync(string query)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(SQLiteCrud.connectionString))
                {
                    await conn.OpenAsync();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"SELECT sorgu hatası: {ex.Message} | Query: {query}");
                XtraMessageBox.Show($"Hata: {ex.Message}", "Sorgu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private async Task<int> ExecuteNonQueryAsync(string query)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(SQLiteCrud.connectionString))
                {
                    await conn.OpenAsync();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        int affectedRows = await cmd.ExecuteNonQueryAsync();
                        return affectedRows;
                    }
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"NonQuery hatası: {ex.Message} | Query: {query}");
                XtraMessageBox.Show($"Hata: {ex.Message}", "Sorgu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }
        private async Task RefreshLastSelectQuery()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(lastExecutedQuery) &&
                    lastExecutedQuery.ToUpper().TrimStart().StartsWith("SELECT"))
                {
                    // Grid'i tamamen temizle
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                    Application.DoEvents();
                    DataTable dt = await ExecuteSelectQueryAsync(lastExecutedQuery);
                    if (dt != null)
                    {
                        gridControl1.DataSource = dt;
                        gridControl1.RefreshDataSource();
                        gridView1.BestFitColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"Refresh hatası: {ex.Message}");
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            memoQuery.EditValue = string.Empty;
            memoQuery.Text = string.Empty;
            gridControl1.DataSource = null;
            gridView1.Columns.Clear();
            lblResult.Text = string.Empty;
            lastExecutedQuery = string.Empty;
            memoQuery.Focus();
        }
        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridControl1.DataSource == null)
                {
                    ShowResult("Excel'e aktarılacak veri yok!", false);
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Excel'e Aktar",
                    FileName = $"SQLite_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    gridView1.ExportToXlsx(saveDialog.FileName);
                    ShowResult($"Excel dosyası başarıyla oluşturuldu: {Path.GetFileName(saveDialog.FileName)}", true);
                    DialogResult openResult = XtraMessageBox.Show(
                        "Dosya başarıyla kaydedildi. Açmak ister misiniz?",
                        "Başarılı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (openResult == DialogResult.Yes)
                        Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Excel aktarım hatası: {ex.Message}", false);
                await TextLog.LogToSQLiteAsync($"Excel export hatası: {ex.Message}");
            }
        }
        private async void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string encryptedText = txtEncrypted.EditValue?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(encryptedText))
                {
                    XtraMessageBox.Show("Lütfen şifreli metin girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEncrypted.Focus();
                    return;
                }
                string decryptedText = EncryptionHelper.Decrypt(encryptedText);
                if (!string.IsNullOrWhiteSpace(decryptedText))
                {
                    txtDecrypted.EditValue = decryptedText;
                    XtraMessageBox.Show("Şifre başarıyla çözüldü!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtDecrypted.EditValue = string.Empty;
                    XtraMessageBox.Show("Şifre çözülemedi! Geçerli bir şifreli metin girdiğinizden emin olun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Şifre çözme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await TextLog.LogToSQLiteAsync($"Decrypt hatası: {ex.Message}");
            }
        }
        private void ShowResult(string message, bool success)
        {
            lblResult.Text = message;
            lblResult.Appearance.ForeColor = success ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
        private void SQLiteForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}