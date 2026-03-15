namespace SmartSheetProject.Forms
{
    partial class CustomerNotCodeForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomerNotCodeForm));
            this.rch_CustomerCodes = new System.Windows.Forms.RichTextBox();
            this.btn_Clear = new DevExpress.XtraEditors.SimpleButton();
            this.btn_Save = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // rch_CustomerCodes
            // 
            this.rch_CustomerCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rch_CustomerCodes.Font = new System.Drawing.Font("Tahoma", 9F);
            this.rch_CustomerCodes.Location = new System.Drawing.Point(0, 0);
            this.rch_CustomerCodes.Name = "rch_CustomerCodes";
            this.rch_CustomerCodes.Size = new System.Drawing.Size(809, 379);
            this.rch_CustomerCodes.TabIndex = 0;
            this.rch_CustomerCodes.Text = "";
            // 
            // btn_Clear
            // 
            this.btn_Clear.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Warning;
            this.btn_Clear.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btn_Clear.Appearance.Options.UseBackColor = true;
            this.btn_Clear.Appearance.Options.UseFont = true;
            this.btn_Clear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Clear.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_Clear.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_Clear.ImageOptions.Image")));
            this.btn_Clear.Location = new System.Drawing.Point(0, 409);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(809, 30);
            this.btn_Clear.TabIndex = 3;
            this.btn_Clear.Text = "Tümünü Temizle";
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Success;
            this.btn_Save.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btn_Save.Appearance.Options.UseBackColor = true;
            this.btn_Save.Appearance.Options.UseFont = true;
            this.btn_Save.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Save.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_Save.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_Save.ImageOptions.Image")));
            this.btn_Save.Location = new System.Drawing.Point(0, 379);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(809, 30);
            this.btn_Save.TabIndex = 1;
            this.btn_Save.Text = "Kaydet";
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // CustomerNotCodeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 439);
            this.Controls.Add(this.rch_CustomerCodes);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.btn_Clear);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("CustomerNotCodeForm.IconOptions.Image")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "CustomerNotCodeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Müşteri Listesi";
            this.Load += new System.EventHandler(this.CustomerNotCodeForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CustomerNotCodeForm_KeyDown);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.RichTextBox rch_CustomerCodes;
        private DevExpress.XtraEditors.SimpleButton btn_Clear;
        private DevExpress.XtraEditors.SimpleButton btn_Save;
    }
}