using System;
using System.Reflection;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.LookAndFeel;
using SmartSheetProject.Classes;
using System.Windows.Forms;

namespace SmartSheetProject.Forms
{
    public partial class HomeForm : FluentDesignForm
    {
        public HomeForm()
        {
            InitializeComponent();
            // Tema değişikliğini dinle
            UserLookAndFeel.Default.StyleChanged += UserLookAndFeel_StyleChanged;
        }
        private async void HomeForm_Load(object sender, EventArgs e)
        {
            // Versiyon bilgisi
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            barStaticItemVersiyon.Caption = $"v{version.Major}.{version.Minor}.{version.Build}";
            barStaticItemTarih.Caption = DateTime.Now.ToString("dd MMMM yyyy HH:mm");
            // Kullanıcının kayıtlı temasını yükle
            await ThemeManager.LoadUserThemeAsync();
        }
        /// <summary>
        /// Tema değiştiğinde otomatik kaydet
        /// </summary>
        private async void UserLookAndFeel_StyleChanged(object sender, EventArgs e)
        {
            try
            {
                string currentTheme = UserLookAndFeel.Default.ActiveSkinName;
                await ThemeManager.SaveUserThemeAsync(currentTheme);
            }
            catch (Exception ex)
            {
                await TextLog.LogToSQLiteAsync($"Tema kaydetme hatası: {ex.Message}");
            }
        }
        private void accordionControlElementBaglan_Click(object sender, EventArgs e)
        {
            GiderForm form = new GiderForm();
            form.ShowDialog();
        }
        private void accordionControlElementSheetler_Click(object sender, EventArgs e)
        {
            GelirForm form = new GelirForm();
            form.ShowDialog();
        }
        private void accordionControlElementVeriCek_Click(object sender, EventArgs e)
        {
            BulutERPInvoice cs = new BulutERPInvoice();
            cs.ShowDialog();
        }
        private void accordionControlElementVeriGonder_Click(object sender, EventArgs e)
        {
            XtraMessageBox.Show("Veriler gönderiliyor...", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void accordionControlElementSmartsheetAyarlari_Click(object sender, EventArgs e)
        {
            SmartsheetSettingsForm form = new SmartsheetSettingsForm();
            form.ShowDialog();
        }
        private void accordionControlElementBulutERPAyarlari_Click(object sender, EventArgs e)
        {
            BulutERPSettingsForm form = new BulutERPSettingsForm();
            form.ShowDialog();
        }
        private void accordionControlElementTemaAyarlari_Click(object sender, EventArgs e)
        {
            // Popup menüyü göster (DevExpress tema seçici)
            popupMenu2.ShowPopup(Cursor.Position);
        }
        private void accordionControlElementHakkimizda_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog();
        }
        private void accordionControlElementError_Click(object sender, EventArgs e)
        {
            ErrorListForm form = new ErrorListForm();
            form.ShowDialog();
        }
        private void HomeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void sqliteAccordionElement_Click(object sender, EventArgs e)
        {
            SQLiteForm form = new SQLiteForm();
            form.ShowDialog();
        }
    }
}