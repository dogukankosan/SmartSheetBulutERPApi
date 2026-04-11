// PurInvoiceFaturaModel.cs
using System;
using System.Collections.Generic;

namespace SmartSheetProject.Models
{
    // Ham sorgu satırı (SQL'den gelen düz veri)
    public class PurInvoiceFaturaModel
    {
        public int FATURALOGICALREF { get; set; }
        public string FATURA_NO { get; set; }
        public DateTime? TARIHI { get; set; }
        public DateTime? FATURA_VADE_TARIHI { get; set; }
        public string CARI_KODU { get; set; }
        public string CARI_ACIKLAMASI { get; set; }
        public string PROJE_KODU { get; set; }
        public string PARA_BIRIMI { get; set; }
        public decimal KUR { get; set; }
 
        public string FATURA_TIPI { get; set; }
        public decimal FATURA_KDVSIZ_TUTAR { get; set; }
        public decimal KDV_TUTARI { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_TL { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_ID { get; set; }
        public string FATURA_ACIKLAMASI { get; set; }
        // Malzeme detay alanları
        public string MALZEME_KODU { get; set; }
        public string MALZEME_ADI { get; set; }
        public decimal MIKTAR { get; set; }
        public decimal SATIR_KDV_DOVIZ { get; set; }
        public string BIRIM { get; set; }
        public decimal BIRIM_FIYAT { get; set; }
        public decimal SATIR_TOPLAM_TL { get; set; }
        public decimal SATIR_TOPLAM_DOVIZ { get; set; }
        public string SATIR_DOVIZ { get; set; }
        public decimal SATIR_KUR { get; set; }
        public decimal SATIR_KDV_ORANI { get; set; }
        public decimal SATIR_KDV_TL { get; set; }
        public string SATIR_ACIKLAMASI { get; set; }
    }

    // Master (fatura başlığı)
    public class PurInvoiceMasterModel
    {
        public int FATURALOGICALREF { get; set; }
        public string FATURA_NO { get; set; }
        public DateTime? TARIHI { get; set; }
        public DateTime? FATURA_VADE_TARIHI { get; set; }
        public string CARI_KODU { get; set; }
        public string CARI_ACIKLAMASI { get; set; }
        public string PROJE_KODU { get; set; }
        public string PARA_BIRIMI { get; set; }
        public decimal KDV_TUTARI_DOVIZ { get; set; }
        public decimal KUR { get; set; }
        public string FATURA_TIPI { get; set; }
        public decimal FATURA_KDVSIZ_TUTAR { get; set; }
        public decimal KDV_TUTARI { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_TL { get; set; }
        public decimal FATURA_TOPLAM_TUTAR_ID { get; set; }
        public string FATURA_ACIKLAMASI { get; set; }
        public List<PurInvoiceDetayModel> Detaylar { get; set; } = new List<PurInvoiceDetayModel>();
    }

    // Detail (malzeme satırı)
    public class PurInvoiceDetayModel
    {
        public int FATURALOGICALREF { get; set; }
        public string MALZEME_KODU { get; set; }
        public string MALZEME_ADI { get; set; }
        public string SATIR_ACIKLAMASI { get; set; }
        public decimal MIKTAR { get; set; }
        public decimal SATIR_KDV_DOVIZ { get; set; }
        public string BIRIM { get; set; }
        public decimal BIRIM_FIYAT { get; set; }
        public decimal SATIR_TOPLAM_TL { get; set; }
        public decimal SATIR_KDV_ORANI { get; set; }
        public decimal SATIR_KDV_TL { get; set; }
        public string SATIR_DOVIZ { get; set; }
        public decimal SATIR_KUR { get; set; }
        public decimal SATIR_TOPLAM_DOVIZ { get; set; }
    }
}