namespace SmartSheetProject.Forms
{
    partial class CustomerMaturityForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomerMaturityForm));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.btnYenile = new DevExpress.XtraBars.BarButtonItem();
            this.btnExcel = new DevExpress.XtraBars.BarButtonItem();
            this.btn_CustomerNotCode = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.btn_Smartsheet = new DevExpress.XtraEditors.SimpleButton();
            this.lblSeciliSayi = new DevExpress.XtraEditors.LabelControl();
            this.btnSecimiTemizle = new DevExpress.XtraEditors.SimpleButton();
            this.btnTumunuSec = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnYenile,
            this.btnExcel,
            this.btn_CustomerNotCode});
            this.barManager1.MainMenu = this.bar1;
            this.barManager1.MaxItemId = 3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Main menu";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btnYenile),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnExcel),
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_CustomerNotCode)});
            this.bar1.OptionsBar.MultiLine = true;
            this.bar1.OptionsBar.UseWholeRow = true;
            this.bar1.Text = "Main menu";
            // 
            // btnYenile
            // 
            this.btnYenile.Caption = "🔄 Yenile";
            this.btnYenile.Id = 0;
            this.btnYenile.Name = "btnYenile";
            this.btnYenile.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnYenile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnYenile_ItemClick);
            // 
            // btnExcel
            // 
            this.btnExcel.Caption = "📊 Excel\'e Aktar";
            this.btnExcel.Id = 1;
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            this.btnExcel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExcel_ItemClick);
            // 
            // btn_CustomerNotCode
            // 
            this.btn_CustomerNotCode.Caption = "👨🏿‍🤝‍👨🏿 Cari Filtresi";
            this.btn_CustomerNotCode.Id = 2;
            this.btn_CustomerNotCode.Name = "btn_CustomerNotCode";
            this.btn_CustomerNotCode.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_CustomerNotCode_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1400, 20);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 600);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1400, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 20);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 580);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1400, 20);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 580);
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 70);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.MenuManager = this.barManager1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1400, 530);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.OptionsView.ShowFooter = true;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.btn_Smartsheet);
            this.panelControl1.Controls.Add(this.lblSeciliSayi);
            this.panelControl1.Controls.Add(this.btnSecimiTemizle);
            this.panelControl1.Controls.Add(this.btnTumunuSec);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 20);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1400, 50);
            this.panelControl1.TabIndex = 0;
            // 
            // btn_Smartsheet
            // 
            this.btn_Smartsheet.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Success;
            this.btn_Smartsheet.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btn_Smartsheet.Appearance.Options.UseBackColor = true;
            this.btn_Smartsheet.Appearance.Options.UseFont = true;
            this.btn_Smartsheet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Smartsheet.Location = new System.Drawing.Point(268, 10);
            this.btn_Smartsheet.Name = "btn_Smartsheet";
            this.btn_Smartsheet.Size = new System.Drawing.Size(145, 30);
            this.btn_Smartsheet.TabIndex = 2;
            this.btn_Smartsheet.Text = "🐱‍🏍 Smartsheet Aktar";
            this.btn_Smartsheet.Click += new System.EventHandler(this.btn_Smartsheet_Click);
            // 
            // lblSeciliSayi
            // 
            this.lblSeciliSayi.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblSeciliSayi.Appearance.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblSeciliSayi.Appearance.Options.UseFont = true;
            this.lblSeciliSayi.Appearance.Options.UseForeColor = true;
            this.lblSeciliSayi.Location = new System.Drawing.Point(447, 17);
            this.lblSeciliSayi.Name = "lblSeciliSayi";
            this.lblSeciliSayi.Size = new System.Drawing.Size(85, 16);
            this.lblSeciliSayi.TabIndex = 2;
            this.lblSeciliSayi.Text = "Seçili: 0 kayıt";
            // 
            // btnSecimiTemizle
            // 
            this.btnSecimiTemizle.Appearance.BackColor = System.Drawing.Color.LightGray;
            this.btnSecimiTemizle.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnSecimiTemizle.Appearance.Options.UseBackColor = true;
            this.btnSecimiTemizle.Appearance.Options.UseFont = true;
            this.btnSecimiTemizle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSecimiTemizle.Location = new System.Drawing.Point(142, 10);
            this.btnSecimiTemizle.Name = "btnSecimiTemizle";
            this.btnSecimiTemizle.Size = new System.Drawing.Size(120, 30);
            this.btnSecimiTemizle.TabIndex = 1;
            this.btnSecimiTemizle.Text = "🗑 Temizle";
            this.btnSecimiTemizle.Click += new System.EventHandler(this.btnSecimiTemizle_Click);
            // 
            // btnTumunuSec
            // 
            this.btnTumunuSec.Appearance.BackColor = System.Drawing.Color.LightBlue;
            this.btnTumunuSec.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnTumunuSec.Appearance.Options.UseBackColor = true;
            this.btnTumunuSec.Appearance.Options.UseFont = true;
            this.btnTumunuSec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTumunuSec.Location = new System.Drawing.Point(12, 10);
            this.btnTumunuSec.Name = "btnTumunuSec";
            this.btnTumunuSec.Size = new System.Drawing.Size(120, 30);
            this.btnTumunuSec.TabIndex = 0;
            this.btnTumunuSec.Text = "☑ Tümünü Seç";
            this.btnTumunuSec.Click += new System.EventHandler(this.btnTumunuSec_Click);
            // 
            // CustomerMaturityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1400, 600);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("CustomerMaturityForm.IconOptions.Image")));
            this.KeyPreview = true;
            this.Name = "CustomerMaturityForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "📋 Cari Vade Analizi";
            this.Load += new System.EventHandler(this.CustomerMaturityForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CustomerMaturityForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem btnYenile;
        private DevExpress.XtraBars.BarButtonItem btnExcel;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnTumunuSec;
        private DevExpress.XtraEditors.SimpleButton btnSecimiTemizle;
        private DevExpress.XtraEditors.LabelControl lblSeciliSayi;
        private DevExpress.XtraEditors.SimpleButton btn_Smartsheet;
        private DevExpress.XtraBars.BarButtonItem btn_CustomerNotCode;
    }
}