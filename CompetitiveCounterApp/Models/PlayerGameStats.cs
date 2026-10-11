namespace CompetitiveCounterApp.Models;

/// <summary>Punto genérico para gráficos (anillos, doughnut).</summary>
public sealed record ChartItem(string Label, double Value);

/// <summary>Victorias del jugador en una sesión, para la curva de evolución.</summary>
public sealed record SessionPoint(string Label, int Wins, bool IsActive);

/// <summary>Balance contra un rival en sesiones cerradas compartidas.</summary>
public sealed class RivalRecord
{
    public required Player Player { get; init; }

    /// <summary>Sesiones en que el jugador terminó por encima del rival.</summary>
    public int Wins { get; init; }
    public int Draws { get; init; }
    public int Losses { get; init; }

    public int Shared => Wins + Draws + Losses;
    public string Name => Player.Name;
    public string Summary => $"G {Wins} · E {Draws} · P {Losses}";
}

/// <summary>Estadísticas de un jugador dentro de un juego.</summary>
public sealed class PlayerGameStats
{
    /// <summary>Puesto en el ranking del juego; 0 = sin puesto (sin victorias).</summary>
    public int Rank { get; init; }
    public int RankedPlayers { get; init; }

    public int TotalWins { get; init; }
    public int SessionsPlayed { get; init; }
    public double AverageWins { get; init; }

    /// <summary>Fracción (0..1) de las victorias de sus sesiones que son suyas.</summary>
    public double WinShare { get; init; }

    public int ClosedSessionsPlayed { get; init; }
    public int SessionsWon { get; init; }

    /// <summary>Fracción (0..1) de sesiones cerradas ganadas.</summary>
    public double SessionWinRate => ClosedSessionsPlayed == 0 ? 0 : (double)SessionsWon / ClosedSessionsPlayed;

    public int CurrentStreak { get; init; }
    public int BestStreak { get; init; }

    public int BestSessionWins { get; init; }
    public DateTime? BestSessionDate { get; init; }
    public DateTime? FirstSessionDate { get; init; }
    public DateTime? LastSessionDate { get; init; }

    public IReadOnlyList<SessionPoint> Evolution { get; init; } = [];
    public IReadOnlyList<RivalRecord> Rivals { get; init; } = [];
    public RivalRecord? Nemesis { get; init; }
    public RivalRecord? Victim { get; init; }

    public int HeadToHeadWins => Rivals.Sum(r => r.Wins);
    public int HeadToHeadDraws => Rivals.Sum(r => r.Draws);
    public int HeadToHeadLosses => Rivals.Sum(r => r.Losses);
}
