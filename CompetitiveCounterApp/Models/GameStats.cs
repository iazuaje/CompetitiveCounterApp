namespace CompetitiveCounterApp.Models;

/// <summary>Fila del ranking completo del juego.</summary>
public sealed class GameRankingRow
{
    public int Rank { get; init; }
    public required Player Player { get; init; }
    public int SessionsWon { get; init; }
    public int TotalWins { get; init; }
    public int ClosedSessionsPlayed { get; init; }

    /// <summary>Largo de la barra (0..1) relativo al líder en sesiones ganadas.</summary>
    public double Progress { get; init; }

    public double SessionWinRate => ClosedSessionsPlayed == 0 ? 0 : (double)SessionsWon / ClosedSessionsPlayed;
    public string Detail => $"{(TotalWins == 1 ? "1 victoria" : $"{TotalWins} victorias")} · {SessionWinRate:P0} de sesiones";
}

/// <summary>Porción de un anillo de reparto (sesiones ganadas o victorias) de un jugador.</summary>
public sealed class ShareSlice
{
    public required Player Player { get; init; }
    public int Value { get; init; }
    public string Label => Player.Name;
}

/// <summary>Victorias de un jugador en cada sesión del historial (misma lista de etiquetas para todos).</summary>
public sealed class HistorySeries
{
    public required Player Player { get; init; }
    public IReadOnlyList<HistoryPoint> Points { get; init; } = [];
}

public sealed record HistoryPoint(string Label, int Wins);

/// <summary>Récord destacado del juego (mejor sesión, mayor paliza, etc.).</summary>
public sealed record GameRecord(string Glyph, string Title, string Headline, string Detail);

/// <summary>Estadísticas generales de un juego.</summary>
public sealed class GameStats
{
    public int SessionsPlayed { get; init; }
    public int ClosedSessions { get; init; }
    public int TotalWins { get; init; }
    public int DistinctPlayers { get; init; }
    public double AveragePlayersPerSession { get; init; }
    public double AverageWinsPerSession { get; init; }
    public DateTime? FirstSessionDate { get; init; }
    public DateTime? LastSessionDate { get; init; }

    public IReadOnlyList<GameRankingRow> Ranking { get; init; } = [];
    public IReadOnlyList<ShareSlice> SessionsWonShare { get; init; } = [];
    public IReadOnlyList<ShareSlice> WinsShare { get; init; } = [];
    public IReadOnlyList<HistorySeries> History { get; init; } = [];
    public IReadOnlyList<GameRecord> Records { get; init; } = [];
}
