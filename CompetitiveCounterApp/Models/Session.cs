namespace CompetitiveCounterApp.Models
{
    public class Session
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public DateTime SessionDate { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public Game? Game { get; set; }
        public List<SessionPlayer> SessionPlayers { get; set; } = [];

        public override string ToString() => $"{Game?.Name ?? "Unknown"} - {SessionDate:dd/MM/yyyy}";
    }
}
