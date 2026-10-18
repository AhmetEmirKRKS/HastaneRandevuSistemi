using HastaneRandevuSistemi.Models.Enums;
using HastaneRandevuSistemi.Models.Randevular;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    public class Hasta : Kullanici
    {
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public override KullaniciRolu Rol => KullaniciRolu.Hasta;
    }
}
