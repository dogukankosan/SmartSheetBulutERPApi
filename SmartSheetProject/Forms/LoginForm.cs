using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class LoginForm : XtraForm
    {
        // Form sürükleme için değişkenler
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        public LoginForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            AttachDragEvents();
        }
        private void AttachDragEvents()
        {
            // Sol panel için
            panelControl1.MouseDown += Form_MouseDown;
            panelControl1.MouseMove += Form_MouseMove;
            panelControl1.MouseUp += Form_MouseUp;
            // Sağ panel için
            panelControl2.MouseDown += Form_MouseDown;
            panelControl2.MouseMove += Form_MouseMove;
            panelControl2.MouseUp += Form_MouseUp;
            // Üst panel için
            if (panelTop != null)
            {
                panelTop.MouseDown += Form_MouseDown;
                panelTop.MouseMove += Form_MouseMove;
                panelTop.MouseUp += Form_MouseUp;
            }
            // Label'lar için (başlık, açıklama)
            foreach (Control ctrl in panelControl1.Controls)
            {
                if (ctrl is LabelControl)
                {
                    ctrl.MouseDown += Form_MouseDown;
                    ctrl.MouseMove += Form_MouseMove;
                    ctrl.MouseUp += Form_MouseUp;
                }
            }
        }
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private async void LoginForm_Load(object sender, EventArgs e)
        {
          
            // Lisans kontrolü yap
            bool isLicenseValid = await CheckLicenseAsync();
            if (!isLicenseValid)
            {
                // Lisans geçersiz - Uygulama kapansın
                Application.Exit();
                return;
            }
            txtKullaniciAdi.Focus();
        }
        /// <summary>
        /// Lisans kontrolü yap
        /// </summary>
        private async Task<bool> CheckLicenseAsync()
        {
            try
            {
                // Kayıtlı lisans var mı kontrol et
                LicenseInfo savedLicense = await LicenseManager.GetSavedLicenseAsync();
                if (savedLicense == null)
                {
                    // İlk kullanım - Lisans girişi iste
                    return await ShowLicenseInputFormAsync();
                }
                // Online doğrulama yap
                ApiResponse apiResponse = await LicenseApiClient.ValidateLicenseAsync(savedLicense.LicenseKey);
                if (apiResponse.Success)
                {
                    // Lisans geçerli - Bilgileri güncelle
                    await LicenseManager.SaveLicenseAsync(
                        savedLicense.LicenseKey,
                        apiResponse.CompanyName,
                        apiResponse.ExpiryDate);
                    await TextLog.LogToSQLiteAsync($"✅ Lisans doğrulandı: {savedLicense.LicenseKey}");
                    return true;
                }
                else
                {
                    // Lisans geçersiz - Kullanıcıya bildir ve yeni lisans iste
                    XtraMessageBox.Show(
                        $"Lisans geçersiz!\n\n{apiResponse.Message}\n\nLütfen yeni bir lisans anahtarı giriniz.",
                        "Lisans Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    // Eski lisansı sil
                    await LicenseManager.DeleteLicenseAsync();
                    // Yeni lisans iste
                    return await ShowLicenseInputFormAsync();
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Lisans kontrol hatası: {ex.Message}");
                // Offline mode - Kayıtlı lisans varsa devam et
                LicenseInfo savedLicense = await LicenseManager.GetSavedLicenseAsync();
                if (savedLicense != null && savedLicense.IsActive)
                {
                    XtraMessageBox.Show(
                        "Sunucuya bağlanılamadı. Offline modda devam ediliyor.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return true;
                }
                XtraMessageBox.Show(
                    $"Lisans kontrolü yapılamadı ve kayıtlı lisans bulunamadı!\n\nHata: {ex.Message}",
                    "Kritik Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }
        /// <summary>
        /// Lisans girişi formu göster
        /// </summary>
        private async Task<bool> ShowLicenseInputFormAsync()
        {
            using (LicenseInputForm licenseForm = new LicenseInputForm())
            {
                if (licenseForm.ShowDialog() == DialogResult.OK)
                {
                    string licenseKey = licenseForm.LicenseKey;
                    string companyName = licenseForm.CompanyName;
                    // Aktivasyon yap
                    ApiResponse apiResponse = await LicenseApiClient.ActivateLicenseAsync(licenseKey, companyName);
                    if (apiResponse.Success)
                    {
                        // Lisansı kaydet
                        await LicenseManager.SaveLicenseAsync(licenseKey, companyName, apiResponse.ExpiryDate);
                        XtraMessageBox.Show(
                            $"Lisans başarıyla aktive edildi!\n\nŞirket: {companyName}\nGeçerlilik: {apiResponse.ExpiryDate?.ToString("dd.MM.yyyy")}",
                            "Başarılı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        await TextLog.LogToSQLiteAsync($"✅ Lisans aktive edildi: {licenseKey}");
                        return true;
                    }
                    else
                    {
                        XtraMessageBox.Show(
                            $"Lisans aktivasyonu başarısız!\n\n{apiResponse.Message}",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        await TextLog.LogToSQLiteAsync($"❌ Lisans aktivasyon hatası: {apiResponse.Message}");
                        return false;
                    }
                }
                // Kullanıcı iptal etti
                return false;
            }
        }
        private void btnGiris_Click(object sender, EventArgs e)
        {
            GirisYap();
        }
        private async void GirisYap()
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text.Trim();
            if (string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                XtraMessageBox.Show("Lütfen kullanıcı adı giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKullaniciAdi.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(sifre))
            {
                XtraMessageBox.Show("Lütfen şifre giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSifre.Focus();
                return;
            }
            // Kullanıcı adını şifrele
            string encryptedUsername = EncryptionHelper.Encrypt(kullaniciAdi);
            try
            {
                // Butonu devre dışı bırak
                btnGiris.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                // Veritabanından kullanıcıyı kontrol et
                string query = "SELECT Password FROM Users WHERE Username = @username LIMIT 1";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@username", encryptedUsername }
                };
                DataTable dt = await SQLiteCrud.GetDataFromSQLiteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                {
                    // Kullanıcı bulunamadı
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSifre.Text = "";
                    txtSifre.Focus();
                    return;
                }
                // Şifreyi çöz ve karşılaştır
                string encryptedPassword = dt.Rows[0]["Password"].ToString();
                string decryptedPassword = EncryptionHelper.Decrypt(encryptedPassword);
                if (decryptedPassword == sifre)
                {
                    // Giriş başarılı - HomeForm aç
                    this.Hide();
                    HomeForm homeForm = new HomeForm();
                    homeForm.ShowDialog();
                    // HomeForm kapandığında LoginForm'u da kapat
                    this.Close();
                }
                else
                {
                    // Şifre hatalı
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSifre.Text = "";
                    txtSifre.Focus();
                }
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"❌ Login hatası: {ex.Message}");
                XtraMessageBox.Show($"Giriş sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGiris.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
        private void hyperlinkLabelControl1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://mutluyazilim.com.tr/",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore
            }
        }
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                GirisYap();
            else if (e.KeyCode == Keys.Escape)
                Application.Exit();
        }
        private void btn_Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btn_Hide_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}