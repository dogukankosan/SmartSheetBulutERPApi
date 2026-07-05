using System;
namespace SmartSheetProject.Models
{
    public class GiderFaturaModel
    {
        public int FATURALOGICALREF { get; set; }
        public string FATURA_NO { get; set; }
        public DateTime? TARIHI { get; set; }
        public DateTime? FATURA_VADE_TARIHI { get; set; }
        //public int VADE_KALAN_GUN { get; set; }
        public string CARI_KODU { get; set; }
        public string CARI_ACIKLAMASI { get; set; }
        public decimal CARI_BAKIYESI { get; set; }
        public string PROJE_KODU { get; set; }
        public string PARA_BIRIMI { get; set; }
        public decimal KUR { get; set; }
        public decimal FATURA_KDVSIZ_TUTAR { get; set; }
        public decimal KDV_TUTARI { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_TL { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_ID { get; set; }
        public string FATURA_ACIKLAMASI { get; set; }
        public string MALZEME_BILGILERI { get; set; }
        public string KAYNAK_SHEET { get; set; }
    }
}