using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kurumlar;
using HastaneRandevuSistemi.Models.Randevular;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    // Randevu verebilen çalışanların (Doktor, SaglikPersoneli) ortak atası
    public abstract class Calisan : Kullanici
    {
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; } = null!;

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<UygunlukDilimi> UygunlukDilimleri { get; set; } = new List<UygunlukDilimi>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public bool HizmetVerebilirMi(Hizmet hizmet) => Hizmetler.Any(h => h.Id == hizmet.Id);

        // Hastalar sadece çalışanın uygunluk dilimlerine randevu alabilir
        public bool UygunMu(DateTime baslangic, DateTime bitis) =>
            UygunlukDilimleri.Any(d => d.Kapsar(baslangic, bitis));
    }
}
