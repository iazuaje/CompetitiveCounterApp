using System.Globalization;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Estadísticas de todos los juegos, en memoria, a partir de todas las sesiones (con Game, SessionPlayers y Player).
/// Mismas reglas que el resto: sesiones sin victorias no cuentan; resultados solo de sesiones cerradas.
/// </summary>
public static class GeneralStatsCalculator
{
    public const int ActivityMonths = 6;

    static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-ES");

    public static GeneralStats Compute(IReadOnlyList<Game> games, IReadOnlyList<Session> sessions, DateTime today)
    {
        var valid = sessions
            .Where(s => s.TotalWins > 0)
            .OrderBy(s => s.SessionDate)
            .ToList();
        var closed = valid.Where(s => !s.IsActive).ToList();

        return new GeneralStats
        {
            GamesPlayed = valid.Select(s => s.GameID).Distinct().Count(),
            SessionsPlayed = valid.Count,
            TotalWins = valid.Sum(s => s.TotalWins),
            DistinctPlayers = valid.SelectMany(s => s.SessionPlayers).Select(sp => sp.PlayerID).Distinct().Count(),
            BusiestDay = BusiestDay(valid),
            Activity = BuildActivity(valid, today),
            Ranking = BuildRanking(valid),
            Games = BuildGames(games, valid),
            Records = BuildRecords(valid, closed)
        };
    }

    static string BusiestDay(IReadOnlyList<Session> sessions)
    {
        if (sessions.Count == 0)
            return string.Empty;

        var day = sessions
            .GroupBy(s => s.SessionDate.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return Spanish.DateTimeFormat.GetDayName(day);
    }

    /// <summary>Últimos meses hasta el actual (incluido), con 0 en los meses sin sesiones.</summary>
    static List<MonthPoint> BuildActivity(IReadOnlyList<Session> sessions, DateTime today) =>
        LastMonths(today)
            .Select(month => new MonthPoint(
                MonthLabel(month),
                sessions.Count(s => IsInMonth(s, month))))
            .ToList();

    /// <summary>Primer día de cada uno de los últimos <see cref="ActivityMonths"/> meses, del más viejo al actual.</summary>
    internal static IEnumerable<DateTime> LastMonths(DateTime today)
    {
        var current = new DateTime(today.Year, today.Month, 1);
        return Enumerable.Range(0, ActivityMonths).Select(i => current.AddMonths(i - ActivityMonths + 1));
    }

    internal static string MonthLabel(DateTime month) =>
        Spanish.DateTimeFormat.GetAbbreviatedMonthName(month.Month).TrimEnd('.');

    internal static bool IsInMonth(Session session, DateTime month) =>
        session.SessionDate.Year == month.Year && session.SessionDate.Month == month.Month;

    /// <summary>Mismo criterio que el ranking de cada juego, sumando todos: sesiones ganadas, victorias, nombre.</summary>
    static List<GlobalRankingRow> BuildRanking(IReadOnlyList<Session> sessions)
    {
        var ranked = PlayerGameStatsCalculator.RankPlayers(sessions);
        var maxSessionsWon = ranked.Count == 0 ? 0 : ranked.Max(r => r.SessionsWon);
        var maxWins = ranked.Count == 0 ? 0 : ranked.Max(r => r.TotalWins);

        return ranked
            .Select((r, index) => new GlobalRankingRow
            {
                Rank = index + 1,
                Player = r.Player,
                SessionsWon = r.SessionsWon,
                TotalWins = r.TotalWins,
                GamesPlayed = sessions
                    .Where(s => s.SessionPlayers.Any(sp => sp.PlayerID == r.Player.ID))
                    .Select(s => s.GameID)
                    .Distinct()
                    .Count(),
                Progress = maxSessionsWon > 0
                    ? (double)r.SessionsWon / maxSessionsWon
                    : maxWins == 0 ? 0 : (double)r.TotalWins / maxWins
            })
            .ToList();
    }

    static List<GameSummaryRow> BuildGames(IReadOnlyList<Game> games, IReadOnlyList<Session> sessions)
    {
        var byGame = sessions.GroupBy(s => s.GameID).ToDictionary(g => g.Key, g => g.ToList());
        var maxSessions = byGame.Count == 0 ? 0 : byGame.Values.Max(l => l.Count);

        return games
            .Where(g => byGame.ContainsKey(g.ID))
            .Select(g =>
            {
                var gameSessions = byGame[g.ID];
                return new GameSummaryRow
                {
                    Game = g,
                    Sessions = gameSessions.Count,
                    TotalWins = gameSessions.Sum(s => s.TotalWins),
                    Progress = maxSessions == 0 ? 0 : (double)gameSessions.Count / maxSessions,
                    LeaderName = PlayerGameStatsCalculator.RankPlayers(gameSessions).FirstOrDefault()?.Player.Name ?? string.Empty
                };
            })
            .OrderByDescending(r => r.Sessions)
            .ThenBy(r => r.Game.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    static List<GameRecord> BuildRecords(IReadOnlyList<Session> valid, IReadOnlyList<Session> closed)
    {
        var records = GameStatsCalculator.BuildRecords(valid, closed, showGame: true);

        // Más versátil: ganó sesiones en más juegos distintos (solo tiene sentido con 2 o más).
        var versatile = closed
            .SelectMany(s => s.SessionPlayers
                .Where(sp => sp.Player is not null && PlayerGameStatsCalculator.WonSession(s, sp.PlayerID))
                .Select(sp => (Player: sp.Player!, s.GameID)))
            .GroupBy(x => x.Player.ID)
            .Select(g => (Player: g.First().Player, Games: g.Select(x => x.GameID).Distinct().Count()))
            .Where(x => x.Games >= 2)
            .OrderByDescending(x => x.Games)
            .ThenBy(x => x.Player.Name, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault();
        if (versatile.Player is not null)
        {
            records.Add(new GameRecord(
                FluentUI.ribbon_24_regular,
                "Más versátil",
                versatile.Player.Name,
                $"Ganó sesiones en {versatile.Games} juegos distintos"));
        }

        return records;
    }
}
