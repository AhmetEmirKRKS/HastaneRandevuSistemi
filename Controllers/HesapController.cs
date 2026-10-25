using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models.Kullanicilar;
using HastaneRandevuSistemi.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers
{
    // Giriş, kayıt ve çıkış işlemleri
    public class HesapController : Controller
    {
        private readonly HastaneContext _db;

        public HesapController(HastaneContext db)
        {
            _db = db;
        }

        public IActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string sifreHash = Sifreleme.Hashle(model.Sifre);
            Kullanici? kullanici = _db.Kullanicilar
                .FirstOrDefault(k => k.Email == model.Email && k.SifreHash == sifreHash);

            if (kullanici == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }

            await OturumAc(kullanici);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Kayit()
        {
            return View();
        }

        // Sadece hastalar kendi kendine kayıt olabilir; çalışanları yönetici ekler
        [HttpPost]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (_db.Kullanicilar.Any(k => k.Email == model.Email))
                ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı.");

            if (!ModelState.IsValid)
                return View(model);

            var hasta = new Hasta
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                SifreHash = Sifreleme.Hashle(model.Sifre)
            };
            _db.Hastalar.Add(hasta);
            _db.SaveChanges();

            await OturumAc(hasta);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult YetkisizErisim()
        {
            return View();
        }

        // Kullanıcının bilgilerini çereze yazar. kullanici.Rol ve kullanici.GorunenAd
        // alt sınıfa göre farklı sonuç verir (çok biçimlilik).
        private async Task OturumAc(Kullanici kullanici)
        {
            var bilgiler = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.GorunenAd),
                new Claim(ClaimTypes.Role, kullanici.Rol.ToString())
            };

            var kimlik = new ClaimsIdentity(bilgiler, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(kimlik));
        }
    }
}
