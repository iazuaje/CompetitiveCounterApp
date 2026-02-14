namespace CompetitiveCounterApp.Models
{
    public class SessionPlayer
    {
        public int ID { get; set; }
        public int SessionID { get; set; }
        public int PlayerID { get; set; }
        public int Wins { get; set; }

        // Navigation properties
        public Session? Session { get; set; }
        public Player? Player { get; set; }

        public override string ToString() => $"{Player?.Name ?? "Unknown"} - {Wins} wins";
    }
}
