namespace HastaneRandevuSistemi.Models.Kurumlar
{
    // Hastane/klinik. Tek şubeli bir klinik, tek Sube'si olan bir Kurum'dur.
    public class Kurum
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;

        public ICollection<Sube> Subeler { get; set; } = new List<Sube>();
    }
}
