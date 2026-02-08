# 📊 SmartSheetBulutERPApi

![License](https://img.shields.io/github/license/dogukankosan/SmartSheetBulutERPApi)
![Stars](https://img.shields.io/github/stars/dogukankosan/SmartSheetBulutERPApi)
![Issues](https://img.shields.io/github/issues/dogukankosan/SmartSheetBulutERPApi)
![Last Commit](https://img.shields.io/github/last-commit/dogukankosan/SmartSheetBulutERPApi)

<img width="1514" height="810" alt="ss" src="https://github.com/user-attachments/assets/1707075b-e559-459a-8a19-bb2f98514e77" />


> **SmartSheetBulutERPApi**, Logo Bulut ERP sisteminden fatura verilerini otomatik olarak çekerek Smartsheet platformuna aktaran ve Smartsheet'teki onaylı gider kayıtlarını Logo'ya fatura olarak gönderen, lisans bazlı çalışan bir masaüstü C#/.NET entegrasyon uygulamasıdır.

---

## 🚀 Özellikler

- 🔗 Logo Bulut ERP ile tam entegrasyon (REST API)
- 📊 Smartsheet otomasyonu (Gider/Gelir/Expenses sheet'leri)
- 🔐 Hardware ID bazlı lisans sistemi
- 🔄 Otomatik token yönetimi ve yenileme
- 📋 Gider ve Gelir faturalarını otomatik aktarma
- 💼 **Smartsheet'ten Logo'ya otomatik fatura oluşturma**
- ✅ **3 aşamalı onay kontrolü (Muhasebe/Yönetici/Supervisor)**
- 🚫 Duplicate (tekrarlı kayıt) kontrolü
- 💾 SQLite ile yerel ayar ve log yönetimi
- 🔒 Şifreli token ve API key saklama
- ⚡ Toplu veri işleme desteği
- 📝 Detaylı hata loglama sistemi
- ⏱ Token süre takibi (5 dakika önceden otomatik yenileme)
- 🧾 **JSON log sistemi (başarılı ve hatalı faturalar)**
- 🔍 **Malzeme kodu validasyonu**
- 📧 **Email bazlı cari eşleştirme**

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
│   ├── ExpenseModel.cs             # ✨ Smartsheet gider kaydı modeli
│   ├── GroupedExpenseModel.cs      # ✨ Gruplandırılmış gider modeli
│   └── BulutERPSettings.cs         # Bulut ERP ayarları modeli
├── Forms/
│   └── MainForm.cs                 # Ana uygulama ekranı
├── Database/
│   └── Settings.db                 # SQLite veritabanı
└── JSONLog/                         # ✨ Fatura JSON logları (başarılı/hatalı)
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
- **Expenses Sheet ID:** `8931463861849988` ✨
- **API Token:** Şifreli olarak SQLite'da saklanır

---

## 📡 Ana Servisler

### 1. Bulut ERP Servisi

| Metod | Açıklama |
|-------|----------|
| `EnsureValidTokenAsync()` | Token kontrolü ve otomatik yenileme |
| `ExecuteSelectQueryAsync()` | SQL sorgusu çalıştırma |
| `GetTokenAsync()` | Yeni token alma |
| `CreateInvoiceAsync()` | ✨ Logo'ya fatura oluşturma |
| `GetMalzemeCardTypeAsync()` | ✨ Malzeme kodu validasyonu |
| `ConvertGroupedExpenseToInvoiceAsync()` | ✨ Expense'i faturaya dönüştürme |
| `CheckInvoiceExistsAsync()` | ✨ Fatura kontrol (duplicate check) |

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

// ✨ Logo'ya fatura oluşturma
var invoiceResult = await BulutERPService.CreateInvoiceAsync(
    invoiceData: invoiceObject,
    invoiceType: 4  // 1=Satınalma, 4=Hizmet
);

if (invoiceResult.Success)
{
    Console.WriteLine($"Logo Fiş No: {invoiceResult.LogoInvoiceNo}");
}
```

### 2. Smartsheet Servisi

| Metod | Açıklama |
|-------|----------|
| `AddMultipleGiderFaturaAsync()` | Toplu gider faturası ekleme |
| `AddMultipleGelirFaturaAsync()` | Toplu gelir faturası ekleme |
| `GetGiderFaturaKeysAsync()` | Mevcut gider faturaları (duplicate kontrolü) |
| `GetGelirFaturaKeysAsync()` | Mevcut gelir faturaları (duplicate kontrolü) |
| `GetGroupedApprovedExpensesAsync()` | ✨ Onaylı giderleri grupla ve getir |
| `GetCariKoduByEmailAsync()` | ✨ Email ile cari kodu bul |
| `ValidateExpenseAsync()` | ✨ Gider kaydı validasyonu |
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

// ✨ Onaylı giderleri getir ve Logo'ya gönder
var expensesResult = await SmartsheetService.GetGroupedApprovedExpensesAsync();
if (expensesResult.Success)
{
    foreach (var group in expensesResult.GroupedExpenses)
    {
        // Email ile cari bul
        var cariResult = await SmartsheetService.GetCariKoduByEmailAsync(
            group.KayitEdenKullanici
        );
        
        if (cariResult.Success)
        {
            // Faturaya dönüştür
            var invoiceResult = await BulutERPService.ConvertGroupedExpenseToInvoiceAsync(
                group, 
                cariResult.CariKodu
            );
            
            // Logo'ya gönder
            await BulutERPService.CreateInvoiceAsync(
                invoiceResult.InvoiceData,
                invoiceResult.InvoiceType
            );
        }
    }
}
```

### 3. Lisans Servisi

| Metod | Açıklama |
|-------|----------|
| `ActivateLicenseAsync()` | Lisans aktivasyonu |
| `ValidateLicenseAsync()` | Lisans doğrulama |
| `CheckApiHealthAsync()` | API sağlık kontrolü |

---

## ⚡ Kullanım Senaryoları

### Senaryo 1: Logo'dan Smartsheet'e Fatura Aktarımı

1. **Lisans Aktivasyonu:** Uygulama ilk açılışta lisans anahtarı ile aktive edilir
2. **Ayarlar:** Bulut ERP ve Smartsheet bağlantı bilgileri girilir
3. **Token Alımı:** Bulut ERP'den otomatik token alınır (5 dakikada bir yenilenir)
4. **Veri Çekme:** Logo Bulut ERP'den SQL sorguları ile fatura verileri çekilir
5. **Duplicate Kontrol:** Smartsheet'teki mevcut kayıtlar kontrol edilir
6. **Aktarım:** Yeni faturalar Smartsheet'e toplu olarak eklenir

### Senaryo 2: Smartsheet'ten Logo'ya Gider Faturası Oluşturma ✨

1. **Onaylı Giderleri Getir:** 3 aşamalı onayı geçmiş kayıtlar çekilir
2. **Gruplama:** Aynı fatura no, kayıt eden ve tarihe sahip kayıtlar gruplanır
3. **Validasyon:** 
   - Fatura no, tarih, malzeme kodu kontrolü
   - Malzeme kodlarının Logo'da varlığı doğrulanır
   - Email adresi ile cari kodu eşleştirilir
4. **Duplicate Kontrol:** Logo'da aynı fatura var mı kontrol edilir
5. **Dönüştürme:** Expense kayıtları Logo fatura formatına dönüştürülür
6. **JSON Kayıt:** İstek JSONLog klasörüne kaydedilir
7. **Logo'ya Gönderim:** REST API ile fatura oluşturulur
8. **Sonuç:** Başarılı/hatalı durumlar loglanır ve JSON'a kaydedilir

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

### Expenses Sheet (Gider Onay) ✨

| Kolon | ColumnID | Açıklama |
|-------|----------|----------|
| UID | 158077037531012 | Benzersiz kayıt ID |
| Kayıt Tarihi | 4661676664901508 | Kayıt oluşturma tarihi |
| Kayıt Eden | 2409876851216260 | Email adresi (cari eşleştirme için) |
| Şirket Adı | 6913476478586756 | Tedarikçi adı |
| Fatura Tarihi | 3535776758058884 | Fatura tarihi |
| Fatura No | 8039376385429380 | Fatura numarası (gruplama anahtarı) |
| Proje Kodu | 5224626618322820 | Proje kodu |
| Fatura Açıklaması | 1846926897794948 | Açıklama |
| Döviz Türü | 6350526525165444 | TRY/USD/EUR |
| Amount | 8602326338850692 | Tutar |
| Malzeme Listesi | 7125346611318660 | Malzeme kodu (validasyon için) |
| KDV | 439552014241668 | VAR/YOK |
| KDV Oranı | 1035061510754180 | KDV % |
| Birim Fiyat | 2240014585646980 | Birim fiyat |
| Satır Toplam | 8471080124239748 | Satır toplam tutar |
| Muhasebe Onay | 7194951455297412 | Approved/Rejected |
| Yönetici Onay | 6069051548454788 | Approved/Rejected |
| Supervisor Approval | 1565451921084292 | Approved/Rejected |
| Archive | 3817251734769540 | Arşivleme durumu |
| Logo Reference | 8320851362140036 | Logo fiş numarası |

---

## 🔐 Güvenlik Özellikleri

- ✅ API token'ları AES şifreleme ile saklanır
- ✅ Hardware ID bazlı lisans kontrolü
- ✅ Lisans sunucusu ile online doğrulama
- ✅ SQL injection koruması (parametreli sorgular)
- ✅ Otomatik token süre sonu kontrolü
- ✅ Şifreli veritabanı bağlantıları
- ✅ 3 aşamalı onay mekanizması ✨
- ✅ Detaylı JSON log sistemi ✨

---

## 📝 Loglama Sistemi

### SQLite Logları
Tüm işlemler SQLite veritabanına loglanır:
```csharp
await TextLog.LogToSQLiteAsync("❌ API bağlantı hatası: Timeout");
await TextLog.LogToSQLiteAsync("✅ 150 adet fatura başarıyla aktarıldı");
```

### JSON Log Sistemi ✨
Her Logo fatura işlemi JSONLog klasörüne kaydedilir:

**Başarılı Fatura:**
```
JSONLog/
└── 20250208_143052_FAT2025001.json
```
```json
/*
=================================================
BAŞARILI - LOGO FATURA AKTARIM
=================================================
Fatura No: FAT2025001
Logo Fiş No: ~FAT2025001
Tarih: 08.02.2025 14:30:52
Invoice Type: 4
=================================================
*/

// ========== REQUEST JSON ==========
{
  "no": "FAT2025001",
  "date": "2025-02-08T00:00:00+03:00",
  ...
}

// ========== RESPONSE SUCCESS ==========
{
  "no": "~FAT2025001",
  "successful": true
}
```

**Hatalı Fatura:**
```
JSONLog/
└── 20250208_143052_HATALI_FAT2025002.json
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

### Smartsheet → Smartsheet (Gider/Gelir)
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

### Smartsheet → Logo ✨
```csharp
// Logo'da fatura var mı kontrol et
var existsResult = await BulutERPService.CheckInvoiceExistsAsync(
    faturaNo: group.FaturaNo,
    cariKodu: cariKodu,
    faturaTarihi: group.FaturaTarihi.Value
);

if (existsResult.Exists)
{
    await TextLog.LogToSQLiteAsync($"⚠️ Fatura zaten Logo'da mevcut: {group.FaturaNo}");
    continue; // Bu faturayı atla
}
```

---

## 🚦 API Endpoint'leri

### Lisans Sunucusu

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/api/license/activate` | POST | Lisans aktivasyonu |
| `/api/license/validate` | POST | Lisans doğrulama |
| `/api/license/health` | GET | API sağlık kontrolü |

**Base URL:** `http://188.132.128.186:1020`

### Logo Bulut ERP API

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/restservices/rest/dataQuery/executeSelectQuery` | POST | SQL sorgusu çalıştırma |
| `/restservices/rest/v2.0/invoices/purchase` | POST | Satınalma faturası oluşturma ✨ |

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

### Email ile Cari Bulma ✨
```sql
SELECT CODE 
FROM U_$V(firm)_ARPS 
WHERE BOSTATUS<>1 
  AND UPPER(EMAIL) = UPPER('user@example.com')
```

### Malzeme Kontrolü ✨
```sql
SELECT CARDTYPE 
FROM U_$V(firm)_ITEMS 
WHERE BOSTATUS<>1 
  AND CODE='MAL001'
```

### Fatura Kontrolü ✨
```sql
SELECT INV.LOGICALREF, INV.SLIPNR, ARP.CODE, INV.SLIPDATE
FROM U_$V(firm)_01_INVOICES INV
JOIN U_$V(firm)_ARPS ARP ON ARP.LOGICALREF = INV.ARPREF
WHERE INV.SLIPNR = 'FAT2025001'
  AND ARP.CODE = 'CARI001'
```

**Not:** `$V(firm)` parametresi otomatik olarak firma numarası ile değiştirilir.

---

## 🛡️ Hata Yönetimi

### Genel Try-Catch Yapısı
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

### Expense Validasyonu ✨
```csharp
var validationResult = await SmartsheetService.ValidateExpenseAsync(expense);

if (!validationResult.IsValid)
{
    string hataMesaji = string.Join("\n", validationResult.Errors);
    await TextLog.LogToSQLiteAsync($"❌ Validasyon hatası: {hataMesaji}");
    // Hatalı kayıtları atla veya kullanıcıya bildir
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

## 🔄 Smartsheet → Logo Fatura Akışı ✨

```
1. Onaylı Giderleri Getir
   ↓
   • Muhasebe Onay = "Approved"
   • Yönetici Onay = "Approved"  
   • Supervisor Approval = "Approved"
   ↓
2. Fatura No ve Tarihe Göre Grupla
   ↓
   • Aynı fatura no
   • Aynı kayıt eden
   • Her grup için toplam tutar hesapla
   ↓
3. Validasyon Kontrolleri
   ↓
   ├─ Fatura No boş mu?
   ├─ Fatura Tarihi geçerli mi?
   ├─ Malzeme kodları Logo'da var mı?
   ├─ Satır toplamlar sıfır mı?
   └─ Email adresi geçerli mi?
   ↓
4. Email → Cari Kodu Eşleştirme
   ↓
   • Logo ARPS tablosundan EMAIL ile cari bul
   • Bulunamazsa → HATA
   ↓
5. Logo'da Fatura Var mı Kontrol
   ↓
   • Aynı fatura no + cari kodu varsa → ATLA
   ↓
6. Fatura Dönüştürme
   ↓
   • Malzeme CARDTYPE kontrolü
   • CARDTYPE=1 varsa → invoiceType=1 (Satınalma)
   • Hepsi CARDTYPE≠1 ise → invoiceType=4 (Hizmet)
   • KDV hesaplama
   • JSON formatına dönüştürme
   ↓
7. JSON Kayıt (Request)
   ↓
   • JSONLog/ klasörüne request JSON kaydet
   ↓
8. Logo'ya POST
   ↓
   • /v2.0/invoices/purchase?invoiceType={type}
   ↓
9. Sonuç
   ↓
   ├─ ✅ Başarılı
   │  ├─ Logo fiş no al
   │  ├─ JSON'a response ekle
   │  ├─ SQLite'a log
   │  └─ (Opsiyonel) Smartsheet'e Logo Reference yaz
   │
   └─ ❌ Hata
      ├─ Hata mesajını JSON'a ekle
      ├─ HATALI_ prefix ile kaydet
      └─ SQLite'a hata logu
```

---

## 🧩 Fatura Tipi Belirleme Mantığı ✨

```csharp
// Her malzeme kaleminin CARDTYPE'ı kontrol edilir
HashSet<int> cardTypes = new HashSet<int>();

foreach (var item in group.Items)
{
    var malzemeResult = await GetMalzemeCardTypeAsync(item.MalzemeKodu);
    cardTypes.Add(malzemeResult.CardType.Value);
}

// Fatura tipi belirleme
int invoiceType = cardTypes.Contains(1) ? 1 : 4;

// invoiceType = 1 → SATINALMA FİŞİ (en az 1 malzeme varsa)
// invoiceType = 4 → HİZMET FİŞİ (sadece hizmet kalemleri varsa)
```

---

## 🎯 Kullanım İpuçları

### Genel
1. **Token Süresi:** Token'lar 1 saat geçerlidir, sistem otomatik yeniler
2. **Toplu İşlem:** 500+ fatura için batch işlem önerilir
3. **Duplicate Kontrol:** Her aktarım öncesi mutlaka duplicate kontrol yapılır
4. **Bağlantı Testi:** İlk kurulumda test butonlarını kullanın
5. **Log Takibi:** Hata durumunda SQLite loglarını inceleyin

### Expenses Modülü İçin ✨
6. **Email Adresi:** Kayıt eden kullanıcının email adresi Logo ARPS tablosunda tanımlı olmalı
7. **Malzeme Kodları:** Malzeme Listesi kolonu "MALKOD---Açıklama" formatında olabilir (--- ile split edilir)
8. **3 Onay:** Muhasebe, Yönetici ve Supervisor onayı olmadan fatura oluşturulmaz
9. **JSON Logları:** Başarılı ve hatalı tüm istekler JSONLog/ klasöründe saklanır
10. **KDV Hesaplama:** KDV="VAR" ve KDV Oranı>0 ise otomatik hesaplanır
11. **Gruplama:** Aynı fatura no + kayıt eden bir faturada birleştirilir
12. **CARDTYPE:** Malzeme kartı tipi otomatik tespit edilir (1=Malzeme, diğer=Hizmet)

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

## 🎉 Güncellemeler

### v2.0 (Şubat 2025) ✨
- **Yeni:** Smartsheet'ten Logo'ya otomatik fatura oluşturma
- **Yeni:** Expenses Sheet entegrasyonu
- **Yeni:** 3 aşamalı onay sistemi (Muhasebe/Yönetici/Supervisor)
- **Yeni:** Email bazlı cari eşleştirme
- **Yeni:** Malzeme kodu validasyonu
- **Yeni:** JSON log sistemi (başarılı/hatalı faturalar)
- **Yeni:** Duplicate kontrol (Logo'da fatura kontrolü)
- **İyileştirme:** Gruplama mantığı (tarih gruplama anahtarından çıkarıldı)
- **İyileştirme:** KDV hesaplama otomasyonu
- **İyileştirme:** CARDTYPE bazlı fatura tipi belirleme

---

<p align="center">
  <img src="https://img.shields.io/badge/.NET-Framework%204.7.2+-purple?logo=dotnet" alt="dotnet" />
  <img src="https://img.shields.io/badge/Smartsheet-API-blue?logo=smartsheet" alt="smartsheet" />
  <img src="https://img.shields.io/badge/Logo-Bulut%20ERP-green" alt="logo" />
  <img src="https://img.shields.io/badge/SQLite-Database-orange?logo=sqlite" alt="sqlite" />
  <img src="https://img.shields.io/badge/License-System-red" alt="license" />
</p>
