namespace HastaneRandevuSistemi.Models.Kurumlar
{
    public class CalismaSaati
    {
        public int Id { get; set; }
        public DayOfWeek Gun { get; set; }
        public TimeOnly Acilis { get; set; }
        public TimeOnly Kapanis { get; set; }

        public int SubeId { get; set; }
        public Sube Sube { get; set; } = null!;

        // Verilen saat aralığı şubenin açık olduğu saatlerin içinde mi?
        public bool Kapsar(TimeOnly baslangic, TimeOnly bitis) =>
            baslangic >= Acilis && bitis <= Kapanis;
    }
}
