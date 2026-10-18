using HastaneRandevuSistemi.Models.Enums;
using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kullanicilar;

namespace HastaneRandevuSistemi.Models.Randevular
{
    // Alanlar "private set": randevu dışarıdan doğrudan değiştirilemez,
    // durum sadece Onayla() ve Reddet() metodlarıyla değişir (kapsülleme).
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

        // Entity Framework'ün veritabanından nesne oluşturabilmesi için
        private Randevu()
        {
        }

        public Randevu(Hasta hasta, Calisan calisan, Hizmet hizmet, DateTime baslangic)
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
        }

        // Reddedilen randevular takvimde yer kaplamaz
        public bool CakisiyorMu(DateTime baslangic, DateTime bitis) =>
            Durum != RandevuDurumu.Reddedildi && Baslangic < bitis && baslangic < Bitis;

        public void Onayla()
        {
            if (Durum != RandevuDurumu.Beklemede)
                throw new InvalidOperationException("Sadece bekleyen randevular onaylanabilir.");

            Durum = RandevuDurumu.Onaylandi;
        }

        public void Reddet()
        {
            if (Durum != RandevuDurumu.Beklemede)
                throw new InvalidOperationException("Sadece bekleyen randevular reddedilebilir.");

            Durum = RandevuDurumu.Reddedildi;
        }
    }
}
