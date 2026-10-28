using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Estadísticas generales de un juego, en memoria, a partir de sus sesiones (con SessionPlayers y Player).
/// Mismas reglas que <see cref="PlayerGameStatsCalculator"/>: sesiones sin victorias no cuentan;
/// totales e historial incluyen la activa; resultados (ganadas, récords de sesión, rivalidades) solo cerradas.
/// </summary>
public static class GameStatsCalculator
{
    public const int HistorySessions = 10;

    public static GameStats Compute(IReadOnlyList<Session> sessions)
    {
        var valid = sessions
            .Where(s => s.TotalWins > 0)
            .OrderBy(s => s.SessionDate)
            .ToList();
        var closed = valid.Where(s => !s.IsActive).ToList();

        var ranked = PlayerGameStatsCalculator.RankPlayers(valid);
        var maxSessionsWon = ranked.Count == 0 ? 0 : ranked.Max(r => r.SessionsWon);
        var maxWins = ranked.Count == 0 ? 0 : ranked.Max(r => r.TotalWins);

        var ranking = ranked
            .Select((r, index) => new GameRankingRow
            {
                Rank = index + 1,
                Player = r.Player,
                SessionsWon = r.SessionsWon,
                TotalWins = r.TotalWins,
                ClosedSessionsPlayed = closed.Count(s => s.SessionPlayers.Any(sp => sp.PlayerID == r.Player.ID)),
                // Sin sesiones cerradas ganadas todavía, la barra refleja las victorias.
                Progress = maxSessionsWon > 0
                    ? (double)r.SessionsWon / maxSessionsWon
                    : maxWins == 0 ? 0 : (double)r.TotalWins / maxWins
            })
            .ToList();

        var participations = valid.Sum(s => s.SessionPlayers.Count);

        return new GameStats
        {
            SessionsPlayed = valid.Count,
            ClosedSessions = closed.Count,
            TotalWins = valid.Sum(s => s.TotalWins),
            DistinctPlayers = valid.SelectMany(s => s.SessionPlayers).Select(sp => sp.PlayerID).Distinct().Count(),
            AveragePlayersPerSession = valid.Count == 0 ? 0 : (double)participations / valid.Count,
            AverageWinsPerSession = valid.Count == 0 ? 0 : (double)valid.Sum(s => s.TotalWins) / valid.Count,
            FirstSessionDate = valid.FirstOrDefault()?.SessionDate,
            LastSessionDate = valid.LastOrDefault()?.SessionDate,
            Ranking = ranking,
            SessionsWonShare = ranked
                .Where(r => r.SessionsWon > 0)
                .Select(r => new ShareSlice { Player = r.Player, Value = r.SessionsWon })
                .ToList(),
            // Orden propio por victorias: el líder de este anillo puede no ser el del ranking.
            WinsShare = ranked
                .OrderByDescending(r => r.TotalWins)
                .ThenBy(r => r.Player.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(r => new ShareSlice { Player = r.Player, Value = r.TotalWins })
                .ToList(),
            History = BuildHistory(valid.TakeLast(HistorySessions).ToList()),
            Records = BuildRecords(valid, closed, showGame: false)
        };
    }

    /// <summary>Una serie por jugador con un punto por sesión (0 si no jugó), para columnas apiladas.</summary>
    static List<HistorySeries> BuildHistory(IReadOnlyList<Session> sessions)
    {
        var labels = sessions
            .Select(s => s.IsActive ? "Activa" : s.SessionDate.ToString("dd/MM"))
            .ToList();

        return sessions
            .SelectMany(s => s.SessionPlayers)
            .Where(sp => sp.Player is not null && sp.Wins > 0)
            .GroupBy(sp => sp.PlayerID)
            .Select(g => new HistorySeries
            {
                Player = g.First().Player!,
                Points = sessions
                    .Select((s, i) => new HistoryPoint(
                        labels[i],
                        s.SessionPlayers.FirstOrDefault(sp => sp.PlayerID == g.Key)?.Wins ?? 0))
                    .ToList()
            })
            .OrderByDescending(h => h.Points.Sum(p => p.Wins))
            .ToList();
    }

    /// <summary>
    /// Récords sobre un conjunto de sesiones (de un juego o de todos). Con <paramref name="showGame"/>
    /// el detalle incluye el juego. Las rachas son siempre dentro de un mismo juego.
    /// </summary>
    internal static List<GameRecord> BuildRecords(
        IReadOnlyList<Session> valid,
        IReadOnlyList<Session> closed,
        bool showGame)
    {
        string DateText(Session session) => showGame
            ? $"{session.Game?.Name} · {GameStatsCalculator.DateText(session)}"
            : GameStatsCalculator.DateText(session);

        var records = new List<GameRecord>();

        // Mejor sesión individual: más victorias de un jugador en una sesión (empate: la más reciente).
        var bestIndividual = valid
            .SelectMany(s => s.SessionPlayers.Where(sp => sp.Player is not null).Select(sp => (Session: s, Entry: sp)))
            .OrderByDescending(x => x.Entry.Wins)
            .ThenByDescending(x => x.Session.SessionDate)
            .FirstOrDefault();
        if (bestIndividual.Entry is not null)
        {
            records.Add(new GameRecord(
                FluentUI.star_24_regular,
                "Mejor sesión",
                $"{bestIndividual.Entry.Player!.Name} · {Victories(bestIndividual.Entry.Wins)}",
                DateText(bestIndividual.Session)));
        }

        // Paliza y sesión más pareja: diferencia entre 1º y 2º en sesiones cerradas con 2 o más jugadores.
        var margins = closed
            .Where(s => s.SessionPlayers.Count >= 2)
            .Select(s =>
            {
                var ordered = s.SessionPlayers.OrderByDescending(sp => sp.Wins).ToList();
                return (Session: s, First: ordered[0], Second: ordered[1], Margin: ordered[0].Wins - ordered[1].Wins);
            })
            .ToList();

        var blowout = margins
            .Where(m => m.Margin > 0)
            .OrderByDescending(m => m.Margin)
            .ThenByDescending(m => m.Session.SessionDate)
            .FirstOrDefault();
        if (blowout.Session is not null)
        {
            records.Add(new GameRecord(
                FluentUI.flash_24_regular,
                "Mayor paliza",
                $"{blowout.First.Player?.Name} le sacó {blowout.Margin} a {blowout.Second.Player?.Name}",
                $"{blowout.First.Wins} a {blowout.Second.Wins} · {DateText(blowout.Session)}"));
        }

        var closest = margins
            .OrderBy(m => m.Margin)
            .ThenByDescending(m => m.Session.TotalWins)
            .ThenByDescending(m => m.Session.SessionDate)
            .FirstOrDefault();
        if (closest.Session is not null)
        {
            records.Add(new GameRecord(
                FluentUI.scales_24_regular,
                "Sesión más pareja",
                closest.Margin == 0
                    ? $"Empate entre {closest.First.Player?.Name} y {closest.Second.Player?.Name}"
                    : $"{closest.First.Player?.Name} le ganó por {closest.Margin} a {closest.Second.Player?.Name}",
                $"{closest.First.Wins} a {closest.Second.Wins} · {DateText(closest.Session)}"));
        }

        // Mejor racha de sesiones ganadas: consecutivas, en orden cronológico, dentro de un mismo juego.
        var bestStreak = closed
            .SelectMany(s => s.SessionPlayers
                .Where(sp => sp.Player is not null)
                .Select(sp => (Session: s, Entry: sp)))
            .GroupBy(x => (x.Entry.PlayerID, x.Session.GameID))
            .Select(g => (
                Player: g.First().Entry.Player!,
                Game: g.First().Session.Game,
                Streak: PlayerGameStatsCalculator.Streaks(g
                    .OrderBy(x => x.Session.SessionDate)
                    .Select(x => PlayerGameStatsCalculator.WonSession(x.Session, x.Entry.PlayerID))).Best))
            .Where(x => x.Streak > 0)
            .OrderByDescending(x => x.Streak)
            .FirstOrDefault();
        if (bestStreak.Player is not null)
        {
            records.Add(new GameRecord(
                FluentUI.fire_24_regular,
                "Mejor racha",
                $"{bestStreak.Player.Name} · {bestStreak.Streak} {(bestStreak.Streak == 1 ? "sesión ganada" : "sesiones ganadas seguidas")}",
                showGame
                    ? $"Seguidas en {bestStreak.Game?.Name}"
                    : "Sesiones cerradas consecutivas que terminó primero"));
        }

        // Rivalidad más disputada: par con más sesiones cerradas compartidas; desempate, el balance más parejo.
        var rivalry = closed
            .SelectMany(s => s.SessionPlayers
                .Where(a => a.Player is not null)
                .SelectMany(a => s.SessionPlayers
                    .Where(b => b.Player is not null && b.PlayerID > a.PlayerID)
                    .Select(b => (A: a, B: b))))
            .GroupBy(p => (p.A.PlayerID, p.B.PlayerID))
            .Select(g => (
                A: g.First().A.Player!,
                B: g.First().B.Player!,
                AWins: g.Count(p => p.A.Wins > p.B.Wins),
                BWins: g.Count(p => p.B.Wins > p.A.Wins),
                Draws: g.Count(p => p.A.Wins == p.B.Wins),
                Shared: g.Count()))
            .OrderByDescending(r => r.Shared)
            .ThenBy(r => Math.Abs(r.AWins - r.BWins))
            .FirstOrDefault();
        if (rivalry.A is not null)
        {
            records.Add(new GameRecord(
                FluentUI.people_swap_24_regular,
                "Rivalidad más disputada",
                $"{rivalry.A.Name} vs {rivalry.B.Name}",
                $"{rivalry.AWins} - {rivalry.BWins}{(rivalry.Draws > 0 ? $" · {rivalry.Draws} {(rivalry.Draws == 1 ? "empate" : "empates")}" : string.Empty)} en {rivalry.Shared} {(rivalry.Shared == 1 ? "sesión" : "sesiones")}"));
        }

        return records;
    }

    static string Victories(int wins) => wins == 1 ? "1 victoria" : $"{wins} victorias";

    internal static string DateText(Session session) =>
        session.IsActive ? "Sesión activa" : session.SessionDate.ToString("dd/MM/yyyy");
}
