# 📊 SmartSheetBulutERPApi

![License](https://img.shields.io/github/license/dogukankosan/SmartSheetBulutERPApi)
![Stars](https://img.shields.io/github/stars/dogukankosan/SmartSheetBulutERPApi)
![Issues](https://img.shields.io/github/issues/dogukankosan/SmartSheetBulutERPApi)
![Last Commit](https://img.shields.io/github/last-commit/dogukankosan/SmartSheetBulutERPApi)

> **SmartSheetBulutERPApi**, Logo Bulut ERP sisteminden fatura verilerini otomatik olarak çekerek Smartsheet platformuna aktaran, lisans bazlı çalışan bir masaüstü C#/.NET entegrasyon uygulamasıdır.

---

## 🚀 Özellikler

- 🔗 Logo Bulut ERP ile tam entegrasyon (REST API)
- 📊 Smartsheet otomasyonu (Gider/Gelir sheet'leri)
- 🔐 Hardware ID bazlı lisans sistemi
- 🔄 Otomatik token yönetimi ve yenileme
- 📋 Gider ve Gelir faturalarını otomatik aktarma
- 🚫 Duplicate (tekrarlı kayıt) kontrolü
- 💾 SQLite ile yerel ayar ve log yönetimi
- 🔒 Şifreli token ve API key saklama
- ⚡ Toplu veri işleme desteği
- 📝 Detaylı hata loglama sistemi
- ⏱ Token süre takibi (5 dakika önceden otomatik yenileme)

---

## 🗂 Proje Yapısı
```
SmartSheetBulutERPApi/
├── Classes/
│   ├── BulutERPService.cs          # Bulut ERP API servisi ve token yönetimi
│   ├── SmartsheetService.cs        # Smartsheet entegrasyonu
│   ├── LicenseApiClient.cs         # Lisans doğrulama ve aktivasyon
│   ├── LicenseManager.cs           # Hardware ID ve lisans kontrolü
│   ├── SQLiteCrud.cs               # SQLite veritabanı işlemleri
│   ├── EncryptionHelper.cs         # Token şifreleme/şifre çözme
│   ├── TextLog.cs                  # Loglama sınıfı
│   └── BulutERPConnectionTest.cs   # Bağlantı testi ve ayarlar
├── Models/
│   ├── GiderFaturaModel.cs         # Gider fatura veri modeli
│   ├── GelirFaturaModel.cs         # Gelir fatura veri modeli
│   └── BulutERPSettings.cs         # Bulut ERP ayarları modeli
├── Forms/
│   └── MainForm.cs                 # Ana uygulama ekranı
└── Database/
    └── Settings.db                 # SQLite veritabanı
```

---

## 🛠️ Kurulum & Çalıştırma

### Gereksinimler

- .NET Framework 4.7.2 veya üzeri
- Logo Bulut ERP hesabı ve API erişimi
- Smartsheet hesabı ve API token
- Aktif lisans anahtarı

### Kurulum

1. **Projeyi Klonla:**
```bash
   git clone https://github.com/dogukankosan/SmartSheetBulutERPApi.git
   cd SmartSheetBulutERPApi
```

2. **Projeyi Visual Studio ile Aç ve Derle**

3. **İlk Çalıştırma:**
   - Lisans anahtarınızı girin ve aktive edin
   - Bulut ERP bağlantı bilgilerini ayarlayın
   - Smartsheet API token'ınızı kaydedin

---

## 🔧 Yapılandırma

### Bulut ERP Ayarları
```csharp
// BulutERPSettings modeli
{
    "ServerUrl": "https://your-erp-server.com",
    "MachineID": "your-machine-id",
    "FirmNr": "001",
    "Username": "api-user",
    "Password": "encrypted-password"
}
```

### Smartsheet Ayarları

- **Gider Sheet ID:** `6658795850649476`
- **Gelir Sheet ID:** `4003473281470340`
- **API Token:** Şifreli olarak SQLite'da saklanır

---

## 📡 Ana Servisler

### 1. Bulut ERP Servisi

| Metod | Açıklama |
|-------|----------|
| `EnsureValidTokenAsync()` | Token kontrolü ve otomatik yenileme |
| `ExecuteSelectQueryAsync()` | SQL sorgusu çalıştırma |
| `GetTokenAsync()` | Yeni token alma |

**Örnek Kullanım:**
```csharp
// SQL sorgusu çalıştırma (token otomatik yönetilir)
var result = await BulutERPService.ExecuteSelectQueryAsync(
    "SELECT * FROM LG_$V(firm)_01_INVOICE WHERE TRCODE = 1",
    maxCount: 10000
);

if (result.Success)
{
    foreach (var row in result.Data)
    {
        // Fatura verilerini işle
    }
}
```

### 2. Smartsheet Servisi

| Metod | Açıklama |
|-------|----------|
| `AddMultipleGiderFaturaAsync()` | Toplu gider faturası ekleme |
| `AddMultipleGelirFaturaAsync()` | Toplu gelir faturası ekleme |
| `GetGiderFaturaKeysAsync()` | Mevcut gider faturaları (duplicate kontrolü) |
| `GetGelirFaturaKeysAsync()` | Mevcut gelir faturaları (duplicate kontrolü) |
| `TestConnectionAsync()` | Bağlantı testi |

**Örnek Kullanım:**
```csharp
// Gider faturaları ekleme
List<GiderFaturaModel> faturalar = new List<GiderFaturaModel>();
// ... fatura listesini doldur

var result = await SmartsheetService.AddMultipleGiderFaturaAsync(faturalar);
if (result.Success)
{
    Console.WriteLine($"{result.Count} adet fatura eklendi!");
}
```

### 3. Lisans Servisi

| Metod | Açıklama |
|-------|----------|
| `ActivateLicenseAsync()` | Lisans aktivasyonu |
| `ValidateLicenseAsync()` | Lisans doğrulama |
| `CheckApiHealthAsync()` | API sağlık kontrolü |

---

## ⚡ Kullanım Senaryosu

1. **Lisans Aktivasyonu:** Uygulama ilk açılışta lisans anahtarı ile aktive edilir
2. **Ayarlar:** Bulut ERP ve Smartsheet bağlantı bilgileri girilir
3. **Token Alımı:** Bulut ERP'den otomatik token alınır (5 dakikada bir yenilenir)
4. **Veri Çekme:** Logo Bulut ERP'den SQL sorguları ile fatura verileri çekilir
5. **Duplicate Kontrol:** Smartsheet'teki mevcut kayıtlar kontrol edilir
6. **Aktarım:** Yeni faturalar Smartsheet'e toplu olarak eklenir

---

## 📊 Smartsheet Kolon Yapısı

### Gider Faturaları (GIDER Sheet)

| Kolon | ColumnID | Açıklama |
|-------|----------|----------|
| Cari Kodu | 7349303226617732 | Tedarikçi kodu |
| Cari Açıklaması | 4684432181776260 | Tedarikçi unvanı |
| Cari Bakiyesi | 1554876251983748 | Tedarikçi borç/alacak |
| Proje Kodu | 1719803692404612 | Proje kodu |
| Fatura No | 6223403319775108 | Fatura numarası |
| Tarihi | 3971603506089860 | Fatura tarihi |
| Fatura Açıklaması | 4455123877842820 | Açıklama |
| Vade Tarihi | 4402456749100932 | Vade tarihi |
| Vade Kalan Gün | 6274768167456644 | Kalan gün sayısı |
| Kur | 6995074985185156 | Döviz kuru |
| Para Birimi | 8507499609804676 | TRY/USD/EUR |
| Toplam TL | 8906056376471428 | TL tutar |
| Toplam Döviz | 8958723505213316 | Döviz tutar |
| KDV | 520388499689348 | KDV tutarı |
| KDV'siz Tutar | 57077366738820 | Matrah |
| Malzeme Bilgileri | 6858151763332996 | Kalem detayları |

### Gelir Faturaları (GELİR Sheet)

| Kolon | ColumnID | Açıklama |
|-------|----------|----------|
| Cari Kodu | 2874156071473028 | Müşteri kodu |
| Cari Açıklaması | 7377755698843524 | Müşteri unvanı |
| Proje Kodu | 1748256164630404 | Proje kodu |
| Fatura No | 6251855792000900 | Fatura numarası |
| Tarihi | 4000055978315652 | Fatura tarihi |
| Fatura Açıklaması | 8503655605686148 | Açıklama |
| Vade Tarihi | 340881281077124 | Vade tarihi |
| Vade Kalan Gün | 4844480908447620 | Kalan gün sayısı |
| Kur | 2592681094762372 | Döviz kuru |
| Para Birimi | 7096280722132868 | TRY/USD/EUR |
| Cari Bakiyesi | 1466781187919748 | Müşteri borç/alacak |
| Toplam TL | 5970380815290244 | TL tutar |
| Toplam Döviz | 3718581001604996 | Döviz tutar |
| KDV | 8222180628975492 | KDV tutarı |
| KDV'siz Tutar | 903831234498436 | Matrah |
| Malzeme Bilgileri | 5407430861868932 | Kalem detayları |

---

## 🔐 Güvenlik Özellikleri

- ✅ API token'ları AES şifreleme ile saklanır
- ✅ Hardware ID bazlı lisans kontrolü
- ✅ Lisans sunucusu ile online doğrulama
- ✅ SQL injection koruması (parametreli sorgular)
- ✅ Otomatik token süre sonu kontrolü
- ✅ Şifreli veritabanı bağlantıları

---

## 📝 Loglama Sistemi

Tüm işlemler SQLite veritabanına loglanır:
```csharp
await TextLog.LogToSQLiteAsync("❌ API bağlantı hatası: Timeout");
await TextLog.LogToSQLiteAsync("✅ 150 adet fatura başarıyla aktarıldı");
```

---

## 🗄 Veritabanı Yapısı

### SQLite Tabloları

| Tablo | Açıklama |
|-------|----------|
| `BulutERPSettings` | Bulut ERP bağlantı bilgileri ve token |
| `SmartsheetSettings` | Smartsheet API token (şifreli) |
| `LicenseInfo` | Lisans bilgileri ve hardware ID |
| `Logs` | İşlem logları ve hatalar |

---

## 🔄 Token Yönetimi Akışı
```
1. İlk İstek
   ↓
2. Token Kontrol
   ├─ Yok/Boş → Token Al
   ├─ Süresi dolmuş → Token Al
   ├─ 5dk'dan az kaldı → Token Al
   └─ Geçerli → Kullan
   ↓
3. API İsteği Gönder
   ↓
4. Başarılı mı?
   ├─ Evet → Veriyi Döndür
   └─ Hayır → Hata Logla
```

---

## 🎯 Duplicate Kontrol Mekanizması

Fatura tekrarını önlemek için:
```csharp
// Smartsheet'teki mevcut faturalar
var existingKeys = await SmartsheetService.GetGiderFaturaKeysAsync();

// Yeni faturalar
var newFaturalar = allFaturalar.Where(f => 
    !existingKeys.FaturaKeys.Contains($"{f.CARI_KODU}|{f.FATURA_NO}")
).ToList();

// Sadece yenileri ekle
await SmartsheetService.AddMultipleGiderFaturaAsync(newFaturalar);
```

---

## 🚦 API Endpoint'leri (Lisans Sunucusu)

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/api/license/activate` | POST | Lisans aktivasyonu |
| `/api/license/validate` | POST | Lisans doğrulama |
| `/api/license/health` | GET | API sağlık kontrolü |

**Base URL:** `http://188.132.128.186:1020`

---

## ⚙️ Bulut ERP SQL Sorguları

### Fatura Çekme Örneği
```sql
SELECT 
    CLCARD.CODE AS CARI_KODU,
    CLCARD.DEFINITION_ AS CARI_ACIKLAMASI,
    INVOICE.FICHENO AS FATURA_NO,
    INVOICE.DATE_ AS TARIHI,
    INVOICE.GENEXP1 AS FATURA_ACIKLAMASI,
    INVOICE.NETTOTAL AS FATURA_TOPLAM_TUTAR_TL
FROM LG_$V(firm)_01_INVOICE INVOICE
LEFT JOIN LG_$V(firm)_CLCARD CLCARD ON INVOICE.CLIENTREF = CLCARD.LOGICALREF
WHERE INVOICE.TRCODE IN (1, 2, 3)
ORDER BY INVOICE.DATE_ DESC
```

**Not:** `$V(firm)` parametresi otomatik olarak firma numarası ile değiştirilir.

---

## 🛡️ Hata Yönetimi
```csharp
try
{
    var result = await BulutERPService.ExecuteSelectQueryAsync(sqlQuery);
    
    if (!result.Success)
    {
        await TextLog.LogToSQLiteAsync($"❌ Sorgu hatası: {result.ErrorMessage}");
        MessageBox.Show(result.ErrorMessage, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }
    
    // Başarılı işlem
}
catch (Exception ex)
{
    await TextLog.LogToSQLiteAsync($"❌ Exception: {ex.Message}");
}
```

---

## 📦 NuGet Paketleri
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Smartsheet.API" Version="3.x" />
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
```

---

## 🤝 Katkı

Katkı sağlamak için projeyi forklayabilir ve pull request gönderebilirsiniz.

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Commit yapın (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'e push edin (`git push origin feature/YeniOzellik`)
5. Pull Request açın

---

## 📄 Lisans

MIT License

---

## 📬 İletişim

- 👨‍💻 Geliştirici: [@dogukankosan](https://github.com/dogukankosan)  
- 🐞 Öneri veya sorunlar: [Issues sekmesi](https://github.com/dogukankosan/SmartSheetBulutERPApi/issues)

---

## 🎯 Kullanım İpuçları

1. **Token Süresi:** Token'lar 1 saat geçerlidir, sistem otomatik yeniler
2. **Toplu İşlem:** 500+ fatura için batch işlem önerilir
3. **Duplicate Kontrol:** Her aktarım öncesi mutlaka duplicate kontrol yapılır
4. **Bağlantı Testi:** İlk kurulumda test butonlarını kullanın
5. **Log Takibi:** Hata durumunda SQLite loglarını inceleyin

---

<p align="center">
  <img src="https://img.shields.io/badge/.NET-Framework%204.7.2+-purple?logo=dotnet" alt="dotnet" />
  <img src="https://img.shields.io/badge/Smartsheet-API-blue?logo=smartsheet" alt="smartsheet" />
  <img src="https://img.shields.io/badge/Logo-Bulut%20ERP-green" alt="logo" />
  <img src="https://img.shields.io/badge/SQLite-Database-orange?logo=sqlite" alt="sqlite" />
  <img src="https://img.shields.io/badge/License-System-red" alt="license" />
</p>
