using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Kullanicilar;
using HastaneRandevuSistemi.Models.Kurumlar;
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

        // Çalışanın randevu kabul ettiği gün ve saatler
        public IActionResult Uygunluk(int id)
        {
            var calisan = _db.Calisanlar
                .Include(c => c.UygunlukDilimleri)
                .FirstOrDefault(c => c.Id == id);

            if (calisan == null)
                return NotFound();

            return View(calisan);
        }

        [HttpPost]
        public IActionResult UygunlukEkle(int calisanId, DayOfWeek gun, TimeOnly baslangic, TimeOnly bitis)
        {
            bool cakisiyor = _db.UygunlukDilimleri.Any(d =>
                d.CalisanId == calisanId && d.Gun == gun && d.Baslangic < bitis && baslangic < d.Bitis);

            if (baslangic >= bitis)
            {
                TempData["Hata"] = "Başlangıç saati bitiş saatinden önce olmalıdır.";
            }
            else if (cakisiyor)
            {
                TempData["Hata"] = "Bu saat aralığı mevcut bir uygunluk dilimiyle çakışıyor.";
            }
            else
            {
                _db.UygunlukDilimleri.Add(new UygunlukDilimi { CalisanId = calisanId, Gun = gun, Baslangic = baslangic, Bitis = bitis });
                _db.SaveChanges();
            }
            return RedirectToAction("Uygunluk", new { id = calisanId });
        }
    }
}
