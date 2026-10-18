using HastaneRandevuSistemi.Models.Enums;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    public class Yonetici : Kullanici
    {
        public override KullaniciRolu Rol => KullaniciRolu.Yonetici;
    }
}
