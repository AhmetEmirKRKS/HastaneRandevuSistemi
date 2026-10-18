using HastaneRandevuSistemi.Models.Enums;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    public class SaglikPersoneli : Calisan
    {
        public string Gorev { get; set; } = string.Empty;   // ör. "Hemşire", "Laborant"

        public override KullaniciRolu Rol => KullaniciRolu.SaglikPersoneli;

        public override string GorunenAd => $"{Ad} {Soyad} ({Gorev})";
    }
}
