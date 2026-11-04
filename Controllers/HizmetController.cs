using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Hizmetler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Sağlık hizmetlerinin (muayene, laboratuvar, radyoloji) tanımlanması. Sadece yönetici erişebilir.
    [Authorize(Roles = "Yonetici")]
    public class HizmetController : Controller
    {
        private readonly HastaneContext _db;

        public HizmetController(HastaneContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.Bolumler = _db.Bolumler.Include(b => b.Sube).ToList();

            var hizmetler = _db.Hizmetler
                .Include(h => h.Bolum).ThenInclude(b => b.Sube)
                .ToList();
            return View(hizmetler);
        }

        [HttpPost]
        public IActionResult Ekle(string tur, string ad, int sureDakika, decimal ucret, int bolumId,
                                  bool kontrastliMi, decimal kontrastUcreti)
        {
            if (string.IsNullOrWhiteSpace(ad) || sureDakika <= 0 || ucret < 0)
            {
                TempData["Hata"] = "Lütfen ad, süre ve ücret bilgilerini doğru giriniz.";
                return RedirectToAction("Index");
            }

            // Seçilen türe göre doğru alt sınıftan nesne oluşturulur
            Hizmet hizmet = tur switch
            {
                "Laboratuvar" => new LaboratuvarTesti(),
                "Radyoloji" => new GoruntulemeHizmeti { KontrastliMi = kontrastliMi, KontrastUcreti = kontrastUcreti },
                _ => new Muayene()
            };

            hizmet.Ad = ad;
            hizmet.SureDakika = sureDakika;
            hizmet.Ucret = ucret;
            hizmet.BolumId = bolumId;

            _db.Hizmetler.Add(hizmet);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Duzenle(int id)
        {
            var hizmet = _db.Hizmetler.Find(id);
            if (hizmet == null)
                return NotFound();

            ViewBag.Bolumler = _db.Bolumler.Include(b => b.Sube).ToList();
            return View(hizmet);
        }

        // Hizmetin türü değiştirilemez; ad, süre, ücret ve bölüm değiştirilebilir.
        // Ücret değişse bile eski randevuların ücreti değişmez (randevu kendi ücretini saklar).
        [HttpPost]
        public IActionResult Duzenle(int id, string ad, int sureDakika, decimal ucret, int bolumId,
                                     bool kontrastliMi, decimal kontrastUcreti)
        {
            var hizmet = _db.Hizmetler.Find(id);
            if (hizmet == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(ad) || sureDakika <= 0 || ucret < 0)
            {
                TempData["Hata"] = "Lütfen ad, süre ve ücret bilgilerini doğru giriniz.";
                return RedirectToAction("Duzenle", new { id });
            }

            hizmet.Ad = ad;
            hizmet.SureDakika = sureDakika;
            hizmet.Ucret = ucret;
            hizmet.BolumId = bolumId;

            if (hizmet is GoruntulemeHizmeti goruntuleme)
            {
                goruntuleme.KontrastliMi = kontrastliMi;
                goruntuleme.KontrastUcreti = kontrastUcreti;
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Sil(int id)
        {
            if (_db.Randevular.Any(r => r.HizmetId == id))
            {
                TempData["Hata"] = "Randevusu olan bir hizmet silinemez.";
            }
            else
            {
                _db.Hizmetler.Remove(_db.Hizmetler.Find(id)!);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
