namespace CompetitiveCounterApp.Models
{
    public class Session
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public DateTime SessionDate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Null = sesión activa. Con valor = sesión cerrada.
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        public Game? Game { get; set; }
        public List<SessionPlayer> SessionPlayers { get; set; } = [];

        public bool IsActive => ClosedAt is null;

        /// <summary>Fecha de creación en zona horaria local del dispositivo.</summary>
        public DateTime SessionDateLocal => ToLocal(SessionDate);

        public DateTime? ClosedAtLocal => ClosedAt is null ? null : ToLocal(ClosedAt.Value);

        public override string ToString() => $"{Game?.Name ?? "Unknown"} - {SessionDateLocal:dd/MM/yyyy}";

        /// <summary>
        /// SQLite/EF suelen materializar DateTime como UTC o Unspecified (tratado como UTC).
        /// </summary>
        private static DateTime ToLocal(DateTime value) =>
            value.Kind == DateTimeKind.Local
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime();
    }
}
