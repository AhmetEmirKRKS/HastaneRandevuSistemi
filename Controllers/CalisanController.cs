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

        [HttpPost]
        public IActionResult UygunlukSil(int id)
        {
            var dilim = _db.UygunlukDilimleri.Find(id)!;
            _db.UygunlukDilimleri.Remove(dilim);
            _db.SaveChanges();
            return RedirectToAction("Uygunluk", new { id = dilim.CalisanId });
        }

        public IActionResult Duzenle(int id)
        {
            var calisan = _db.Calisanlar
                .Include(c => c.Hizmetler)
                .FirstOrDefault(c => c.Id == id);

            if (calisan == null)
                return NotFound();

            ViewBag.Bolumler = _db.Bolumler.Include(b => b.Sube).ToList();
            ViewBag.Hizmetler = _db.Hizmetler.Include(h => h.Bolum).ToList();
            return View(calisan);
        }

        // Çalışanın türü (Doktor / Sağlık Personeli) değiştirilemez, diğer bilgileri değiştirilebilir
        [HttpPost]
        public IActionResult Duzenle(int id, string ad, string soyad, string email,
                                     int bolumId, string uzmanlikVeyaGorev, List<int> hizmetIdleri)
        {
            var calisan = _db.Calisanlar
                .Include(c => c.Hizmetler)
                .FirstOrDefault(c => c.Id == id);

            if (calisan == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) || string.IsNullOrWhiteSpace(email))
            {
                TempData["Hata"] = "Lütfen tüm alanları doldurunuz.";
                return RedirectToAction("Duzenle", new { id });
            }

            if (_db.Kullanicilar.Any(k => k.Email == email && k.Id != id))
            {
                TempData["Hata"] = "Bu e-posta başka bir kullanıcıya ait.";
                return RedirectToAction("Duzenle", new { id });
            }

            calisan.Ad = ad;
            calisan.Soyad = soyad;
            calisan.Email = email;
            calisan.BolumId = bolumId;

            if (calisan is Doktor doktor)
                doktor.UzmanlikAlani = uzmanlikVeyaGorev;
            else if (calisan is SaglikPersoneli personel)
                personel.Gorev = uzmanlikVeyaGorev;

            // Verdiği hizmetleri yeniden belirle
            calisan.Hizmetler.Clear();
            foreach (var hizmet in _db.Hizmetler.Where(h => hizmetIdleri.Contains(h.Id)))
            {
                calisan.Hizmetler.Add(hizmet);
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Sil(int id)
        {
            if (_db.Randevular.Any(r => r.CalisanId == id))
            {
                TempData["Hata"] = "Randevusu olan bir çalışan silinemez.";
            }
            else
            {
                // Çalışanın uygunluk dilimleri de onunla birlikte silinir
                _db.Calisanlar.Remove(_db.Calisanlar.Find(id)!);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
