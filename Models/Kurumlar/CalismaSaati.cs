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
    }
}
