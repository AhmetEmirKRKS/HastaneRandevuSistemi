using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Kullanicilar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Doktor ve sağlık personeli kaydı. Sadece yönetici erişebilir.
    [Authorize(Roles = "Yonetici")]
    public class CalisanController : Controller
    {
        private readonly HastaneContext _db;

        public CalisanController(HastaneContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.Bolumler = _db.Bolumler.Include(b => b.Sube).ToList();
            ViewBag.Hizmetler = _db.Hizmetler.Include(h => h.Bolum).ToList();

            var calisanlar = _db.Calisanlar
                .Include(c => c.Bolum).ThenInclude(b => b.Sube)
                .Include(c => c.Hizmetler)
                .ToList();
            return View(calisanlar);
        }

        [HttpPost]
        public IActionResult Ekle(string tur, string ad, string soyad, string email, string sifre,
                                  int bolumId, string uzmanlikVeyaGorev, List<int> hizmetIdleri)
        {
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
            {
                TempData["Hata"] = "Lütfen tüm alanları doldurunuz.";
                return RedirectToAction("Index");
            }

            if (_db.Kullanicilar.Any(k => k.Email == email))
            {
                TempData["Hata"] = "Bu e-posta zaten kayıtlı.";
                return RedirectToAction("Index");
            }

            // Seçilen türe göre Doktor veya SaglikPersoneli nesnesi oluşturulur
            Calisan calisan = tur == "Doktor"
                ? new Doktor { UzmanlikAlani = uzmanlikVeyaGorev }
                : new SaglikPersoneli { Gorev = uzmanlikVeyaGorev };

            calisan.Ad = ad;
            calisan.Soyad = soyad;
            calisan.Email = email;
            calisan.SifreHash = Sifreleme.Hashle(sifre);
            calisan.BolumId = bolumId;

            // Çalışanın verdiği hizmetler
            foreach (var hizmet in _db.Hizmetler.Where(h => hizmetIdleri.Contains(h.Id)))
            {
                calisan.Hizmetler.Add(hizmet);
            }

            _db.Calisanlar.Add(calisan);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
