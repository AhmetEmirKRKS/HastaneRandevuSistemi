namespace HastaneRandevuSistemi.Models
{
    // Haftanın günlerini Türkçe göstermek için yardımcı sınıf
    public static class Gunler
    {
        public static readonly DayOfWeek[] Liste =
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        public static string Ad(DayOfWeek gun) => gun switch
        {
            DayOfWeek.Monday => "Pazartesi",
            DayOfWeek.Tuesday => "Salı",
            DayOfWeek.Wednesday => "Çarşamba",
            DayOfWeek.Thursday => "Perşembe",
            DayOfWeek.Friday => "Cuma",
            DayOfWeek.Saturday => "Cumartesi",
            _ => "Pazar"
        };
    }
}
