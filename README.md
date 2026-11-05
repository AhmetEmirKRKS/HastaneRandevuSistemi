# Hastane / Klinik Randevu Sistemi

Nesneye Dayalı Programlama dersi proje ödevi (Seçenek A).

Tek veya çok şubeli hastane/klinikler için web tabanlı randevu sistemi. Yönetici kurumu, hizmetleri ve çalışanları tanımlar; hasta uygun çalışan ve saati seçerek randevu alır; doktor veya sağlık personeli randevuyu onaylar ya da reddeder.

**Proje ekibi:** Ahmet Emir Karakaş, Ahmet Bektaş

## Kullanılan Teknolojiler

- C# / .NET 10
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- Bootstrap

## Çalıştırma

1. Projeyi Visual Studio ile açın (`HastaneRandevuSistemi.slnx`).
2. **F5** ile çalıştırın.

İlk çalıştırmada `hastane.db` veritabanı otomatik oluşturulur ve örnek veriler eklenir. Ek kurulum gerekmez.

## Test Hesapları

Tüm hesapların şifresi: `123456`

| Rol | E-posta |
|---|---|
| Yönetici | yonetici@hastane.com |
| Doktor | ayse@hastane.com |
| Doktor | mehmet@hastane.com |
| Sağlık Personeli | ali@hastane.com |
| Sağlık Personeli | zeynep@hastane.com |
| Hasta | hasta@hastane.com |

Yeni hasta hesabı "Kayıt Ol" sayfasından oluşturulabilir.

## Özellikler

### Yönetici
- Hastane/klinik ve şubelerini tanımlama (çok şubeli yapı)
- Her şubenin bölümlerini ve çalışma saatlerini tanımlama
- Sağlık hizmetlerini (muayene, laboratuvar, radyoloji) süre ve ücretiyle tanımlama
- Doktor ve sağlık personeli kaydı; uzmanlık alanı/görev ve verdiği hizmetleri belirleme
- Çalışanların uygunluk zaman dilimlerini tanımlama
- Tüm kayıtları düzenleme ve silme (bağlı kaydı olanlar silinemez)

### Hasta
- Hizmet, çalışan, tarih ve saat seçerek randevu alma (isteğe bağlı not ile)
- Randevularını ve onay durumlarını görme
- Henüz gerçekleşmemiş randevusunu iptal etme

### Doktor / Sağlık Personeli
- Kendisine alınan randevuları görme
- Randevuları onaylama veya red nedeni yazarak reddetme
- Saati gelmiş onaylı randevuyu "Tamamlandı" olarak işaretleme

### Randevu Kuralları
- Çalışanın uygunluk saatleri, şubesinin çalışma saatleri içinde olmak zorundadır.
- Hasta sadece çalışanın uygunluk saatleri içine randevu alabilir.
- **Çakışma kontrolü:** Çalışanın veya hastanın aynı saatte başka aktif randevusu varsa randevu oluşturulamaz.
- Randevu detayları (işlem, süre, ücret, çalışan, tarih/saat) kaydedilir. Hizmet ücreti sonradan değişse bile randevu, alındığı andaki ücreti saklar.
- Randevu durumları:
  - **Onay Bekliyor** → Onaylandı, Reddedildi veya İptal Edildi
  - **Onaylandı** → Tamamlandı veya İptal Edildi

## Nesneye Dayalı Programlama Kavramları

### Kalıtım ve Soyutlama
```
Kullanici (abstract)
├── Hasta
├── Yonetici
└── Calisan (abstract)
    ├── Doktor
    └── SaglikPersoneli

Hizmet (abstract)
├── Muayene
├── LaboratuvarTesti
└── GoruntulemeHizmeti
```
- `Kullanici`, `Calisan` ve `Hizmet` soyut (abstract) sınıflardır; doğrudan nesneleri oluşturulamaz.
- Ad, soyad, e-posta gibi ortak alanlar `Kullanici` sınıfında bir kez tanımlanır.
- Dosyalar: `Models/Kullanicilar/`, `Models/Hizmetler/`

### Çok Biçimlilik (Polymorphism)
- `Kullanici.Rol` (abstract) ve `Kullanici.GorunenAd` (virtual): her alt sınıf kendi rolünü bildirir ve ekranda farklı görünür (ör. Doktor → "Dr. Ayşe Yılmaz", Sağlık Personeli → "Ali Kaya (Laborant)").
- `Hizmet.UcretHesapla()` (virtual): `GoruntulemeHizmeti` bu metodu ezerek (override) kontrastlı çekimlerde ek ücret ekler. Randevu oluşturulurken hangi hizmet türü olursa olsun aynı metod çağrılır, doğru hesaplama otomatik çalışır.

### Kapsülleme (Encapsulation)
- `Randevu` sınıfının tüm alanları `private set`'tir; randevu dışarıdan doğrudan değiştirilemez.
- Randevu sadece kurucu metodla (constructor) oluşturulur; burada "çalışan bu hizmeti veriyor mu?" ve "geçmiş tarih mi?" kontrolleri yapılır.
- Durum sadece `Onayla()`, `Reddet()`, `IptalEt()` ve `Tamamla()` metodlarıyla, kurallara uyularak değişir (ör. reddedilmiş randevu onaylanamaz).
- Çakışma kuralı `CakisiyorMu()`, uygunluk kuralı `Calisan.UygunMu()` metodlarındadır.
- Dosya: `Models/Randevular/Randevu.cs`

## Proje Yapısı

```
Controllers/   Sayfaların iş akışı (Hesap, Kurum, Hizmet, Calisan, Randevu, Panel)
Models/        Sınıflar (Kullanicilar, Kurumlar, Hizmetler, Randevular, Enums)
Views/         Sayfalar (Razor)
Data/          Veritabanı bağlantısı, örnek veriler, şifreleme
```
