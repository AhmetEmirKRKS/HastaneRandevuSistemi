namespace HastaneRandevuSistemi.Models.Kurumlar
{
    public class Sube
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;

        public int KurumId { get; set; }
        public Kurum Kurum { get; set; } = null!;

        public ICollection<Bolum> Bolumler { get; set; } = new List<Bolum>();
        public ICollection<CalismaSaati> CalismaSaatleri { get; set; } = new List<CalismaSaati>();
    }
}
