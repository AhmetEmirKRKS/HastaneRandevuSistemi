using System.Security.Cryptography;
using System.Text;

namespace HastaneRandevuSistemi.Data
{
    // Şifreler veritabanında açık halde değil, SHA256 ile şifrelenmiş halde tutulur
    public static class Sifreleme
    {
        public static string Hashle(string sifre)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sifre));
            return Convert.ToHexString(bytes);
        }
    }
}
