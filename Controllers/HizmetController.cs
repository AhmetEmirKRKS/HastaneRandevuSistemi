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
    }
}
