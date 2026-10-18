using HastaneRandevuSistemi.Models.Enums;

namespace HastaneRandevuSistemi.Models.Kullanicilar
{
    // abstract: doğrudan "Kullanici" oluşturulamaz, her kullanıcı bir alt sınıf olmak zorunda
    public abstract class Kullanici
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SifreHash { get; set; } = string.Empty;

        // Her alt sınıf kendi rolünü bildirmek zorunda
        public abstract KullaniciRolu Rol { get; }

        // Alt sınıflar ekranda nasıl görüneceğini değiştirebilir
        public virtual string GorunenAd => $"{Ad} {Soyad}";
    }
}
