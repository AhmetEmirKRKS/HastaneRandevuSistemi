using HastaneRandevuSistemi.Models.Kullanicilar;
using HastaneRandevuSistemi.Models.Kurumlar;

namespace HastaneRandevuSistemi.Models.Hizmetler
{
    // Sağlık hizmetlerinin ortak atası (genel muayene, laboratuvar, radyoloji)
    public abstract class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int SureDakika { get; set; }
        public decimal Ucret { get; set; }

        public int BolumId { get; set; }
        public Bolum Bolum { get; set; } = null!;

        public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();

        public abstract string HizmetTuru { get; }

        // Alt sınıflar ücreti kendi kuralına göre hesaplayabilir
        public virtual decimal UcretHesapla() => Ucret;
    }
}
