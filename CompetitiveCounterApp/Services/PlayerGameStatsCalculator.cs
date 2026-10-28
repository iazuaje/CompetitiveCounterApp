using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Cálculo en memoria de rankings y estadísticas de un jugador en un juego,
/// a partir de las sesiones del juego (con SessionPlayers y Player cargados).
/// </summary>
public static class PlayerGameStatsCalculator
{
    public const int EvolutionSessions = 10;

    public sealed record RankedPlayer(Player Player, int SessionsWon, int TotalWins);

    /// <summary>
    /// Ranking del juego: sesiones ganadas (cerradas) desc, luego victorias totales desc, luego nombre.
    /// Solo jugadores con al menos una victoria. Lo usan el podio del detalle, el líder de la
    /// tarjeta de Juegos y el puesto en las estadísticas: no reimplementar el orden en otro lado.
    /// </summary>
    public static List<RankedPlayer> RankPlayers(IEnumerable<Session> sessions)
    {
        var list = sessions as IReadOnlyCollection<Session> ?? sessions.ToList();

        return list
            .SelectMany(s => s.SessionPlayers ?? [])
            .Where(sp => sp.Player is not null)
            .GroupBy(sp => sp.PlayerID)
            .Select(g => new RankedPlayer(
                g.First().Player!,
                list.Count(s => !s.IsActive && WonSession(s, g.Key)),
                g.Sum(sp => sp.Wins)))
            .Where(x => x.TotalWins > 0)
            .OrderByDescending(x => x.SessionsWon)
            .ThenByDescending(x => x.TotalWins)
            .ThenBy(x => x.Player.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// El jugador terminó primero (empates en el máximo cuentan para todos). Sesiones sin victorias no tienen ganador.
    /// </summary>
    public static bool WonSession(Session session, int playerId)
    {
        var mine = session.SessionPlayers?.FirstOrDefault(sp => sp.PlayerID == playerId);
        if (mine is null || mine.Wins == 0)
            return false;

        return mine.Wins == session.SessionPlayers!.Max(sp => sp.Wins);
    }

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
        bool Won(Session s) => WonSession(s, playerId);

        var totalWins = played.Sum(WinsIn);
        var groupWins = played.Sum(s => s.TotalWins);
        var closed = played.Where(s => !s.IsActive).ToList();

        var (currentStreak, bestStreak) = Streaks(closed.Select(Won));

        // Mejor sesión: más victorias; ante empate, la más reciente.
        var best = played
            .OrderByDescending(WinsIn)
            .ThenByDescending(s => s.SessionDate)
            .FirstOrDefault();

        var rivals = BuildRivals(closed, playerId);
        var (nemesis, victim) = NemesisAndVictim(rivals);

        return new PlayerGameStats
        {
            Rank = rankIndex + 1,
            RankedPlayers = ranking.Count,
            TotalWins = totalWins,
            SessionsPlayed = played.Count,
            AverageWins = played.Count == 0 ? 0 : (double)totalWins / played.Count,
            WinShare = groupWins == 0 ? 0 : (double)totalWins / groupWins,
            ClosedSessionsPlayed = closed.Count,
            SessionsWon = closed.Count(Won),
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
            Nemesis = nemesis,
            Victim = victim
        };
    }

    /// <summary>Racha actual (ganadas consecutivas al final) y mejor racha, en orden cronológico.</summary>
    /// <summary>
    /// Némesis: quien más le gana (desempate: peor saldo). Víctima: a quien más le gana.
    /// Si ambos son el mismo rival, queda solo el que indica el saldo (ninguno si está parejo).
    /// </summary>
    static (RivalRecord? Nemesis, RivalRecord? Victim) NemesisAndVictim(IReadOnlyList<RivalRecord> rivals)
    {
        var nemesis = rivals
            .Where(r => r.Losses > 0)
            .OrderByDescending(r => r.Losses)
            .ThenByDescending(r => r.Losses - r.Wins)
            .FirstOrDefault();
        var victim = rivals
            .Where(r => r.Wins > 0)
            .OrderByDescending(r => r.Wins)
            .ThenByDescending(r => r.Wins - r.Losses)
            .FirstOrDefault();

        if (nemesis is null || victim is null || nemesis.Player.ID != victim.Player.ID)
            return (nemesis, victim);

        var balance = victim.Wins - victim.Losses;
        return balance > 0 ? (null, victim)
            : balance < 0 ? (nemesis, null)
            : (null, null);
    }

    public static (int Current, int Best) Streaks(IEnumerable<bool> wonInOrder)
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
