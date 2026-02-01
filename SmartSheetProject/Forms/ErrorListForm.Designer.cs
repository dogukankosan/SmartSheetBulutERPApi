namespace SmartSheetProject.Forms
{
    partial class ErrorListForm
    {
        private System.ComponentModel.IContainer components = null;
        private DevExpress.XtraEditors.PanelControl panelControlTop;
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private DevExpress.XtraEditors.SimpleButton btnYenile;
        private DevExpress.XtraEditors.SimpleButton btnExcel;
        private DevExpress.XtraEditors.SimpleButton btnTemizle;
        private DevExpress.XtraEditors.SimpleButton btnSil;
        private DevExpress.XtraEditors.DateEdit dateEditBaslangic;
        private DevExpress.XtraEditors.DateEdit dateEditBitis;
        private DevExpress.XtraEditors.LabelControl labelControlBaslangic;
        private DevExpress.XtraEditors.LabelControl labelControlBitis;
        private DevExpress.XtraEditors.LabelControl labelControlToplam;
        private DevExpress.XtraGrid.GridControl gridControlErrors;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewErrors;
        private DevExpress.XtraGrid.Columns.GridColumn colId;
        private DevExpress.XtraGrid.Columns.GridColumn colDetails;
        private DevExpress.XtraGrid.Columns.GridColumn colDate;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorListForm));
            this.panelControlTop = new DevExpress.XtraEditors.PanelControl();
            this.labelControlToplam = new DevExpress.XtraEditors.LabelControl();
            this.labelControlBitis = new DevExpress.XtraEditors.LabelControl();
            this.dateEditBitis = new DevExpress.XtraEditors.DateEdit();
            this.labelControlBaslangic = new DevExpress.XtraEditors.LabelControl();
            this.dateEditBaslangic = new DevExpress.XtraEditors.DateEdit();
            this.btnSil = new DevExpress.XtraEditors.SimpleButton();
            this.btnTemizle = new DevExpress.XtraEditors.SimpleButton();
            this.btnExcel = new DevExpress.XtraEditors.SimpleButton();
            this.btnYenile = new DevExpress.XtraEditors.SimpleButton();
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.gridControlErrors = new DevExpress.XtraGrid.GridControl();
            this.gridViewErrors = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDetails = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDate = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).BeginInit();
            this.panelControlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBitis.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBitis.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBaslangic.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBaslangic.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            this.panelControlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlTop
            // 
            this.panelControlTop.Controls.Add(this.labelControlToplam);
            this.panelControlTop.Controls.Add(this.labelControlBitis);
            this.panelControlTop.Controls.Add(this.dateEditBitis);
            this.panelControlTop.Controls.Add(this.labelControlBaslangic);
            this.panelControlTop.Controls.Add(this.dateEditBaslangic);
            this.panelControlTop.Controls.Add(this.btnSil);
            this.panelControlTop.Controls.Add(this.btnTemizle);
            this.panelControlTop.Controls.Add(this.btnExcel);
            this.panelControlTop.Controls.Add(this.btnYenile);
            this.panelControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlTop.Location = new System.Drawing.Point(0, 0);
            this.panelControlTop.Name = "panelControlTop";
            this.panelControlTop.Size = new System.Drawing.Size(972, 120);
            this.panelControlTop.TabIndex = 0;
            // 
            // labelControlToplam
            // 
            this.labelControlToplam.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelControlToplam.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.labelControlToplam.Appearance.Options.UseFont = true;
            this.labelControlToplam.Appearance.Options.UseForeColor = true;
            this.labelControlToplam.Location = new System.Drawing.Point(850, 80);
            this.labelControlToplam.Name = "labelControlToplam";
            this.labelControlToplam.Size = new System.Drawing.Size(96, 17);
            this.labelControlToplam.TabIndex = 9;
            this.labelControlToplam.Text = "Toplam: 0 kayıt";
            // 
            // labelControlBitis
            // 
            this.labelControlBitis.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlBitis.Appearance.Options.UseFont = true;
            this.labelControlBitis.Location = new System.Drawing.Point(430, 82);
            this.labelControlBitis.Name = "labelControlBitis";
            this.labelControlBitis.Size = new System.Drawing.Size(57, 15);
            this.labelControlBitis.TabIndex = 8;
            this.labelControlBitis.Text = "Bitiş Tarihi:";
            // 
            // dateEditBitis
            // 
            this.dateEditBitis.EditValue = null;
            this.dateEditBitis.Location = new System.Drawing.Point(500, 78);
            this.dateEditBitis.Name = "dateEditBitis";
            this.dateEditBitis.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dateEditBitis.Properties.Appearance.Options.UseFont = true;
            this.dateEditBitis.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateEditBitis.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateEditBitis.Size = new System.Drawing.Size(150, 22);
            this.dateEditBitis.TabIndex = 5;
            this.dateEditBitis.EditValueChanged += new System.EventHandler(this.dateEdit_EditValueChanged);
            // 
            // labelControlBaslangic
            // 
            this.labelControlBaslangic.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelControlBaslangic.Appearance.Options.UseFont = true;
            this.labelControlBaslangic.Location = new System.Drawing.Point(180, 82);
            this.labelControlBaslangic.Name = "labelControlBaslangic";
            this.labelControlBaslangic.Size = new System.Drawing.Size(85, 15);
            this.labelControlBaslangic.TabIndex = 6;
            this.labelControlBaslangic.Text = "Başlangıç Tarihi:";
            // 
            // dateEditBaslangic
            // 
            this.dateEditBaslangic.EditValue = null;
            this.dateEditBaslangic.Location = new System.Drawing.Point(270, 78);
            this.dateEditBaslangic.Name = "dateEditBaslangic";
            this.dateEditBaslangic.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dateEditBaslangic.Properties.Appearance.Options.UseFont = true;
            this.dateEditBaslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateEditBaslangic.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateEditBaslangic.Size = new System.Drawing.Size(150, 22);
            this.dateEditBaslangic.TabIndex = 4;
            this.dateEditBaslangic.EditValueChanged += new System.EventHandler(this.dateEdit_EditValueChanged);
            // 
            // btnSil
            // 
            this.btnSil.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Question;
            this.btnSil.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSil.Appearance.Options.UseBackColor = true;
            this.btnSil.Appearance.Options.UseFont = true;
            this.btnSil.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSil.Location = new System.Drawing.Point(650, 30);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(150, 36);
            this.btnSil.TabIndex = 3;
            this.btnSil.Text = "Seçili Kayıtları Sil";
            this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
            // 
            // btnTemizle
            // 
            this.btnTemizle.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Danger;
            this.btnTemizle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnTemizle.Appearance.Options.UseBackColor = true;
            this.btnTemizle.Appearance.Options.UseFont = true;
            this.btnTemizle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTemizle.Location = new System.Drawing.Point(490, 30);
            this.btnTemizle.Name = "btnTemizle";
            this.btnTemizle.Size = new System.Drawing.Size(150, 36);
            this.btnTemizle.TabIndex = 2;
            this.btnTemizle.Text = "Tüm Logları Temizle";
            this.btnTemizle.Click += new System.EventHandler(this.btnTemizle_Click);
            // 
            // btnExcel
            // 
            this.btnExcel.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Warning;
            this.btnExcel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExcel.Appearance.Options.UseBackColor = true;
            this.btnExcel.Appearance.Options.UseFont = true;
            this.btnExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExcel.Location = new System.Drawing.Point(330, 30);
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.Size = new System.Drawing.Size(150, 36);
            this.btnExcel.TabIndex = 1;
            this.btnExcel.Text = "Excel\'e Aktar";
            this.btnExcel.Click += new System.EventHandler(this.btnExcel_Click);
            // 
            // btnYenile
            // 
            this.btnYenile.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Success;
            this.btnYenile.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnYenile.Appearance.Options.UseBackColor = true;
            this.btnYenile.Appearance.Options.UseFont = true;
            this.btnYenile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnYenile.Location = new System.Drawing.Point(170, 30);
            this.btnYenile.Name = "btnYenile";
            this.btnYenile.Size = new System.Drawing.Size(150, 36);
            this.btnYenile.TabIndex = 0;
            this.btnYenile.Text = "Yenile";
            this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControlErrors);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 120);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(972, 315);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControlErrors
            // 
            this.gridControlErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlErrors.Location = new System.Drawing.Point(2, 2);
            this.gridControlErrors.MainView = this.gridViewErrors;
            this.gridControlErrors.Name = "gridControlErrors";
            this.gridControlErrors.Size = new System.Drawing.Size(968, 311);
            this.gridControlErrors.TabIndex = 0;
            this.gridControlErrors.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewErrors});
            // 
            // gridViewErrors
            // 
            this.gridViewErrors.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colId,
            this.colDetails,
            this.colDate});
            this.gridViewErrors.GridControl = this.gridControlErrors;
            this.gridViewErrors.Name = "gridViewErrors";
            this.gridViewErrors.OptionsBehavior.Editable = false;
            this.gridViewErrors.OptionsBehavior.ReadOnly = true;
            this.gridViewErrors.OptionsSelection.MultiSelect = true;
            this.gridViewErrors.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            this.gridViewErrors.OptionsView.ShowGroupPanel = false;
            // 
            // colId
            // 
            this.colId.Caption = "ID";
            this.colId.FieldName = "Id";
            this.colId.Name = "colId";
            this.colId.Visible = true;
            this.colId.VisibleIndex = 1;
            this.colId.Width = 80;
            // 
            // colDetails
            // 
            this.colDetails.Caption = "Hata Detayı";
            this.colDetails.FieldName = "Details";
            this.colDetails.Name = "colDetails";
            this.colDetails.Visible = true;
            this.colDetails.VisibleIndex = 2;
            this.colDetails.Width = 800;
            // 
            // colDate
            // 
            this.colDate.Caption = "Tarih";
            this.colDate.FieldName = "Date_";
            this.colDate.Name = "colDate";
            this.colDate.Visible = true;
            this.colDate.VisibleIndex = 3;
            this.colDate.Width = 200;
            // 
            // ErrorListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(972, 435);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.panelControlTop);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("ErrorListForm.IconOptions.Image")));
            this.KeyPreview = true;
            this.Name = "ErrorListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hata Logları";
            this.Load += new System.EventHandler(this.ErrorListForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ErrorListForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlTop)).EndInit();
            this.panelControlTop.ResumeLayout(false);
            this.panelControlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBitis.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBitis.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBaslangic.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEditBaslangic.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlErrors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewErrors)).EndInit();
            this.ResumeLayout(false);

        }
    }
}