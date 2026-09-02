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

        /// <summary>Fondo de tarjeta en listados: color del juego si está activa.</summary>
        public Color ListCardBackground =>
            IsActive
                ? Game?.CurrentGameColor ?? Colors.Transparent
                : ResolveSecondaryBackground();

        /// <summary>Borde de tarjeta: complementario del juego si está activa.</summary>
        public Color ListCardStroke =>
            IsActive
                ? Game?.ComplementaryColor ?? Colors.Transparent
                : Colors.Transparent;

        public double ListCardStrokeThickness => IsActive ? 2d : 0d;

        /// <summary>Color de iconos/métricas secundarias en la tarjeta del listado.</summary>
        public Color ListCardIconColor =>
            IsActive ? Colors.White : ResolveMutedForeground();

        static Color ResolveSecondaryBackground()
        {
            var app = Application.Current;
            if (app?.Resources is null)
                return Colors.Transparent;

            var key = app.RequestedTheme == AppTheme.Dark
                ? "DarkSecondaryBackground"
                : "LightSecondaryBackground";

            return app.Resources.TryGetValue(key, out var resource) && resource is Color color
                ? color
                : Colors.Transparent;
        }

        static Color ResolveMutedForeground()
        {
            var app = Application.Current;
            if (app?.Resources is null)
                return Colors.Gray;

            var key = app.RequestedTheme == AppTheme.Dark ? "Gray400" : "Gray600";
            return app.Resources.TryGetValue(key, out var resource) && resource is Color color
                ? color
                : Colors.Gray;
        }

        public override string ToString() => $"{Game?.Name ?? "Unknown"} - {SessionDateLocal:dd/MM/yyyy}";
    }
}
