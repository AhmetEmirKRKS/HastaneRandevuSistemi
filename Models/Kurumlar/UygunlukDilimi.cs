using HastaneRandevuSistemi.Models.Kullanicilar;

namespace HastaneRandevuSistemi.Models.Kurumlar
{
    // Çalışanın randevu kabul ettiği gün ve saat aralığı
    public class UygunlukDilimi
    {
        public int Id { get; set; }
        public DayOfWeek Gun { get; set; }
        public TimeOnly Baslangic { get; set; }
        public TimeOnly Bitis { get; set; }

        public int CalisanId { get; set; }
        public Calisan Calisan { get; set; } = null!;

        // Verilen zaman aralığı bu dilimin içinde mi?
        public bool Kapsar(DateTime baslangic, DateTime bitis)
        {
            if (baslangic.Date != bitis.Date || baslangic.DayOfWeek != Gun)
                return false;

            return TimeOnly.FromDateTime(baslangic) >= Baslangic
                && TimeOnly.FromDateTime(bitis) <= Bitis;
        }
    }
}
