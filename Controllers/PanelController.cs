using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    // Doktor ve sağlık personelinin kendi randevularını görüp onayladığı/reddettiği panel
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
            return DurumDegistir(id, onayla: true);
        }

        [HttpPost]
        public IActionResult Reddet(int id)
        {
            return DurumDegistir(id, onayla: false);
        }

        private IActionResult DurumDegistir(int id, bool onayla)
        {
            // Çalışan sadece kendi randevusunu onaylayabilir/reddedebilir
            int calisanId = GirisYapanId;
            var randevu = _db.Randevular.FirstOrDefault(r => r.Id == id && r.CalisanId == calisanId);
            if (randevu == null)
                return NotFound();

            try
            {
                // Durum doğrudan değiştirilemez; Randevu sınıfının metodları kuralları kontrol eder
                if (onayla)
                    randevu.Onayla();
                else
                    randevu.Reddet();

                _db.SaveChanges();
            }
            catch (InvalidOperationException hata)
            {
                TempData["Hata"] = hata.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
