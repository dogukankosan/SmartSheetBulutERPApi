namespace SmartSheetProject.Forms
{
    partial class BulutERPSettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private DevExpress.XtraEditors.LabelControl labelControlBaslik;
        private DevExpress.XtraEditors.LabelControl labelControlClientId;
        private DevExpress.XtraEditors.TextEdit textEditClientId;
        private DevExpress.XtraEditors.LabelControl labelControlClientSecret;
        private DevExpress.XtraEditors.TextEdit textEditClientSecret;
        private DevExpress.XtraEditors.LabelControl labelControlUsername;
        private DevExpress.XtraEditors.TextEdit textEditUsername;
        private DevExpress.XtraEditors.LabelControl labelControlPassword;
        private DevExpress.XtraEditors.TextEdit textEditPassword;
        private DevExpress.XtraEditors.LabelControl labelControlFirmNr;
        private DevExpress.XtraEditors.TextEdit textEditFirmNr;
        private DevExpress.XtraEditors.LabelControl labelControlServerUrl;
        private DevExpress.XtraEditors.TextEdit textEditServerUrl;
        private DevExpress.XtraEditors.SimpleButton btnTest;
        private DevExpress.XtraEditors.SimpleButton btnIptal;
        private DevExpress.XtraEditors.LabelControl labelControlDurum;
        private DevExpress.XtraEditors.LabelControl labelControlAciklama;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulutERPSettingsForm));
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.txt_MachineID = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControlDurum = new DevExpress.XtraEditors.LabelControl();
            this.btnIptal = new DevExpress.XtraEditors.SimpleButton();
            this.btnTest = new DevExpress.XtraEditors.SimpleButton();
            this.textEditServerUrl = new DevExpress.XtraEditors.TextEdit();
            this.labelControlServerUrl = new DevExpress.XtraEditors.LabelControl();
            this.textEditFirmNr = new DevExpress.XtraEditors.TextEdit();
            this.labelControlFirmNr = new DevExpress.XtraEditors.LabelControl();
            this.textEditPassword = new DevExpress.XtraEditors.TextEdit();
            this.labelControlPassword = new DevExpress.XtraEditors.LabelControl();
            this.textEditUsername = new DevExpress.XtraEditors.TextEdit();
            this.labelControlUsername = new DevExpress.XtraEditors.LabelControl();
            this.textEditClientSecret = new DevExpress.XtraEditors.TextEdit();
            this.labelControlClientSecret = new DevExpress.XtraEditors.LabelControl();
            this.textEditClientId = new DevExpress.XtraEditors.TextEdit();
            this.labelControlClientId = new DevExpress.XtraEditors.LabelControl();
            this.labelControlAciklama = new DevExpress.XtraEditors.LabelControl();
            this.labelControlBaslik = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            this.panelControlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_MachineID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditServerUrl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFirmNr.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClientSecret.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClientId.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.txt_MachineID);
            this.panelControlMain.Controls.Add(this.labelControl1);
            this.panelControlMain.Controls.Add(this.labelControlDurum);
            this.panelControlMain.Controls.Add(this.btnIptal);
            this.panelControlMain.Controls.Add(this.btnTest);
            this.panelControlMain.Controls.Add(this.textEditServerUrl);
            this.panelControlMain.Controls.Add(this.labelControlServerUrl);
            this.panelControlMain.Controls.Add(this.textEditFirmNr);
            this.panelControlMain.Controls.Add(this.labelControlFirmNr);
            this.panelControlMain.Controls.Add(this.textEditPassword);
            this.panelControlMain.Controls.Add(this.labelControlPassword);
            this.panelControlMain.Controls.Add(this.textEditUsername);
            this.panelControlMain.Controls.Add(this.labelControlUsername);
            this.panelControlMain.Controls.Add(this.textEditClientSecret);
            this.panelControlMain.Controls.Add(this.labelControlClientSecret);
            this.panelControlMain.Controls.Add(this.textEditClientId);
            this.panelControlMain.Controls.Add(this.labelControlClientId);
            this.panelControlMain.Controls.Add(this.labelControlAciklama);
            this.panelControlMain.Controls.Add(this.labelControlBaslik);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 0);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(700, 600);
            this.panelControlMain.TabIndex = 0;
            // 
            // txt_MachineID
            // 
            this.txt_MachineID.Location = new System.Drawing.Point(40, 493);
            this.txt_MachineID.Name = "txt_MachineID";
            this.txt_MachineID.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_MachineID.Properties.Appearance.Options.UseFont = true;
            this.txt_MachineID.Properties.MaxLength = 10;
            this.txt_MachineID.Properties.UseSystemPasswordChar = true;
            this.txt_MachineID.Size = new System.Drawing.Size(620, 24);
            this.txt_MachineID.TabIndex = 6;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(40, 468);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(67, 17);
            this.labelControl1.TabIndex = 19;
            this.labelControl1.Text = "Makine ID:";
            // 
            // labelControlDurum
            // 
            this.labelControlDurum.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
            this.labelControlDurum.Appearance.Options.UseFont = true;
            this.labelControlDurum.Appearance.Options.UseForeColor = true;
            this.labelControlDurum.Location = new System.Drawing.Point(40, 530);
            this.labelControlDurum.Name = "labelControlDurum";
            this.labelControlDurum.Size = new System.Drawing.Size(0, 15);
            this.labelControlDurum.TabIndex = 17;
            // 
            // btnIptal
            // 
            this.btnIptal.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Danger;
            this.btnIptal.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnIptal.Appearance.Options.UseBackColor = true;
            this.btnIptal.Appearance.Options.UseFont = true;
            this.btnIptal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIptal.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnIptal.Location = new System.Drawing.Point(374, 552);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(130, 36);
            this.btnIptal.TabIndex = 8;
            this.btnIptal.Text = "İptal";
            this.btnIptal.Click += new System.EventHandler(this.btnIptal_Click);
            // 
            // btnTest
            // 
            this.btnTest.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Success;
            this.btnTest.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnTest.Appearance.Options.UseBackColor = true;
            this.btnTest.Appearance.Options.UseFont = true;
            this.btnTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTest.Location = new System.Drawing.Point(129, 552);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(220, 36);
            this.btnTest.TabIndex = 7;
            this.btnTest.Text = "Bağlantıyı Test Et";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // textEditServerUrl
            // 
            this.textEditServerUrl.Location = new System.Drawing.Point(40, 426);
            this.textEditServerUrl.Name = "textEditServerUrl";
            this.textEditServerUrl.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditServerUrl.Properties.Appearance.Options.UseFont = true;
            this.textEditServerUrl.Properties.UseSystemPasswordChar = true;
            this.textEditServerUrl.Size = new System.Drawing.Size(620, 24);
            this.textEditServerUrl.TabIndex = 5;
            // 
            // labelControlServerUrl
            // 
            this.labelControlServerUrl.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlServerUrl.Appearance.Options.UseFont = true;
            this.labelControlServerUrl.Location = new System.Drawing.Point(40, 400);
            this.labelControlServerUrl.Name = "labelControlServerUrl";
            this.labelControlServerUrl.Size = new System.Drawing.Size(92, 17);
            this.labelControlServerUrl.TabIndex = 12;
            this.labelControlServerUrl.Text = "Sunucu Adresi:";
            // 
            // textEditFirmNr
            // 
            this.textEditFirmNr.Location = new System.Drawing.Point(40, 360);
            this.textEditFirmNr.Name = "textEditFirmNr";
            this.textEditFirmNr.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditFirmNr.Properties.Appearance.Options.UseFont = true;
            this.textEditFirmNr.Properties.MaxLength = 3;
            this.textEditFirmNr.Properties.UseSystemPasswordChar = true;
            this.textEditFirmNr.Size = new System.Drawing.Size(620, 24);
            this.textEditFirmNr.TabIndex = 4;
            // 
            // labelControlFirmNr
            // 
            this.labelControlFirmNr.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlFirmNr.Appearance.Options.UseFont = true;
            this.labelControlFirmNr.Location = new System.Drawing.Point(40, 335);
            this.labelControlFirmNr.Name = "labelControlFirmNr";
            this.labelControlFirmNr.Size = new System.Drawing.Size(102, 17);
            this.labelControlFirmNr.TabIndex = 10;
            this.labelControlFirmNr.Text = "Firma Numarası:";
            // 
            // textEditPassword
            // 
            this.textEditPassword.Location = new System.Drawing.Point(40, 290);
            this.textEditPassword.Name = "textEditPassword";
            this.textEditPassword.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditPassword.Properties.Appearance.Options.UseFont = true;
            this.textEditPassword.Properties.PasswordChar = '●';
            this.textEditPassword.Size = new System.Drawing.Size(620, 24);
            this.textEditPassword.TabIndex = 3;
            // 
            // labelControlPassword
            // 
            this.labelControlPassword.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlPassword.Appearance.Options.UseFont = true;
            this.labelControlPassword.Location = new System.Drawing.Point(40, 265);
            this.labelControlPassword.Name = "labelControlPassword";
            this.labelControlPassword.Size = new System.Drawing.Size(32, 17);
            this.labelControlPassword.TabIndex = 8;
            this.labelControlPassword.Text = "Şifre:";
            // 
            // textEditUsername
            // 
            this.textEditUsername.Location = new System.Drawing.Point(40, 220);
            this.textEditUsername.Name = "textEditUsername";
            this.textEditUsername.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditUsername.Properties.Appearance.Options.UseFont = true;
            this.textEditUsername.Size = new System.Drawing.Size(620, 24);
            this.textEditUsername.TabIndex = 2;
            // 
            // labelControlUsername
            // 
            this.labelControlUsername.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlUsername.Appearance.Options.UseFont = true;
            this.labelControlUsername.Location = new System.Drawing.Point(40, 195);
            this.labelControlUsername.Name = "labelControlUsername";
            this.labelControlUsername.Size = new System.Drawing.Size(82, 17);
            this.labelControlUsername.TabIndex = 6;
            this.labelControlUsername.Text = "Kullanıcı Adı:";
            // 
            // textEditClientSecret
            // 
            this.textEditClientSecret.Location = new System.Drawing.Point(350, 150);
            this.textEditClientSecret.Name = "textEditClientSecret";
            this.textEditClientSecret.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditClientSecret.Properties.Appearance.Options.UseFont = true;
            this.textEditClientSecret.Properties.PasswordChar = '●';
            this.textEditClientSecret.Size = new System.Drawing.Size(310, 24);
            this.textEditClientSecret.TabIndex = 1;
            // 
            // labelControlClientSecret
            // 
            this.labelControlClientSecret.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlClientSecret.Appearance.Options.UseFont = true;
            this.labelControlClientSecret.Location = new System.Drawing.Point(350, 125);
            this.labelControlClientSecret.Name = "labelControlClientSecret";
            this.labelControlClientSecret.Size = new System.Drawing.Size(81, 17);
            this.labelControlClientSecret.TabIndex = 4;
            this.labelControlClientSecret.Text = "Client Secret:";
            // 
            // textEditClientId
            // 
            this.textEditClientId.Location = new System.Drawing.Point(40, 150);
            this.textEditClientId.Name = "textEditClientId";
            this.textEditClientId.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditClientId.Properties.Appearance.Options.UseFont = true;
            this.textEditClientId.Properties.PasswordChar = '●';
            this.textEditClientId.Size = new System.Drawing.Size(290, 24);
            this.textEditClientId.TabIndex = 0;
            // 
            // labelControlClientId
            // 
            this.labelControlClientId.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlClientId.Appearance.Options.UseFont = true;
            this.labelControlClientId.Location = new System.Drawing.Point(40, 125);
            this.labelControlClientId.Name = "labelControlClientId";
            this.labelControlClientId.Size = new System.Drawing.Size(58, 17);
            this.labelControlClientId.TabIndex = 2;
            this.labelControlClientId.Text = "Client ID:";
            // 
            // labelControlAciklama
            // 
            this.labelControlAciklama.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlAciklama.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.labelControlAciklama.Appearance.Options.UseFont = true;
            this.labelControlAciklama.Appearance.Options.UseForeColor = true;
            this.labelControlAciklama.Location = new System.Drawing.Point(40, 75);
            this.labelControlAciklama.Name = "labelControlAciklama";
            this.labelControlAciklama.Size = new System.Drawing.Size(445, 30);
            this.labelControlAciklama.TabIndex = 1;
            this.labelControlAciklama.Text = "Logo Bulut ERP bağlantı bilgilerinizi girin. Tüm bilgiler şifreli olarak saklanac" +
    "aktır.\r\nKaydettikten sonra test sorgusu çalıştırılacak ve başarılı olursa ayarla" +
    "r kaydedilecektir.";
            // 
            // labelControlBaslik
            // 
            this.labelControlBaslik.Appearance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelControlBaslik.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.labelControlBaslik.Appearance.Options.UseFont = true;
            this.labelControlBaslik.Appearance.Options.UseForeColor = true;
            this.labelControlBaslik.Location = new System.Drawing.Point(40, 30);
            this.labelControlBaslik.Name = "labelControlBaslik";
            this.labelControlBaslik.Size = new System.Drawing.Size(209, 32);
            this.labelControlBaslik.TabIndex = 0;
            this.labelControlBaslik.Text = "Bulut ERP Ayarları";
            // 
            // BulutERPSettingsForm
            // 
            this.AcceptButton = this.btnTest;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnIptal;
            this.ClientSize = new System.Drawing.Size(700, 600);
            this.Controls.Add(this.panelControlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("BulutERPSettingsForm.IconOptions.Image")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BulutERPSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bulut ERP Ayarları";
            this.Load += new System.EventHandler(this.BulutERPSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            this.panelControlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_MachineID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditServerUrl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFirmNr.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClientSecret.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClientId.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        private DevExpress.XtraEditors.TextEdit txt_MachineID;
        private DevExpress.XtraEditors.LabelControl labelControl1;
    }
}