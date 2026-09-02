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

        /// <summary>Suma de victorias de todos los jugadores de la sesión.</summary>
        public int TotalWins => SessionPlayers?.Sum(sp => sp.Wins) ?? 0;

        public int PlayerCount => SessionPlayers?.Count ?? 0;

        /// <summary>Fecha para UI: hora local del dispositivo (sin conversión UTC).</summary>
        public DateTime SessionDateLocal => SessionDate;

        public DateTime? ClosedAtLocal => ClosedAt;

        public override string ToString() => $"{Game?.Name ?? "Unknown"} - {SessionDateLocal:dd/MM/yyyy}";
    }
}
