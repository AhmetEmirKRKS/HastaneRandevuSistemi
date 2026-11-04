using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Kurumlar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Hastane/klinik, şube, bölüm ve çalışma saati yönetimi. Sadece yönetici erişebilir.
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

        // ---------- Kurum ----------

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

        public IActionResult KurumDuzenle(int id)
        {
            var kurum = _db.Kurumlar.Find(id);
            if (kurum == null)
                return NotFound();

            return AdDuzenleSayfasi("Kurum Düzenle", "KurumDuzenle", kurum.Id, kurum.Ad, "Index", null);
        }

        [HttpPost]
        public IActionResult KurumDuzenle(int id, string ad)
        {
            var kurum = _db.Kurumlar.Find(id);
            if (kurum != null && !string.IsNullOrWhiteSpace(ad))
            {
                kurum.Ad = ad;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult KurumSil(int id)
        {
            if (_db.Subeler.Any(s => s.KurumId == id))
            {
                TempData["Hata"] = "Şubesi olan bir kurum silinemez. Önce şubeleri silin.";
            }
            else
            {
                _db.Kurumlar.Remove(_db.Kurumlar.Find(id)!);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // ---------- Şube ----------

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

        public IActionResult SubeDuzenle(int id)
        {
            var sube = _db.Subeler.Find(id);
            if (sube == null)
                return NotFound();

            return AdDuzenleSayfasi("Şube Düzenle", "SubeDuzenle", sube.Id, sube.Ad, "Sube", sube.Id);
        }

        [HttpPost]
        public IActionResult SubeDuzenle(int id, string ad)
        {
            var sube = _db.Subeler.Find(id);
            if (sube != null && !string.IsNullOrWhiteSpace(ad))
            {
                sube.Ad = ad;
                _db.SaveChanges();
            }
            return RedirectToAction("Sube", new { id });
        }

        [HttpPost]
        public IActionResult SubeSil(int id)
        {
            if (_db.Bolumler.Any(b => b.SubeId == id))
            {
                TempData["Hata"] = "Bölümü olan bir şube silinemez. Önce bölümleri silin.";
                return RedirectToAction("Sube", new { id });
            }

            // Şubenin çalışma saatleri de onunla birlikte silinir
            _db.Subeler.Remove(_db.Subeler.Find(id)!);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ---------- Bölüm ----------

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

        public IActionResult BolumDuzenle(int id)
        {
            var bolum = _db.Bolumler.Find(id);
            if (bolum == null)
                return NotFound();

            return AdDuzenleSayfasi("Bölüm Düzenle", "BolumDuzenle", bolum.Id, bolum.Ad, "Sube", bolum.SubeId);
        }

        [HttpPost]
        public IActionResult BolumDuzenle(int id, string ad)
        {
            var bolum = _db.Bolumler.Find(id)!;
            if (!string.IsNullOrWhiteSpace(ad))
            {
                bolum.Ad = ad;
                _db.SaveChanges();
            }
            return RedirectToAction("Sube", new { id = bolum.SubeId });
        }

        [HttpPost]
        public IActionResult BolumSil(int id)
        {
            var bolum = _db.Bolumler.Find(id)!;

            if (_db.Hizmetler.Any(h => h.BolumId == id) || _db.Calisanlar.Any(c => c.BolumId == id))
            {
                TempData["Hata"] = "Hizmeti veya çalışanı olan bir bölüm silinemez.";
            }
            else
            {
                _db.Bolumler.Remove(bolum);
                _db.SaveChanges();
            }
            return RedirectToAction("Sube", new { id = bolum.SubeId });
        }

        // ---------- Çalışma Saati ----------

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

        [HttpPost]
        public IActionResult CalismaSaatiSil(int id)
        {
            var saat = _db.CalismaSaatleri.Find(id)!;
            _db.CalismaSaatleri.Remove(saat);
            _db.SaveChanges();
            return RedirectToAction("Sube", new { id = saat.SubeId });
        }

        // Kurum, şube ve bölüm düzenleme için ortak "ad değiştir" sayfası
        private IActionResult AdDuzenleSayfasi(string baslik, string action, int id, string ad, string geriAction, int? geriId)
        {
            ViewBag.Baslik = baslik;
            ViewBag.Action = action;
            ViewBag.Id = id;
            ViewBag.Ad = ad;
            ViewBag.GeriAction = geriAction;
            ViewBag.GeriId = geriId;
            return View("AdDuzenle");
        }
    }
}
