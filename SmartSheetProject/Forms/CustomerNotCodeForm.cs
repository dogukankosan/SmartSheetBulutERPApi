using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class CustomerNotCodeForm : XtraForm
    {
        public CustomerNotCodeForm()
        {
            InitializeComponent();
        }
        private async void CustomerNotCodeForm_Load(object sender, EventArgs e)
        {
            await LoadCodesAsync();
        }
        private async Task LoadCodesAsync()
        {
            try
            {
                string query = "SELECT CustomerCode FROM CustomerCodes ORDER BY ID";
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query);
                List<string> codes = new List<string>();
                foreach (DataRow row in dt.Rows)
                    codes.Add(row["CustomerCode"].ToString());
                rch_CustomerCodes.Text = string.Join(Environment.NewLine, codes);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync("CustomerNotCodeForm LoadCodesAsync hatası: " + ex.Message);
                XtraMessageBox.Show("Veriler yüklenirken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                var lines = rch_CustomerCodes.Text
                    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Distinct()
                    .ToList();
                if (lines.Count == 0)
                {
                    XtraMessageBox.Show("Lütfen en az bir müşteri kodu girin.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Önce tümünü temizle
                await SQLiteCrud.InsertUpdateDeleteAsync("DELETE FROM CustomerCodes");
               // Sonra hepsini tekrar ekle
                int eklenen = 0;
                foreach (string code in lines)
                {
                    string insertQuery = "INSERT INTO CustomerCodes (CustomerCode) VALUES (@code)";
                    Dictionary<string, object> parameters = new Dictionary<string, object> { { "@code", code } };
                    var result = await SQLiteCrud.InsertUpdateDeleteAsync(insertQuery, parameters);
                    if (result.Success)
                        eklenen++;
                    else
                        await TextLog.LogToSQLiteAsync($"CustomerCode eklenemedi ({code}): {result.ErrorMessage}");
                }
                XtraMessageBox.Show($"✅ {eklenen} kod kaydedildi.", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
               await LoadCodesAsync();
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync("CustomerNotCodeForm btn_Save_Click hatası: " + ex.Message);
                XtraMessageBox.Show("Kaydetme sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btn_Clear_Click(object sender, EventArgs e)
        {
            DialogResult onay = XtraMessageBox.Show(
                "Tüm müşteri kodları silinecek. Emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (onay != DialogResult.Yes) return;
            var result = await SQLiteCrud.InsertUpdateDeleteAsync("DELETE FROM CustomerCodes");
            if (result.Success)
            {
                rch_CustomerCodes.Text = "";
                XtraMessageBox.Show("Tüm kodlar silindi.", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show("Silme işlemi başarısız:\n" + result.ErrorMessage, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CustomerNotCodeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}