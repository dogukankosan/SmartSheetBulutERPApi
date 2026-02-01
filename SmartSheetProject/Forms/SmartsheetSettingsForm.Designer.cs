namespace SmartSheetProject.Forms
{
    partial class SmartsheetSettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private DevExpress.XtraEditors.LabelControl labelControlBaslik;
        private DevExpress.XtraEditors.LabelControl labelControlToken;
        private DevExpress.XtraEditors.TextEdit textEditApiToken;
        private DevExpress.XtraEditors.SimpleButton btnTest;
        private DevExpress.XtraEditors.SimpleButton btnIptal;
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private DevExpress.XtraEditors.LabelControl labelControlAciklama;
        private DevExpress.XtraEditors.LabelControl labelControlDurum;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmartsheetSettingsForm));
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.labelControlDurum = new DevExpress.XtraEditors.LabelControl();
            this.btnIptal = new DevExpress.XtraEditors.SimpleButton();
            this.btnTest = new DevExpress.XtraEditors.SimpleButton();
            this.textEditApiToken = new DevExpress.XtraEditors.TextEdit();
            this.labelControlAciklama = new DevExpress.XtraEditors.LabelControl();
            this.labelControlToken = new DevExpress.XtraEditors.LabelControl();
            this.labelControlBaslik = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            this.panelControlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditApiToken.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.labelControlDurum);
            this.panelControlMain.Controls.Add(this.btnIptal);
            this.panelControlMain.Controls.Add(this.btnTest);
            this.panelControlMain.Controls.Add(this.textEditApiToken);
            this.panelControlMain.Controls.Add(this.labelControlAciklama);
            this.panelControlMain.Controls.Add(this.labelControlToken);
            this.panelControlMain.Controls.Add(this.labelControlBaslik);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 0);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(600, 350);
            this.panelControlMain.TabIndex = 0;
            // 
            // labelControlDurum
            // 
            this.labelControlDurum.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlDurum.Appearance.ForeColor = System.Drawing.Color.Green;
            this.labelControlDurum.Appearance.Options.UseFont = true;
            this.labelControlDurum.Appearance.Options.UseForeColor = true;
            this.labelControlDurum.Location = new System.Drawing.Point(40, 225);
            this.labelControlDurum.Name = "labelControlDurum";
            this.labelControlDurum.Size = new System.Drawing.Size(0, 15);
            this.labelControlDurum.TabIndex = 7;
            // 
            // btnIptal
            // 
            this.btnIptal.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Danger;
            this.btnIptal.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnIptal.Appearance.Options.UseBackColor = true;
            this.btnIptal.Appearance.Options.UseFont = true;
            this.btnIptal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIptal.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnIptal.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnIptal.Location = new System.Drawing.Point(304, 271);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(180, 36);
            this.btnIptal.TabIndex = 2;
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
            this.btnTest.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnTest.Location = new System.Drawing.Point(84, 271);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(180, 36);
            this.btnTest.TabIndex = 1;
            this.btnTest.Text = "Bağlantıyı Test Et";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // textEditApiToken
            // 
            this.textEditApiToken.Location = new System.Drawing.Point(40, 180);
            this.textEditApiToken.Name = "textEditApiToken";
            this.textEditApiToken.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textEditApiToken.Properties.Appearance.Options.UseFont = true;
            this.textEditApiToken.Properties.PasswordChar = '●';
            this.textEditApiToken.Size = new System.Drawing.Size(520, 24);
            this.textEditApiToken.TabIndex = 0;
            // 
            // labelControlAciklama
            // 
            this.labelControlAciklama.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlAciklama.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.labelControlAciklama.Appearance.Options.UseFont = true;
            this.labelControlAciklama.Appearance.Options.UseForeColor = true;
            this.labelControlAciklama.Location = new System.Drawing.Point(40, 90);
            this.labelControlAciklama.Name = "labelControlAciklama";
            this.labelControlAciklama.Size = new System.Drawing.Size(403, 45);
            this.labelControlAciklama.TabIndex = 2;
            this.labelControlAciklama.Text = resources.GetString("labelControlAciklama.Text");
            // 
            // labelControlToken
            // 
            this.labelControlToken.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlToken.Appearance.Options.UseFont = true;
            this.labelControlToken.Location = new System.Drawing.Point(40, 150);
            this.labelControlToken.Name = "labelControlToken";
            this.labelControlToken.Size = new System.Drawing.Size(159, 17);
            this.labelControlToken.TabIndex = 1;
            this.labelControlToken.Text = "Smartsheet Access Token:";
            // 
            // labelControlBaslik
            // 
            this.labelControlBaslik.Appearance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelControlBaslik.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.labelControlBaslik.Appearance.Options.UseFont = true;
            this.labelControlBaslik.Appearance.Options.UseForeColor = true;
            this.labelControlBaslik.Location = new System.Drawing.Point(40, 40);
            this.labelControlBaslik.Name = "labelControlBaslik";
            this.labelControlBaslik.Size = new System.Drawing.Size(224, 32);
            this.labelControlBaslik.TabIndex = 0;
            this.labelControlBaslik.Text = "Smartsheet Ayarları";
            // 
            // SmartsheetSettingsForm
            // 
            this.AcceptButton = this.btnTest;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnIptal;
            this.ClientSize = new System.Drawing.Size(600, 350);
            this.Controls.Add(this.panelControlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("SmartsheetSettingsForm.IconOptions.Image")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmartsheetSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smartsheet Ayarları";
            this.Load += new System.EventHandler(this.SmartsheetSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            this.panelControlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditApiToken.Properties)).EndInit();
            this.ResumeLayout(false);

        }
    }
}