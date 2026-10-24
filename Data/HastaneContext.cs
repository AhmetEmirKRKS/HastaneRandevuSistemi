using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kullanicilar;
using HastaneRandevuSistemi.Models.Kurumlar;
using HastaneRandevuSistemi.Models.Randevular;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Data
{
    // Veritabanı ile sınıflarımız arasındaki köprü. Her DbSet bir tabloya karşılık gelir.
    public class HastaneContext : DbContext
    {
        public HastaneContext(DbContextOptions<HastaneContext> options) : base(options)
        {
        }

        public DbSet<Kurum> Kurumlar { get; set; }
        public DbSet<Sube> Subeler { get; set; }
        public DbSet<Bolum> Bolumler { get; set; }
        public DbSet<CalismaSaati> CalismaSaatleri { get; set; }

        // Kalıtım: Hasta, Doktor, SaglikPersoneli ve Yonetici hepsi tek "Kullanicilar" tablosunda tutulur
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Hasta> Hastalar { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<Doktor> Doktorlar { get; set; }
        public DbSet<SaglikPersoneli> SaglikPersonelleri { get; set; }
        public DbSet<Yonetici> Yoneticiler { get; set; }
        public DbSet<UygunlukDilimi> UygunlukDilimleri { get; set; }

        // Muayene, LaboratuvarTesti ve GoruntulemeHizmeti tek "Hizmetler" tablosunda tutulur
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Muayene> Muayeneler { get; set; }
        public DbSet<LaboratuvarTesti> LaboratuvarTestleri { get; set; }
        public DbSet<GoruntulemeHizmeti> GoruntulemeHizmetleri { get; set; }

        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aynı e-posta ile iki kullanıcı olamaz
            modelBuilder.Entity<Kullanici>().HasIndex(k => k.Email).IsUnique();
        }
    }
}
