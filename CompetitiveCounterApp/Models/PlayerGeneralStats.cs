namespace CompetitiveCounterApp.Models;

/// <summary>Resultado del jugador en un juego concreto (lista "Por juego").</summary>
public sealed class PlayerGameRow
{
    public required Game Game { get; init; }
    public int Rank { get; init; }
    public int RankedPlayers { get; init; }
    public int SessionsWon { get; init; }
    public int TotalWins { get; init; }
    public int SessionsPlayed { get; init; }

    public string RankText => Rank > 0 ? $"{Rank}º de {RankedPlayers}" : "Sin puesto";

    public string Detail =>
        $"{(SessionsWon == 1 ? "1 sesión ganada" : $"{SessionsWon} sesiones ganadas")} · {(TotalWins == 1 ? "1 victoria" : $"{TotalWins} victorias")}";
}

/// <summary>Porción del anillo de sesiones ganadas por juego.</summary>
public sealed class GameShareSlice
{
    public required Game Game { get; init; }
    public int Value { get; init; }
    public string Label => Game.Name;
}

/// <summary>Sesiones jugadas y ganadas en un mes.</summary>
public sealed record PlayerMonthPoint(string Label, int Played, int Won);

/// <summary>Estadísticas generales de un jugador en todos los juegos.</summary>
public sealed class PlayerGeneralStats
{
    /// <summary>Totales globales, duelos y rivales (mismo cálculo que la vista por juego, sobre todas las sesiones).</summary>
    public required PlayerGameStats Overall { get; init; }

    public int GamesPlayed { get; init; }
    public IReadOnlyList<PlayerGameRow> Games { get; init; } = [];
    public IReadOnlyList<GameShareSlice> SessionsWonByGame { get; init; } = [];
    public IReadOnlyList<PlayerMonthPoint> Activity { get; init; } = [];
    public IReadOnlyList<GameRecord> Achievements { get; init; } = [];
}
