using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Estadísticas de un jugador en todos los juegos. Reutiliza <see cref="PlayerGameStatsCalculator.Compute"/>:
/// sobre todas las sesiones (totales, puesto global, duelos y rivales) y sobre las de cada juego (puesto y racha por juego).
/// Las rachas se cuentan dentro de un mismo juego.
/// </summary>
public static class PlayerGeneralStatsCalculator
{
    public static PlayerGeneralStats Compute(
        IReadOnlyList<Game> games,
        IReadOnlyList<Session> sessions,
        int playerId,
        DateTime today)
    {
        var overall = PlayerGameStatsCalculator.Compute(sessions, playerId);

        var played = sessions
            .Where(s => s.TotalWins > 0 && s.SessionPlayers.Any(sp => sp.PlayerID == playerId))
            .OrderBy(s => s.SessionDate)
            .ToList();

        var perGame = games
            .Select(g => (Game: g, Stats: PlayerGameStatsCalculator.Compute(
                sessions.Where(s => s.GameID == g.ID).ToList(), playerId)))
            .Where(x => x.Stats.SessionsPlayed > 0)
            .ToList();

        var rows = perGame
            .Select(x => new PlayerGameRow
            {
                Game = x.Game,
                Rank = x.Stats.Rank,
                RankedPlayers = x.Stats.RankedPlayers,
                SessionsWon = x.Stats.SessionsWon,
                TotalWins = x.Stats.TotalWins,
                SessionsPlayed = x.Stats.SessionsPlayed
            })
            .OrderByDescending(r => r.SessionsWon)
            .ThenByDescending(r => r.TotalWins)
            .ThenBy(r => r.Game.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new PlayerGeneralStats
        {
            Overall = overall,
            GamesPlayed = rows.Count,
            Games = rows,
            SessionsWonByGame = rows
                .Where(r => r.SessionsWon > 0)
                .Select(r => new GameShareSlice { Game = r.Game, Value = r.SessionsWon })
                .ToList(),
            Activity = GeneralStatsCalculator.LastMonths(today)
                .Select(month =>
                {
                    var inMonth = played.Where(s => GeneralStatsCalculator.IsInMonth(s, month)).ToList();
                    return new PlayerMonthPoint(
                        GeneralStatsCalculator.MonthLabel(month),
                        inMonth.Count,
                        inMonth.Count(s => !s.IsActive && PlayerGameStatsCalculator.WonSession(s, playerId)));
                })
                .ToList(),
            Achievements = BuildAchievements(played, perGame, playerId)
        };
    }

    static List<GameRecord> BuildAchievements(
        IReadOnlyList<Session> played,
        IReadOnlyList<(Game Game, PlayerGameStats Stats)> perGame,
        int playerId)
    {
        var achievements = new List<GameRecord>();

        int WinsIn(Session s) => s.SessionPlayers.First(sp => sp.PlayerID == playerId).Wins;
        string Where(Session s) => $"{s.Game?.Name} · {GameStatsCalculator.DateText(s)}";

        // Mejor sesión: más victorias propias en una sesión (empate: la más reciente).
        var best = played
            .OrderByDescending(WinsIn)
            .ThenByDescending(s => s.SessionDate)
            .FirstOrDefault();
        if (best is not null && WinsIn(best) > 0)
        {
            var wins = WinsIn(best);
            achievements.Add(new GameRecord(
                FluentUI.star_24_regular,
                "Mejor sesión",
                wins == 1 ? "1 victoria" : $"{wins} victorias",
                Where(best)));
        }

        // Mejor racha: la mayor de sus rachas por juego.
        var streak = perGame
            .Where(x => x.Stats.BestStreak > 0)
            .OrderByDescending(x => x.Stats.BestStreak)
            .FirstOrDefault();
        if (streak.Game is not null)
        {
            achievements.Add(new GameRecord(
                FluentUI.fire_24_regular,
                "Mejor racha",
                streak.Stats.BestStreak == 1 ? "1 sesión ganada" : $"{streak.Stats.BestStreak} sesiones ganadas seguidas",
                $"En {streak.Game.Name}"));
        }

        // Mayor paliza que dio: sesión cerrada ganada con la mayor diferencia sobre el segundo.
        var blowout = played
            .Where(s => !s.IsActive && s.SessionPlayers.Count >= 2 && PlayerGameStatsCalculator.WonSession(s, playerId))
            .Select(s =>
            {
                var runnerUp = s.SessionPlayers
                    .Where(sp => sp.PlayerID != playerId)
                    .OrderByDescending(sp => sp.Wins)
                    .First();
                return (Session: s, Mine: WinsIn(s), RunnerUp: runnerUp, Margin: WinsIn(s) - runnerUp.Wins);
            })
            .Where(x => x.Margin > 0)
            .OrderByDescending(x => x.Margin)
            .ThenByDescending(x => x.Session.SessionDate)
            .FirstOrDefault();
        if (blowout.Session is not null)
        {
            achievements.Add(new GameRecord(
                FluentUI.flash_24_regular,
                "Mayor paliza",
                $"Le sacó {blowout.Margin} a {blowout.RunnerUp.Player?.Name}",
                $"{blowout.Mine} a {blowout.RunnerUp.Wins} · {Where(blowout.Session)}"));
        }

        // Juego favorito y mejor juego: solo aportan algo con 2 o más juegos.
        if (perGame.Count >= 2)
        {
            var favorite = perGame
                .OrderByDescending(x => x.Stats.SessionsPlayed)
                .ThenByDescending(x => x.Stats.TotalWins)
                .First();
            achievements.Add(new GameRecord(
                FluentUI.games_24_regular,
                "Juego favorito",
                favorite.Game.Name,
                favorite.Stats.SessionsPlayed == 1 ? "1 sesión jugada" : $"{favorite.Stats.SessionsPlayed} sesiones jugadas"));

            var bestGame = perGame
                .Where(x => x.Stats.ClosedSessionsPlayed > 0 && x.Stats.SessionsWon > 0)
                .OrderByDescending(x => x.Stats.SessionWinRate)
                .ThenByDescending(x => x.Stats.SessionsWon)
                .FirstOrDefault();
            if (bestGame.Game is not null)
            {
                achievements.Add(new GameRecord(
                    FluentUI.ribbon_24_regular,
                    "Mejor juego",
                    bestGame.Game.Name,
                    $"Ganó {bestGame.Stats.SessionWinRate:P0} de sus sesiones ({bestGame.Stats.SessionsWon} de {bestGame.Stats.ClosedSessionsPlayed})"));
            }
        }

        return achievements;
    }
}
