namespace CompetitiveCounterApp.Models
{
    public class Session
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public DateTime SessionDate { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Null = sesión activa. Con valor = sesión cerrada.
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        public Game? Game { get; set; }
        public List<SessionPlayer> SessionPlayers { get; set; } = [];

        public bool IsActive => ClosedAt is null;

        public override string ToString() => $"{Game?.Name ?? "Unknown"} - {SessionDate:dd/MM/yyyy}";
    }
}
