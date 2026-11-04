using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Randevular;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Doktor ve sağlık personelinin kendi randevularını yönettiği panel
    [Authorize(Roles = "Doktor,SaglikPersoneli")]
    public class PanelController : Controller
    {
        private readonly HastaneContext _db;

        public PanelController(HastaneContext db)
        {
            _db = db;
        }

        private int GirisYapanId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public IActionResult Index()
        {
            int calisanId = GirisYapanId;
            var randevular = _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Hizmet)
                .Where(r => r.CalisanId == calisanId)
                .OrderBy(r => r.Baslangic)
                .ToList();
            return View(randevular);
        }

        [HttpPost]
        public IActionResult Onayla(int id)
        {
            var randevu = KendiRandevusu(id);
            if (randevu == null)
                return NotFound();

            try
            {
                randevu.Onayla();
                _db.SaveChanges();
            }
            catch (InvalidOperationException hata)
            {
                TempData["Hata"] = hata.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reddet(int id, string neden)
        {
            var randevu = KendiRandevusu(id);
            if (randevu == null)
                return NotFound();

            try
            {
                randevu.Reddet(neden);
                _db.SaveChanges();
            }
            catch (InvalidOperationException hata)
            {
                TempData["Hata"] = hata.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Tamamla(int id)
        {
            var randevu = KendiRandevusu(id);
            if (randevu == null)
                return NotFound();

            try
            {
                randevu.Tamamla();
                _db.SaveChanges();
            }
            catch (InvalidOperationException hata)
            {
                TempData["Hata"] = hata.Message;
            }
            return RedirectToAction("Index");
        }

        // Çalışan sadece kendi randevusu üzerinde işlem yapabilir
        private Randevu? KendiRandevusu(int id)
        {
            int calisanId = GirisYapanId;
            return _db.Randevular.FirstOrDefault(r => r.Id == id && r.CalisanId == calisanId);
        }
    }
}
