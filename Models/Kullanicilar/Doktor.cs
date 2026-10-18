using HastaneRandevuSistemi.Models.Enums;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    public class Doktor : Calisan
    {
        public string UzmanlikAlani { get; set; } = string.Empty;

        public override KullaniciRolu Rol => KullaniciRolu.Doktor;

        public override string GorunenAd => $"Dr. {Ad} {Soyad}";
    }
}
