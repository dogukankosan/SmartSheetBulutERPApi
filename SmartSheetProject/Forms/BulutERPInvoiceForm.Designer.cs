namespace SmartSheetProject.Forms
{
    partial class BulutERPInvoiceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulutERPInvoiceForm));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.btnYenile = new DevExpress.XtraBars.BarButtonItem();
            this.btnExcel = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.lblSeciliSayi = new DevExpress.XtraEditors.LabelControl();
            this.btnSecimiTemizle = new DevExpress.XtraEditors.SimpleButton();
            this.btnTumunuSec = new DevExpress.XtraEditors.SimpleButton();
            this.btn_AktarilmayanlarinHepsiniSec = new DevExpress.XtraEditors.SimpleButton();
            this.btnGrubuCoz = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnAktarLogo = new DevExpress.XtraEditors.SimpleButton();
            this.panelTarihFiltre = new DevExpress.XtraEditors.PanelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.dateBaslangic = new DevExpress.XtraEditors.DateEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.dateBitis = new DevExpress.XtraEditors.DateEdit();
            this.btnFiltrele = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelTarihFiltre)).BeginInit();
            this.panelTarihFiltre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateBaslangic.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBaslangic.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBitis.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBitis.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar2});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnYenile,
            this.btnExcel});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 2;
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btnYenile),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnExcel)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // btnYenile
            // 
            this.btnYenile.Caption = "🔄 Yenile";
            this.btnYenile.Id = 0;
            this.btnYenile.Name = "btnYenile";
            this.btnYenile.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            // 
            // btnExcel
            // 
            this.btnExcel.Caption = "📊 Excel\'e Aktar";
            this.btnExcel.Id = 1;
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
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
            this.gridControl1.Location = new System.Drawing.Point(0, 130);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.MenuManager = this.barManager1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1400, 470);
            this.gridControl1.TabIndex = 4;
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
            this.gridView1.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.OptionsView.ShowFooter = true;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.lblSeciliSayi);
            this.panelControl1.Controls.Add(this.btnSecimiTemizle);
            this.panelControl1.Controls.Add(this.btnTumunuSec);
            this.panelControl1.Controls.Add(this.btn_AktarilmayanlarinHepsiniSec);
            this.panelControl1.Controls.Add(this.btnGrubuCoz);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.btnAktarLogo);
            this.panelControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 20);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1400, 70);
            this.panelControl1.TabIndex = 9;
            // 
            // lblSeciliSayi
            // 
            this.lblSeciliSayi.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblSeciliSayi.Appearance.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblSeciliSayi.Appearance.Options.UseFont = true;
            this.lblSeciliSayi.Appearance.Options.UseForeColor = true;
            this.lblSeciliSayi.Location = new System.Drawing.Point(1190, 27);
            this.lblSeciliSayi.Name = "lblSeciliSayi";
            this.lblSeciliSayi.Size = new System.Drawing.Size(94, 16);
            this.lblSeciliSayi.TabIndex = 6;
            this.lblSeciliSayi.Text = "Seçili: 0 fatura";
            // 
            // btnSecimiTemizle
            // 
            this.btnSecimiTemizle.Appearance.BackColor = System.Drawing.Color.LightGray;
            this.btnSecimiTemizle.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnSecimiTemizle.Appearance.Options.UseBackColor = true;
            this.btnSecimiTemizle.Appearance.Options.UseFont = true;
            this.btnSecimiTemizle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSecimiTemizle.Location = new System.Drawing.Point(460, 20);
            this.btnSecimiTemizle.Name = "btnSecimiTemizle";
            this.btnSecimiTemizle.Size = new System.Drawing.Size(110, 30);
            this.btnSecimiTemizle.TabIndex = 4;
            this.btnSecimiTemizle.Text = "🗑 Temizle";
            // 
            // btnTumunuSec
            // 
            this.btnTumunuSec.Appearance.BackColor = System.Drawing.Color.LightBlue;
            this.btnTumunuSec.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnTumunuSec.Appearance.Options.UseBackColor = true;
            this.btnTumunuSec.Appearance.Options.UseFont = true;
            this.btnTumunuSec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTumunuSec.Location = new System.Drawing.Point(344, 20);
            this.btnTumunuSec.Name = "btnTumunuSec";
            this.btnTumunuSec.Size = new System.Drawing.Size(110, 30);
            this.btnTumunuSec.TabIndex = 3;
            this.btnTumunuSec.Text = "☑ Tümünü Seç";
            // 
            // btn_AktarilmayanlarinHepsiniSec
            // 
            this.btn_AktarilmayanlarinHepsiniSec.Appearance.BackColor = System.Drawing.Color.Orange;
            this.btn_AktarilmayanlarinHepsiniSec.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btn_AktarilmayanlarinHepsiniSec.Appearance.Options.UseBackColor = true;
            this.btn_AktarilmayanlarinHepsiniSec.Appearance.Options.UseFont = true;
            this.btn_AktarilmayanlarinHepsiniSec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_AktarilmayanlarinHepsiniSec.Location = new System.Drawing.Point(158, 20);
            this.btn_AktarilmayanlarinHepsiniSec.Name = "btn_AktarilmayanlarinHepsiniSec";
            this.btn_AktarilmayanlarinHepsiniSec.Size = new System.Drawing.Size(180, 30);
            this.btn_AktarilmayanlarinHepsiniSec.TabIndex = 2;
            this.btn_AktarilmayanlarinHepsiniSec.Text = "⚪ Aktarılmayanları Seç";
            // 
            // btnGrubuCoz
            // 
            this.btnGrubuCoz.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnGrubuCoz.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnGrubuCoz.Appearance.Options.UseBackColor = true;
            this.btnGrubuCoz.Appearance.Options.UseFont = true;
            this.btnGrubuCoz.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGrubuCoz.Location = new System.Drawing.Point(12, 20);
            this.btnGrubuCoz.Name = "btnGrubuCoz";
            this.btnGrubuCoz.Size = new System.Drawing.Size(140, 30);
            this.btnGrubuCoz.TabIndex = 1;
            this.btnGrubuCoz.Text = "📂 Grubu Çöz";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Italic);
            this.labelControl1.Appearance.ForeColor = System.Drawing.Color.DarkBlue;
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Appearance.Options.UseForeColor = true;
            this.labelControl1.Location = new System.Drawing.Point(769, 16);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(331, 42);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "✅ Yeşil satırlar = Logo\'da mevcut faturalar\r\n⚪ Beyaz satırlar = Henüz aktarılmamı" +
    "ş\r\n📌 Grup seçimi: Bir fatura seçince tüm satırları otomatik seçilir";
            // 
            // btnAktarLogo
            // 
            this.btnAktarLogo.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Success;
            this.btnAktarLogo.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnAktarLogo.Appearance.Options.UseBackColor = true;
            this.btnAktarLogo.Appearance.Options.UseFont = true;
            this.btnAktarLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAktarLogo.Location = new System.Drawing.Point(580, 20);
            this.btnAktarLogo.Name = "btnAktarLogo";
            this.btnAktarLogo.Size = new System.Drawing.Size(150, 30);
            this.btnAktarLogo.TabIndex = 0;
            this.btnAktarLogo.Text = "🚀 Logo\'ya Aktar";
            // 
            // panelTarihFiltre
            // 
            this.panelTarihFiltre.Controls.Add(this.labelControl2);
            this.panelTarihFiltre.Controls.Add(this.dateBaslangic);
            this.panelTarihFiltre.Controls.Add(this.labelControl3);
            this.panelTarihFiltre.Controls.Add(this.dateBitis);
            this.panelTarihFiltre.Controls.Add(this.btnFiltrele);
            this.panelTarihFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTarihFiltre.Location = new System.Drawing.Point(0, 90);
            this.panelTarihFiltre.Name = "panelTarihFiltre";
            this.panelTarihFiltre.Size = new System.Drawing.Size(1400, 40);
            this.panelTarihFiltre.TabIndex = 10;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.labelControl2.Appearance.Options.UseFont = true;
            this.labelControl2.Location = new System.Drawing.Point(12, 12);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(115, 14);
            this.labelControl2.TabIndex = 0;
            this.labelControl2.Text = "📅 Başlangıç Tarihi:";
            // 
            // dateBaslangic
            // 
            this.dateBaslangic.EditValue = null;
            this.dateBaslangic.Location = new System.Drawing.Point(135, 9);
            this.dateBaslangic.Name = "dateBaslangic";
            this.dateBaslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateBaslangic.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateBaslangic.Properties.DisplayFormat.FormatString = "dd.MM.yyyy";
            this.dateBaslangic.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dateBaslangic.Properties.EditFormat.FormatString = "dd.MM.yyyy";
            this.dateBaslangic.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dateBaslangic.Properties.Mask.EditMask = "dd.MM.yyyy";
            this.dateBaslangic.Size = new System.Drawing.Size(120, 20);
            this.dateBaslangic.TabIndex = 1;
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(273, 12);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(85, 14);
            this.labelControl3.TabIndex = 2;
            this.labelControl3.Text = "📅 Bitiş Tarihi:";
            // 
            // dateBitis
            // 
            this.dateBitis.EditValue = null;
            this.dateBitis.Location = new System.Drawing.Point(374, 9);
            this.dateBitis.Name = "dateBitis";
            this.dateBitis.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateBitis.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateBitis.Properties.DisplayFormat.FormatString = "dd.MM.yyyy";
            this.dateBitis.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dateBitis.Properties.EditFormat.FormatString = "dd.MM.yyyy";
            this.dateBitis.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dateBitis.Properties.Mask.EditMask = "dd.MM.yyyy";
            this.dateBitis.Size = new System.Drawing.Size(120, 20);
            this.dateBitis.TabIndex = 3;
            // 
            // btnFiltrele
            // 
            this.btnFiltrele.Appearance.BackColor = System.Drawing.Color.LightGreen;
            this.btnFiltrele.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnFiltrele.Appearance.Options.UseBackColor = true;
            this.btnFiltrele.Appearance.Options.UseFont = true;
            this.btnFiltrele.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFiltrele.Location = new System.Drawing.Point(516, 6);
            this.btnFiltrele.Name = "btnFiltrele";
            this.btnFiltrele.Size = new System.Drawing.Size(100, 28);
            this.btnFiltrele.TabIndex = 4;
            this.btnFiltrele.Text = "🔍 Filtrele";
            // 
            // BulutERPInvoice
            // 
            this.AcceptButton = this.btnAktarLogo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 600);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.panelTarihFiltre);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("BulutERPInvoice.IconOptions.Image")));
            this.KeyPreview = true;
            this.Name = "BulutERPInvoice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "📋 Smartsheet → Logo Fatura Aktarımı (✅ Yeşil = Logo\'da Var)";
            this.Load += new System.EventHandler(this.BulutERPInvoice_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BulutERPInvoice_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelTarihFiltre)).EndInit();
            this.panelTarihFiltre.ResumeLayout(false);
            this.panelTarihFiltre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateBaslangic.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBaslangic.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBitis.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateBitis.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.BarButtonItem btnYenile;
        private DevExpress.XtraBars.BarButtonItem btnExcel;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnAktarLogo;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnGrubuCoz;
        private DevExpress.XtraEditors.SimpleButton btn_AktarilmayanlarinHepsiniSec;
        private DevExpress.XtraEditors.SimpleButton btnTumunuSec;
        private DevExpress.XtraEditors.SimpleButton btnSecimiTemizle;
        private DevExpress.XtraEditors.LabelControl lblSeciliSayi;
        // YENİ KONTROLLER!
        private DevExpress.XtraEditors.PanelControl panelTarihFiltre;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.DateEdit dateBaslangic;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.DateEdit dateBitis;
        private DevExpress.XtraEditors.SimpleButton btnFiltrele;
    }
}