using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class SmartsheetSettingsForm : XtraForm
    {
        public SmartsheetSettingsForm()
        {
            InitializeComponent();
        }
        private async void SmartsheetSettingsForm_Load(object sender, EventArgs e)
        {
            await LoadExistingTokenAsync();
        }
        private async Task LoadExistingTokenAsync()
        {
            try
            {
                var result = await SmartsheetService.GetApiTokenAsync();
                if (result.Success && !string.IsNullOrWhiteSpace(result.Token))
                {
                    // Token varsa tam halini göster (düzenleme için)
                    textEditApiToken.Text = result.Token;
                    labelControlDurum.Text = "✓ API Token kayıtlı";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    labelControlDurum.Text = "⚠ API Token kayıtlı değil";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Orange;
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"SmartsheetSettings Token yükleme hatası: {ex.Message}");
                XtraMessageBox.Show($"Token yükleme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Test butonu - Test yap, başarılıysa otomatik kaydet
        /// </summary>
        private async void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                string token = textEditApiToken.Text.Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    XtraMessageBox.Show("Lütfen API Token giriniz!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textEditApiToken.Focus();
                    return;
                }
                btnTest.Enabled = false;
                btnTest.Text = "Test ediliyor...";
                // Bağlantıyı test et
                var testResult = await TestConnectionWithTokenAsync(token);
                btnTest.Enabled = true;
                btnTest.Text = "Bağlantıyı Test Et";
                if (testResult.Success)
                {
                    // ✅ TEST BAŞARILI - Şimdi kaydet
                    var saveResult = await SmartsheetService.SaveApiTokenAsync(token);
                    if (saveResult.Success)
                    {
                        labelControlDurum.Text = "✓ Bağlantı başarılı - Token kaydedildi";
                        labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
                        XtraMessageBox.Show(
                            "✓ Bağlantı testi başarılı!\n✓ API Token şifreli olarak kaydedildi!",
                            "Başarılı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        labelControlDurum.Text = "✓ Bağlantı başarılı ama kaydetme hatası";
                        labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Orange;
                        XtraMessageBox.Show(
                            $"Bağlantı başarılı ama token kaydedilemedi:\n{saveResult.ErrorMessage}",
                            "Uyarı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // ❌ TEST BAŞARISIZ
                    labelControlDurum.Text = "✗ Bağlantı başarısız";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Red;
                    XtraMessageBox.Show(
                        $"Bağlantı testi başarısız!\n\n{testResult.Message}\n\nToken kaydedilmedi. Lütfen token'ı kontrol edin.",
                        "Bağlantı Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                btnTest.Enabled = true;
                btnTest.Text = "Bağlantıyı Test Et";
                await Classes.TextLog.LogToSQLiteAsync($"SmartsheetSettings btnTest hatası: {ex.Message}");
                XtraMessageBox.Show($"Test hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Verilen token ile bağlantı testi yapar (kaydetmeden)
        /// </summary>
        private async Task<(bool Success, string Message)> TestConnectionWithTokenAsync(string token)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.smartsheet.com/2.0/users/me");
                    if (response.IsSuccessStatusCode)
                        return (true, "Bağlantı başarılı!");
                    else
                    {
                        string error = $"{response.StatusCode} - {response.ReasonPhrase}";
                        await TextLog.LogToSQLiteAsync($"SmartsheetSettings Bağlantı testi başarısız: {error}");
                        return (false, error);
                    }
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"SmartsheetSettings TestConnectionWithToken hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}