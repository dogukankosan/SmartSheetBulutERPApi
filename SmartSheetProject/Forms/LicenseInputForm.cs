using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SmartSheetProject.Classes;

namespace SmartSheetProject.Forms
{
    public partial class LicenseInputForm : XtraForm
    {
        public string LicenseKey { get; private set; }
        public string CompanyName { get; private set; }
        public LicenseInputForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }
        private void LicenseInputForm_Load(object sender, EventArgs e)
        {
            // Hardware ID'yi göster
            txtHardwareId.Text = LicenseManager.GetHardwareId();
            txtLicenseKey.Focus();
        }
        private void btnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();
            string companyName = txtCompanyName.Text.Trim();
            // Validasyon
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                XtraMessageBox.Show(
                    "Lütfen lisans anahtarını giriniz!",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtLicenseKey.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(companyName))
            {
                XtraMessageBox.Show(
                    "Lütfen şirket adını giriniz!",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return;
            }
            // Değerleri ayarla
            LicenseKey = licenseKey;
            CompanyName = companyName;
            // Form'u kapat (DialogResult.OK)
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void btnCopyHardwareId_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtHardwareId.Text);
                XtraMessageBox.Show(
                    "Hardware ID kopyalandı!\n\nBu ID'yi Mutlu Yazılım'a ileterek lisans anahtarı alabilirsiniz.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    $"Kopyalama hatası: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                btnCancel_Click(null, null);
                return true;
            }
            else if (keyData == Keys.Enter)
            {
                btnActivate_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}