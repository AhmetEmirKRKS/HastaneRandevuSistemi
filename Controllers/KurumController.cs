using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Kurumlar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Hastane/klinik, şube, bölüm ve çalışma saati tanımlama. Sadece yönetici erişebilir.
    [Authorize(Roles = "Yonetici")]
    public class KurumController : Controller
    {
        private readonly HastaneContext _db;

        public KurumController(HastaneContext db)
        {
            _db = db;
        }

        // Tüm kurumlar ve şubeleri
        public IActionResult Index()
        {
            var kurumlar = _db.Kurumlar.Include(k => k.Subeler).ToList();
            return View(kurumlar);
        }

        [HttpPost]
        public IActionResult KurumEkle(string ad)
        {
            if (!string.IsNullOrWhiteSpace(ad))
            {
                _db.Kurumlar.Add(new Kurum { Ad = ad });
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SubeEkle(int kurumId, string ad)
        {
            if (!string.IsNullOrWhiteSpace(ad))
            {
                _db.Subeler.Add(new Sube { KurumId = kurumId, Ad = ad });
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Bir şubenin bölümleri ve çalışma saatleri
        public IActionResult Sube(int id)
        {
            var sube = _db.Subeler
                .Include(s => s.Kurum)
                .Include(s => s.Bolumler)
                .Include(s => s.CalismaSaatleri)
                .FirstOrDefault(s => s.Id == id);

            if (sube == null)
                return NotFound();

            return View(sube);
        }

        [HttpPost]
        public IActionResult BolumEkle(int subeId, string ad)
        {
            if (!string.IsNullOrWhiteSpace(ad))
            {
                _db.Bolumler.Add(new Bolum { SubeId = subeId, Ad = ad });
                _db.SaveChanges();
            }
            return RedirectToAction("Sube", new { id = subeId });
        }

        [HttpPost]
        public IActionResult CalismaSaatiEkle(int subeId, DayOfWeek gun, TimeOnly acilis, TimeOnly kapanis)
        {
            if (acilis >= kapanis)
            {
                TempData["Hata"] = "Açılış saati kapanış saatinden önce olmalıdır.";
            }
            else if (_db.CalismaSaatleri.Any(c => c.SubeId == subeId && c.Gun == gun))
            {
                TempData["Hata"] = "Bu gün için çalışma saati zaten tanımlı.";
            }
            else
            {
                _db.CalismaSaatleri.Add(new CalismaSaati { SubeId = subeId, Gun = gun, Acilis = acilis, Kapanis = kapanis });
                _db.SaveChanges();
            }
            return RedirectToAction("Sube", new { id = subeId });
        }
    }
}
