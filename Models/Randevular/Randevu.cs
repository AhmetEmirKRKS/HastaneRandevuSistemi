using HastaneRandevuSistemi.Models.Enums;
using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kullanicilar;

namespace HastaneRandevuSistemi.Models.Randevular
{
    // Alanlar "private set": randevu dışarıdan doğrudan değiştirilemez,
    // durum sadece Onayla(), Reddet(), IptalEt() ve Tamamla() metodlarıyla değişir (kapsülleme).
    public class Randevu
    {
        public int Id { get; private set; }

        public int HastaId { get; private set; }
        public Hasta Hasta { get; private set; } = null!;

        public int CalisanId { get; private set; }
        public Calisan Calisan { get; private set; } = null!;

        public int HizmetId { get; private set; }
        public Hizmet Hizmet { get; private set; } = null!;

        public DateTime Baslangic { get; private set; }
        public DateTime Bitis { get; private set; }
        public decimal Ucret { get; private set; }
        public RandevuDurumu Durum { get; private set; }

        public string? HastaNotu { get; private set; }
        public string? RedNedeni { get; private set; }

        // Ekranda gösterilecek Türkçe durum
        public string DurumMetni => Durum switch
        {
            RandevuDurumu.Onaylandi => "Onaylandı",
            RandevuDurumu.Reddedildi => "Reddedildi",
            RandevuDurumu.IptalEdildi => "İptal Edildi",
            RandevuDurumu.Tamamlandi => "Tamamlandı",
            _ => "Onay Bekliyor"
        };

        // Sadece bekleyen veya onaylanmış randevular takvimde yer kaplar
        public bool AktifMi => Durum == RandevuDurumu.Beklemede || Durum == RandevuDurumu.Onaylandi;

        // Entity Framework'ün veritabanından nesne oluşturabilmesi için
        private Randevu()
        {
        }

        public Randevu(Hasta hasta, Calisan calisan, Hizmet hizmet, DateTime baslangic, string? hastaNotu)
        {
            if (!calisan.HizmetVerebilirMi(hizmet))
                throw new InvalidOperationException("Seçilen çalışan bu hizmeti vermiyor.");

            if (baslangic <= DateTime.Now)
                throw new InvalidOperationException("Geçmiş bir zamana randevu alınamaz.");

            Hasta = hasta;
            HastaId = hasta.Id;
            Calisan = calisan;
            CalisanId = calisan.Id;
            Hizmet = hizmet;
            HizmetId = hizmet.Id;

            Baslangic = baslangic;
            Bitis = baslangic.AddMinutes(hizmet.SureDakika);
            Ucret = hizmet.UcretHesapla();   // hizmetin türüne göre doğru metod çalışır
            Durum = RandevuDurumu.Beklemede;
            HastaNotu = hastaNotu;
        }

        public bool CakisiyorMu(DateTime baslangic, DateTime bitis) =>
            AktifMi && Baslangic < bitis && baslangic < Bitis;

        public void Onayla()
        {
            if (Durum != RandevuDurumu.Beklemede)
                throw new InvalidOperationException("Sadece bekleyen randevular onaylanabilir.");

            Durum = RandevuDurumu.Onaylandi;
        }

        public void Reddet(string neden)
        {
            if (Durum != RandevuDurumu.Beklemede)
                throw new InvalidOperationException("Sadece bekleyen randevular reddedilebilir.");

            if (string.IsNullOrWhiteSpace(neden))
                throw new InvalidOperationException("Red nedeni yazılmalıdır.");

            Durum = RandevuDurumu.Reddedildi;
            RedNedeni = neden;
        }

        public void IptalEt()
        {
            if (!AktifMi)
                throw new InvalidOperationException("Bu randevu iptal edilemez.");

            if (Baslangic <= DateTime.Now)
                throw new InvalidOperationException("Saati gelmiş veya geçmiş bir randevu iptal edilemez.");

            Durum = RandevuDurumu.IptalEdildi;
        }

        public void Tamamla()
        {
            if (Durum != RandevuDurumu.Onaylandi)
                throw new InvalidOperationException("Sadece onaylanmış randevular tamamlanabilir.");

            if (Baslangic > DateTime.Now)
                throw new InvalidOperationException("Saati gelmemiş bir randevu tamamlanamaz.");

            Durum = RandevuDurumu.Tamamlandi;
        }
    }
}
