using HastaneRandevuSistemi.Models.Hizmetler;
using HastaneRandevuSistemi.Models.Kullanicilar;
using HastaneRandevuSistemi.Models.Kurumlar;

namespace HastaneRandevuSistemi.Data
{
    // Veritabanı boşsa uygulamayı denemek için örnek veriler ekler
    public static class OrnekVeriler
    {
        public static void Ekle(HastaneContext db)
        {
            if (db.Kurumlar.Any())
                return;

            // Kurum ve şubeler (çok şubeli yapı)
            var kurum = new Kurum { Ad = "Sağlık Hastanesi" };
            var merkez = new Sube { Ad = "Merkez Şube", Kurum = kurum };
            var kadikoy = new Sube { Ad = "Kadıköy Şube", Kurum = kurum };

            // Hafta içi 08:00 - 18:00 çalışma saatleri
            foreach (var sube in new[] { merkez, kadikoy })
            {
                for (var gun = DayOfWeek.Monday; gun <= DayOfWeek.Friday; gun++)
                {
                    sube.CalismaSaatleri.Add(new CalismaSaati
                    {
                        Gun = gun,
                        Acilis = new TimeOnly(8, 0),
                        Kapanis = new TimeOnly(18, 0)
                    });
                }
            }

            // Bölümler
            var dahiliye = new Bolum { Ad = "Dahiliye", Sube = merkez };
            var laboratuvar = new Bolum { Ad = "Laboratuvar", Sube = merkez };
            var radyoloji = new Bolum { Ad = "Radyoloji", Sube = merkez };
            var kardiyoloji = new Bolum { Ad = "Kardiyoloji", Sube = kadikoy };

            // Hizmetler (süre dakika, ücret TL)
            var genelMuayene = new Muayene { Ad = "Genel Muayene", SureDakika = 20, Ucret = 500, Bolum = dahiliye };
            var kardiyolojiMuayene = new Muayene { Ad = "Kardiyoloji Muayenesi", SureDakika = 20, Ucret = 700, Bolum = kardiyoloji };
            var kanTahlili = new LaboratuvarTesti { Ad = "Tam Kan Sayımı", SureDakika = 10, Ucret = 150, Bolum = laboratuvar };
            var rontgen = new GoruntulemeHizmeti { Ad = "Akciğer Röntgeni", SureDakika = 15, Ucret = 400, Bolum = radyoloji };
            var mr = new GoruntulemeHizmeti { Ad = "Kontrastlı MR", SureDakika = 30, Ucret = 2000, KontrastliMi = true, KontrastUcreti = 500, Bolum = radyoloji };

            // Çalışanlar ve verdikleri hizmetler
            var doktor1 = new Doktor { Ad = "Ayşe", Soyad = "Yılmaz", Email = "ayse@hastane.com", UzmanlikAlani = "İç Hastalıkları", Bolum = dahiliye };
            doktor1.Hizmetler.Add(genelMuayene);

            var doktor2 = new Doktor { Ad = "Mehmet", Soyad = "Demir", Email = "mehmet@hastane.com", UzmanlikAlani = "Kardiyoloji", Bolum = kardiyoloji };
            doktor2.Hizmetler.Add(kardiyolojiMuayene);

            var laborant = new SaglikPersoneli { Ad = "Ali", Soyad = "Kaya", Email = "ali@hastane.com", Gorev = "Laborant", Bolum = laboratuvar };
            laborant.Hizmetler.Add(kanTahlili);

            var teknisyen = new SaglikPersoneli { Ad = "Zeynep", Soyad = "Şahin", Email = "zeynep@hastane.com", Gorev = "Radyoloji Teknikeri", Bolum = radyoloji };
            teknisyen.Hizmetler.Add(rontgen);
            teknisyen.Hizmetler.Add(mr);

            var calisanlar = new Calisan[] { doktor1, doktor2, laborant, teknisyen };

            // Her çalışan hafta içi 09:00 - 12:00 ve 13:00 - 17:00 arası randevu kabul eder
            foreach (var calisan in calisanlar)
            {
                calisan.SifreHash = Sifreleme.Hashle("123456");

                for (var gun = DayOfWeek.Monday; gun <= DayOfWeek.Friday; gun++)
                {
                    calisan.UygunlukDilimleri.Add(new UygunlukDilimi { Gun = gun, Baslangic = new TimeOnly(9, 0), Bitis = new TimeOnly(12, 0) });
                    calisan.UygunlukDilimleri.Add(new UygunlukDilimi { Gun = gun, Baslangic = new TimeOnly(13, 0), Bitis = new TimeOnly(17, 0) });
                }
            }

            // Yönetici ve hasta
            var yonetici = new Yonetici { Ad = "Sistem", Soyad = "Yöneticisi", Email = "yonetici@hastane.com", SifreHash = Sifreleme.Hashle("123456") };
            var hasta = new Hasta { Ad = "Can", Soyad = "Öztürk", Email = "hasta@hastane.com", SifreHash = Sifreleme.Hashle("123456") };

            // Şube, bölüm ve hizmetler çalışanlar üzerinden zincirleme olarak eklenir
            db.Kurumlar.Add(kurum);
            db.Calisanlar.AddRange(calisanlar);
            db.Kullanicilar.Add(yonetici);
            db.Kullanicilar.Add(hasta);
            db.SaveChanges();
        }
    }
}
