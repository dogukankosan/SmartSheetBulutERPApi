using System;

namespace SmartSheetProject.Models
{
    public class ExpenseModel
    {
        public string UID { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public string KayitEdenKullanici { get; set; }
        public string SirketAdi { get; set; }
        public DateTime? FaturaTarihi { get; set; }
        public string FaturaNo { get; set; }
        public string ProjeKodu { get; set; }
        public string PaymentType { get; set; }
        public string FaturaAciklamasi { get; set; }
        public string DovizTuru { get; set; }
        public decimal? Amount { get; set; }
        public string MalzemeListesi { get; set; }
        public string KDV { get; set; }
        public decimal? KDVOrani { get; set; }
        public decimal? BirimFiyat { get; set; }
        public decimal? SatirToplamTutar { get; set; }
        public string MuhasebeOnay { get; set; }
        public string SupervisorApproval { get; set; }
        public string YoneticiOnay { get; set; }
        public bool? Archive { get; set; }
        public string LogoReference { get; set; }
        // ← BURAYA EKLE
        public long SmartsheetRowId { get; set; }
        /// <summary>
        /// 3 onay da verilmiş mi?
        /// </summary>
        public bool IsFullyApproved
        {
            get
            {
                return MuhasebeOnay == "Approved" &&
                       YoneticiOnay == "Approved" &&
                       SupervisorApproval == "Approved";
            }
        }
        /// <summary>
        /// KDV var mı?
        /// </summary>
        public bool HasVAT => KDV?.ToUpper() == "VAR";
        /// <summary>
        /// Malzeme kodu parse et (--- öncesi)
        /// </summary>
        public string GetMalzemeKodu()
        {
            if (string.IsNullOrWhiteSpace(MalzemeListesi))
                return string.Empty;
            if (MalzemeListesi.Contains("---"))
                return MalzemeListesi.Split(new[] { "---" }, StringSplitOptions.None)[0].Trim();
            return MalzemeListesi.Trim();
        }
        /// <summary>
        /// KDV'siz tutarı hesapla
        /// </summary>
        public decimal CalculateNetAmount()
        {
            if (!SatirToplamTutar.HasValue)
                return 0;
            if (HasVAT && KDVOrani.HasValue && KDVOrani.Value > 0)
                return Math.Round(SatirToplamTutar.Value * 100 / (100 + KDVOrani.Value), 2);
            return SatirToplamTutar.Value;
        }
        /// <summary>
        /// KDV tutarını hesapla
        /// </summary>
        public decimal CalculateVATAmount()
        {
            if (!HasVAT || !SatirToplamTutar.HasValue)
                return 0;
            decimal netAmount = CalculateNetAmount();
            return Math.Round(SatirToplamTutar.Value - netAmount, 2);
        }
    }
}