using System;
using System.Collections.Generic;

namespace SmartSheetProject.Models
{
    public class GroupedExpenseModel
    {
        public string FaturaNo { get; set; }
        public DateTime? FaturaTarihi { get; set; }
        public string KayitEdenKullanici { get; set; }
        public string SirketAdi { get; set; }
        public string FaturaAciklamasi { get; set; }
        public string ProjeKodu { get; set; }
        public string DovizTuru { get; set; }
        public decimal ToplamTutar { get; set; }
        public string LogoReference { get; set; }
        public List<ExpenseModel> Items { get; set; } = new List<ExpenseModel>();
        // Cari bilgileri
        public string CariKodu { get; set; }
        public string CariHataMesaji { get; set; }
        // Malzeme hataları
        public List<string> MalzemeHatalari { get; set; } = new List<string>();
        /// <summary>
        /// Tüm hataları birleştirir (Cari + Malzeme)
        /// </summary>
        public string TumHatalar
        {
            get
            {
                List<string> hatalar = new List<string>();
                if (!string.IsNullOrWhiteSpace(CariHataMesaji))
                    hatalar.Add($"Cari: {CariHataMesaji}");
                if (MalzemeHatalari != null && MalzemeHatalari.Count > 0)
                    hatalar.Add($"Malzeme: {string.Join(", ", MalzemeHatalari)}");
                return hatalar.Count > 0 ? string.Join(" | ", hatalar) : string.Empty;
            }
        }
        /// <summary>
        /// Herhangi bir hata var mı?
        /// </summary>
        public bool HasErrors => !string.IsNullOrWhiteSpace(TumHatalar);
        /// <summary>
        /// Fatura Logo'ya aktarılabilir durumda mı?
        /// </summary>
        public bool IsReadyForLogoTransfer
        {
            get
            {
                return !HasErrors &&
                       !string.IsNullOrWhiteSpace(FaturaNo) &&
                       !string.IsNullOrWhiteSpace(CariKodu) &&
                       FaturaTarihi.HasValue &&
                       Items != null &&
                       Items.Count > 0;
            }
        }
    }
}