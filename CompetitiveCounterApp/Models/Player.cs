namespace CompetitiveCounterApp.Models
{
    public class Player
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WinCount { get; set; }
        public string ColorHex { get; set; } = "#FF6B6B";

        public Color Color => Color.FromArgb(ColorHex);

        public override string ToString() => Name;
    }
}