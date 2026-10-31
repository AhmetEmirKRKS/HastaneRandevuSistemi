using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Randevular;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Hastaların randevu alma işlemleri
    [Authorize(Roles = "Hasta")]
    public class RandevuController : Controller
    {
        private readonly HastaneContext _db;

        public RandevuController(HastaneContext db)
        {
            _db = db;
        }

        // 1. adım: hizmet seçilir. 2. adım (hizmetId gelince): çalışan, tarih ve saat seçilir.
        public IActionResult Al(int? hizmetId)
        {
            ViewBag.Hizmetler = _db.Hizmetler
                .Include(h => h.Bolum).ThenInclude(b => b.Sube)
                .ToList();

            if (hizmetId != null)
            {
                ViewBag.SeciliHizmet = _db.Hizmetler.Find(hizmetId);
                ViewBag.Calisanlar = _db.Calisanlar
                    .Include(c => c.UygunlukDilimleri)
                    .Where(c => c.Hizmetler.Any(h => h.Id == hizmetId))
                    .ToList();
            }

            return View();
        }

        [HttpPost]
        public IActionResult Al(int hizmetId, int calisanId, DateOnly tarih, TimeOnly saat)
        {
            int hastaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var hasta = _db.Hastalar.Find(hastaId)!;
            var hizmet = _db.Hizmetler.Find(hizmetId)!;
            var calisan = _db.Calisanlar
                .Include(c => c.Hizmetler)
                .Include(c => c.UygunlukDilimleri)
                .First(c => c.Id == calisanId);

            try
            {
                // Kurallar (çalışan bu hizmeti veriyor mu, geçmiş tarih mi) Randevu sınıfının içinde kontrol edilir
                var randevu = new Randevu(hasta, calisan, hizmet, tarih.ToDateTime(saat));

                if (!calisan.UygunMu(randevu.Baslangic, randevu.Bitis))
                    throw new InvalidOperationException("Seçilen çalışan bu gün ve saatte randevu kabul etmiyor.");

                // Çakışma kontrolü: aynı gündeki randevulardan biri bu saatle çakışıyor mu?
                var gun = randevu.Baslangic.Date;
                var oGunkuRandevular = _db.Randevular
                    .Where(r => (r.CalisanId == calisanId || r.HastaId == hastaId)
                             && r.Baslangic >= gun && r.Baslangic < gun.AddDays(1))
                    .ToList();

                if (oGunkuRandevular.Any(r => r.CalisanId == calisanId && r.CakisiyorMu(randevu.Baslangic, randevu.Bitis)))
                    throw new InvalidOperationException("Seçilen çalışanın bu saatte başka bir randevusu var.");

                if (oGunkuRandevular.Any(r => r.HastaId == hastaId && r.CakisiyorMu(randevu.Baslangic, randevu.Bitis)))
                    throw new InvalidOperationException("Bu saatte zaten başka bir randevunuz var.");

                _db.Randevular.Add(randevu);
                _db.SaveChanges();

                TempData["Basari"] = $"Randevunuz oluşturuldu ve onay bekliyor: {calisan.GorunenAd}, {randevu.Baslangic:dd.MM.yyyy HH:mm}";
            }
            catch (InvalidOperationException hata)
            {
                TempData["Hata"] = hata.Message;
            }

            return RedirectToAction("Al", new { hizmetId });
        }
    }
}
