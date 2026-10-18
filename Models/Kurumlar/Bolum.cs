using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kullanicilar;

namespace HastaneRandevuSistemi.Models.Kurumlar
{
    public class Bolum
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;   // ör. "Kardiyoloji", "Radyoloji"

        public int SubeId { get; set; }
        public Sube Sube { get; set; } = null!;

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();
    }
}
