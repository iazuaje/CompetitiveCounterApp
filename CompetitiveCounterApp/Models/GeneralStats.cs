namespace CompetitiveCounterApp.Models;

/// <summary>Sesiones jugadas en un mes (gráfico de actividad).</summary>
public sealed record MonthPoint(string Label, int Sessions);

/// <summary>Fila del ranking global (todos los juegos).</summary>
public sealed class GlobalRankingRow
{
    public int Rank { get; init; }
    public required Player Player { get; init; }
    public int SessionsWon { get; init; }
    public int TotalWins { get; init; }
    public int GamesPlayed { get; init; }

    /// <summary>Largo de la barra (0..1) relativo al líder.</summary>
    public double Progress { get; init; }

    public string Detail =>
        $"{(TotalWins == 1 ? "1 victoria" : $"{TotalWins} victorias")} · {(GamesPlayed == 1 ? "1 juego" : $"{GamesPlayed} juegos")}";
}

/// <summary>Fila de la comparativa de juegos.</summary>
public sealed class GameSummaryRow
{
    public required Game Game { get; init; }
    public int Sessions { get; init; }
    public int TotalWins { get; init; }

    /// <summary>Largo de la barra (0..1) relativo al juego más jugado.</summary>
    public double Progress { get; init; }

    public string LeaderName { get; init; } = string.Empty;
    public bool HasLeader => !string.IsNullOrEmpty(LeaderName);

    public string Detail =>
        $"{(Sessions == 1 ? "1 sesión" : $"{Sessions} sesiones")} · {(TotalWins == 1 ? "1 victoria" : $"{TotalWins} victorias")}";
}

/// <summary>Estadísticas generales de todos los juegos.</summary>
public sealed class GeneralStats
{
    public int GamesPlayed { get; init; }
    public int SessionsPlayed { get; init; }
    public int TotalWins { get; init; }
    public int DistinctPlayers { get; init; }

    /// <summary>Día de la semana con más sesiones (vacío si no hay sesiones).</summary>
    public string BusiestDay { get; init; } = string.Empty;

    public IReadOnlyList<MonthPoint> Activity { get; init; } = [];
    public IReadOnlyList<GlobalRankingRow> Ranking { get; init; } = [];
    public IReadOnlyList<GameSummaryRow> Games { get; init; } = [];
    public IReadOnlyList<GameRecord> Records { get; init; } = [];
}
