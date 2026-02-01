using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SmartSheetProject.Forms
{
    public partial class AboutForm : XtraForm
    {
        public AboutForm()
        {
            InitializeComponent();
        }
        private void AboutForm_Load(object sender, EventArgs e)
        {
            LoadAboutInfo();
        }
        private void LoadAboutInfo()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                // Company
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                string company = attributes.Length > 0 ? ((AssemblyCompanyAttribute)attributes[0]).Company : "Mutlu Yazılım";
                // Copyright
                attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                string copyright = attributes.Length > 0 ? ((AssemblyCopyrightAttribute)attributes[0]).Copyright : "© 2025 Mutlu Yazılım";
                // Description
                attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                string description = "Smartsheet ve Logo Bulut ERP entegrasyon sistemi. Veri senkronizasyonu ve otomatik iş akışları.";
                // Bilgileri form kontrollerine yükle
                labelCompany.Text = company;
                labelVersion.Text = $"Versiyon {version.Major}.{version.Minor}.{version.Build}";
                labelDescription.Text = description;
                labelCopyright.Text = copyright;
                labelWebsite.Text = "www.mutluyazilim.com.tr";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Bilgiler yüklenirken hata oluştu: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void labelWebsite_Click(object sender, EventArgs e)
        {
            try
            {
               Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.mutluyazilim.com.tr",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Web sitesi açılamadı: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnKapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}