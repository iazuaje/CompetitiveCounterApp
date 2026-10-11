using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Cálculo en memoria de rankings y estadísticas de un jugador en un juego,
/// a partir de las sesiones del juego (con SessionPlayers y Player cargados).
/// </summary>
public static class PlayerGameStatsCalculator
{
    public const int EvolutionSessions = 10;

    public sealed record RankedPlayer(Player Player, int TotalWins);

    /// <summary>
    /// Ranking por victorias acumuladas (desc) y nombre. Solo jugadores con al menos una victoria.
    /// Es el criterio del podio del detalle: ambas pantallas deben usar esta función.
    /// </summary>
    public static List<RankedPlayer> RankPlayers(IEnumerable<Session> sessions) =>
        sessions
            .SelectMany(s => s.SessionPlayers ?? [])
            .Where(sp => sp.Player is not null)
            .GroupBy(sp => sp.PlayerID)
            .Select(g => new RankedPlayer(g.First().Player!, g.Sum(sp => sp.Wins)))
            .Where(x => x.TotalWins > 0)
            .OrderByDescending(x => x.TotalWins)
            .ThenBy(x => x.Player.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    /// <summary>
    /// Totales y evolución incluyen la sesión activa; ganadas, rachas y cara a cara solo sesiones cerradas.
    /// Sesiones sin victorias no cuentan. Empate en el máximo = sesión ganada para todos los empatados.
    /// </summary>
    public static PlayerGameStats Compute(IReadOnlyList<Session> sessions, int playerId)
    {
        var valid = sessions.Where(s => s.TotalWins > 0).ToList();

        var ranking = RankPlayers(valid);
        var rankIndex = ranking.FindIndex(r => r.Player.ID == playerId);

        var played = valid
            .Where(s => s.SessionPlayers.Any(sp => sp.PlayerID == playerId))
            .OrderBy(s => s.SessionDate)
            .ToList();

        int WinsIn(Session s) => s.SessionPlayers.First(sp => sp.PlayerID == playerId).Wins;
        bool WonSession(Session s) => WinsIn(s) == s.SessionPlayers.Max(sp => sp.Wins);

        var totalWins = played.Sum(WinsIn);
        var groupWins = played.Sum(s => s.TotalWins);
        var closed = played.Where(s => !s.IsActive).ToList();

        var (currentStreak, bestStreak) = Streaks(closed.Select(WonSession));

        // Mejor sesión: más victorias; ante empate, la más reciente.
        var best = played
            .OrderByDescending(WinsIn)
            .ThenByDescending(s => s.SessionDate)
            .FirstOrDefault();

        var rivals = BuildRivals(closed, playerId);

        return new PlayerGameStats
        {
            Rank = rankIndex + 1,
            RankedPlayers = ranking.Count,
            TotalWins = totalWins,
            SessionsPlayed = played.Count,
            AverageWins = played.Count == 0 ? 0 : (double)totalWins / played.Count,
            WinShare = groupWins == 0 ? 0 : (double)totalWins / groupWins,
            ClosedSessionsPlayed = closed.Count,
            SessionsWon = closed.Count(WonSession),
            CurrentStreak = currentStreak,
            BestStreak = bestStreak,
            BestSessionWins = best is null ? 0 : WinsIn(best),
            BestSessionDate = best?.SessionDate,
            FirstSessionDate = played.FirstOrDefault()?.SessionDate,
            LastSessionDate = played.LastOrDefault()?.SessionDate,
            Evolution = played
                .TakeLast(EvolutionSessions)
                .Select(s => new SessionPoint(
                    s.IsActive ? "Activa" : s.SessionDate.ToString("dd/MM"),
                    WinsIn(s),
                    s.IsActive))
                .ToList(),
            Rivals = rivals,
            // Némesis: quien más le gana (desempate: peor saldo). Víctima: a quien más le gana.
            Nemesis = rivals
                .Where(r => r.Losses > 0)
                .OrderByDescending(r => r.Losses)
                .ThenByDescending(r => r.Losses - r.Wins)
                .FirstOrDefault(),
            Victim = rivals
                .Where(r => r.Wins > 0)
                .OrderByDescending(r => r.Wins)
                .ThenByDescending(r => r.Wins - r.Losses)
                .FirstOrDefault()
        };
    }

    /// <summary>Racha actual (ganadas consecutivas al final) y mejor racha, en orden cronológico.</summary>
    static (int Current, int Best) Streaks(IEnumerable<bool> wonInOrder)
    {
        int current = 0, best = 0;
        foreach (var won in wonInOrder)
        {
            current = won ? current + 1 : 0;
            best = Math.Max(best, current);
        }

        return (current, best);
    }

    static List<RivalRecord> BuildRivals(IEnumerable<Session> closedSessions, int playerId)
    {
        var tally = new Dictionary<int, (Player? Player, int Wins, int Draws, int Losses)>();

        foreach (var session in closedSessions)
        {
            var mine = session.SessionPlayers.First(sp => sp.PlayerID == playerId).Wins;

            foreach (var other in session.SessionPlayers)
            {
                if (other.PlayerID == playerId || other.Player is null)
                    continue;

                tally.TryGetValue(other.PlayerID, out var t);
                t.Player ??= other.Player;

                if (mine > other.Wins) t.Wins++;
                else if (mine < other.Wins) t.Losses++;
                else t.Draws++;

                tally[other.PlayerID] = t;
            }
        }

        return tally.Values
            .Select(t => new RivalRecord { Player = t.Player!, Wins = t.Wins, Draws = t.Draws, Losses = t.Losses })
            .OrderByDescending(r => r.Shared)
            .ThenBy(r => r.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
