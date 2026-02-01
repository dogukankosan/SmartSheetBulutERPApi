using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class BulutERPSettingsForm : XtraForm
    {
        public BulutERPSettingsForm()
        {
            InitializeComponent();
        }
        private async void BulutERPSettingsForm_Load(object sender, EventArgs e)
        {
            await LoadExistingSettingsAsync();
        }
        /// <summary>
        /// Mevcut ayarları yükle
        /// </summary>
        private async Task LoadExistingSettingsAsync()
        {
            try
            {
                var result = await BulutERPConnectionTest.GetSettingsAsync();
                if (result.Success && result.Settings != null)
                {
                    // Ayarları forma yükle
                    textEditClientId.Text = result.Settings.ClientId;
                    textEditClientSecret.Text = result.Settings.ClientSecret;
                    textEditUsername.Text = result.Settings.Username;
                    textEditPassword.Text = result.Settings.Password;
                    textEditFirmNr.Text = result.Settings.FirmNr;
                    textEditServerUrl.Text = result.Settings.ServerUrl;
                    txt_MachineID.Text = result.Settings.MachineID;
                    labelControlDurum.Text = "✓ Ayarlar kayıtlı - Değişiklik yapmak için test edin";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    labelControlDurum.Text = "⚠ Ayar bulunamadı - Yeni ayar giriniz";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Orange;
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"BulutERPSettings Form Load hatası: {ex.Message}");
                XtraMessageBox.Show($"Ayarlar yüklenirken hata oluştu:\n{ex.Message}", "Hata",
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
                btnTest.Enabled = false;
                btnTest.Text = "Test ediliyor...";
                // Alanları al
                string clientId = textEditClientId.Text.Trim();
                string clientSecret = textEditClientSecret.Text.Trim();
                string username = textEditUsername.Text.Trim();
                string password = textEditPassword.Text.Trim();
                string firmNr = textEditFirmNr.Text.Trim();
                string serverUrl = textEditServerUrl.Text.Trim();
                string machineID = txt_MachineID.Text.Trim();
                // 1. VALİDASYON
                var validation = BulutERPValidator.ValidateSettings(
                    clientId, clientSecret, username, password, firmNr, serverUrl, machineID);
                if (!validation.IsValid)
                {
                    btnTest.Enabled = true;
                    btnTest.Text = "Bağlantıyı Test Et";
                    labelControlDurum.Text = "✗ Validasyon hatası";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Red;
                    XtraMessageBox.Show(validation.ErrorMessage, "Validasyon Hatası",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
               }
                // 2. GEÇİCİ KAYDET (Test için)
                btnTest.Text = "Kaydediliyor...";
                var saveResult = await BulutERPConnectionTest.SaveSettingsAsync(
                    clientId, clientSecret, username, password, firmNr, serverUrl, machineID);
                if (!saveResult.Success)
                {
                    btnTest.Enabled = true;
                    btnTest.Text = "Bağlantıyı Test Et";
                    labelControlDurum.Text = "✗ Kaydetme hatası";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Red;
                    XtraMessageBox.Show($"Ayarlar kaydedilemedi:\n{saveResult.ErrorMessage}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // 3. BAĞLANTIYI TEST ET
                btnTest.Text = "Bağlantı test ediliyor...";
               var testResult = await BulutERPConnectionTest.TestConnectionAsync();
                btnTest.Enabled = true;
                btnTest.Text = "Bağlantıyı Test Et";
                if (testResult.Success)
                {
                    // ✅ TEST BAŞARILI - Ayarlar zaten kaydedildi (token dahil)
                    labelControlDurum.Text = "✓ Bağlantı başarılı - Ayarlar kaydedildi";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
                    XtraMessageBox.Show(
                        "✓ Bağlantı testi başarılı!\n✓ Token alındı ve kaydedildi!\n✓ Test sorgusu çalıştı!\n✓ Ayarlar şifreli olarak kaydedildi!",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    // Form'u kapat veya aç bırak (opsiyonel)
                    // this.Close();
                }
                else
                {
                    // ❌ TEST BAŞARISIZ - Ayarlar zaten silindi (TestConnection içinde)
                    labelControlDurum.Text = "✗ Bağlantı başarısız";
                    labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Red;
                    XtraMessageBox.Show(
                        $"Bağlantı testi başarısız!\n\n{testResult.ErrorMessage}\n\nAyarlar kaydedilmedi. Lütfen bilgileri kontrol edip tekrar test edin.",
                        "Bağlantı Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                btnTest.Enabled = true;
                btnTest.Text = "Bağlantıyı Test Et";
                await TextLog.LogToSQLiteAsync($"BulutERPSettings Test hatası: {ex.Message}");
                XtraMessageBox.Show($"Test hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Form alanları değiştiğinde uyarı göster
        /// </summary>
        private void OnTextChanged(object sender, EventArgs e)
        {
            labelControlDurum.Text = "⚠ Değişiklikler test edilmedi";
            labelControlDurum.Appearance.ForeColor = Color.Orange;
        }
    }
}