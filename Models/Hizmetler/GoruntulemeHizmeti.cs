namespace HastaneRandevuSistemi.Models.Hizmetler
{
    public class GoruntulemeHizmeti : Hizmet
    {
        public bool KontrastliMi { get; set; }
        public decimal KontrastUcreti { get; set; }

        public override string HizmetTuru => "Radyoloji";

        // Kontrastlı çekimde ek ücret eklenir
        public override decimal UcretHesapla() =>
            KontrastliMi ? Ucret + KontrastUcreti : Ucret;
    }
}
