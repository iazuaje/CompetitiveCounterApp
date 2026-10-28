using CompetitiveCounterApp.Services;
using static CompetitiveCounterApp.Tests.TestData;

namespace CompetitiveCounterApp.Tests;

public class PlayerGameStatsCalculatorTests
{
    [Fact]
    public void RankPlayers_OrdersBySessionsWonBeforeTotalWins()
    {
        // Beto suma más victorias, pero Ana ganó más sesiones.
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 3), (Beto, 2)),
            Session(Day(10, 2), (Ana, 4), (Beto, 1)),
            Session(Day(10, 3), (Ana, 1), (Beto, 10)),
        };

        var ranking = PlayerGameStatsCalculator.RankPlayers(sessions);

        Assert.Equal(["Ana", "Beto"], ranking.Select(r => r.Player.Name));
        Assert.Equal(2, ranking[0].SessionsWon);
        Assert.Equal(13, ranking[1].TotalWins);
    }

    [Fact]
    public void RankPlayers_BreaksSessionTiesByTotalWins()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Carla, 2), (Beto, 1)),
            Session(Day(10, 2), (Beto, 3), (Carla, 1)),
            Session(Day(10, 3), (Ana, 2), (Beto, 0)),
        };

        var ranking = PlayerGameStatsCalculator.RankPlayers(sessions);

        // Los tres ganaron 1 sesión: decide el total de victorias (4, 3 y 2).
        Assert.Equal(["Beto", "Carla", "Ana"], ranking.Select(r => r.Player.Name));
    }

    [Fact]
    public void RankPlayers_BreaksFullTiesByName()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Beto, 0), (Ana, 2)),
            Session(Day(10, 2), (Beto, 2), (Ana, 0)),
        };

        var ranking = PlayerGameStatsCalculator.RankPlayers(sessions);

        Assert.Equal(["Ana", "Beto"], ranking.Select(r => r.Player.Name));
    }

    [Fact]
    public void RankPlayers_IgnoresActiveSessionForSessionsWonButCountsItsWins()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 2), (Beto, 1)),
            Session(Day(10, 2), active: true, (Ana, 0), (Beto, 5)),
        };

        var ranking = PlayerGameStatsCalculator.RankPlayers(sessions);

        var beto = ranking.Single(r => r.Player.ID == Beto.ID);
        Assert.Equal(0, beto.SessionsWon);
        Assert.Equal(6, beto.TotalWins);
        Assert.Equal("Ana", ranking[0].Player.Name);
    }

    [Fact]
    public void RankPlayers_ExcludesPlayersWithoutWins()
    {
        var ranking = PlayerGameStatsCalculator.RankPlayers([Session(Day(10, 1), (Ana, 2), (Beto, 0))]);

        Assert.Single(ranking);
    }

    [Fact]
    public void WonSession_TieAtMaximumCountsForEveryTiedPlayer()
    {
        var session = Session(Day(10, 1), (Ana, 3), (Beto, 3), (Carla, 1));

        Assert.True(PlayerGameStatsCalculator.WonSession(session, Ana.ID));
        Assert.True(PlayerGameStatsCalculator.WonSession(session, Beto.ID));
        Assert.False(PlayerGameStatsCalculator.WonSession(session, Carla.ID));
    }

    [Fact]
    public void WonSession_NobodyWinsASessionWithoutWins()
    {
        var session = Session(Day(10, 1), (Ana, 0), (Beto, 0));

        Assert.False(PlayerGameStatsCalculator.WonSession(session, Ana.ID));
    }

    [Theory]
    [InlineData(new bool[0], 0, 0)]
    [InlineData(new[] { true, true, false, true }, 1, 2)]
    [InlineData(new[] { false, true, true, true }, 3, 3)]
    [InlineData(new[] { true, false, false }, 0, 1)]
    public void Streaks_ReturnsCurrentAndBest(bool[] won, int current, int best)
    {
        Assert.Equal((current, best), PlayerGameStatsCalculator.Streaks(won));
    }

    [Fact]
    public void Compute_SkipsSessionsWithoutWinsAndSummarizesThePlayer()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 3), (Beto, 1)),
            Session(Day(10, 2), (Ana, 0), (Beto, 0)), // sin victorias: no cuenta
            Session(Day(10, 3), (Ana, 1), (Beto, 2)),
            Session(Day(10, 4), (Ana, 4), (Beto, 4)), // empate: ganada por ambos
        };

        var stats = PlayerGameStatsCalculator.Compute(sessions, Ana.ID);

        Assert.Equal(3, stats.SessionsPlayed);
        Assert.Equal(8, stats.TotalWins);
        Assert.Equal(2, stats.SessionsWon);
        Assert.Equal(1, stats.CurrentStreak);
        Assert.Equal(4, stats.BestSessionWins);
        Assert.Equal(Day(10, 4), stats.BestSessionDate);
        Assert.Equal(1, stats.Rank);
        Assert.Equal(8.0 / 15, stats.WinShare, precision: 6);
    }

    [Fact]
    public void Compute_HeadToHeadPicksNemesisAndVictim()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 1), (Beto, 3), (Carla, 0)),
            Session(Day(10, 2), (Ana, 1), (Beto, 2), (Carla, 0)),
            Session(Day(10, 3), (Ana, 2), (Beto, 1), (Carla, 1)),
        };

        var stats = PlayerGameStatsCalculator.Compute(sessions, Ana.ID);

        Assert.Equal(Beto.ID, stats.Nemesis?.Player.ID);
        Assert.Equal(Carla.ID, stats.Victim?.Player.ID);
    }

    [Fact]
    public void Compute_SameRivalIsOnlyNemesisOrVictimByBalance()
    {
        // Contra Beto: 2 ganados y 1 perdido → saldo a favor, solo víctima.
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 2), (Beto, 1)),
            Session(Day(10, 2), (Ana, 3), (Beto, 1)),
            Session(Day(10, 3), (Ana, 0), (Beto, 1)),
        };

        var stats = PlayerGameStatsCalculator.Compute(sessions, Ana.ID);

        Assert.Null(stats.Nemesis);
        Assert.Equal(Beto.ID, stats.Victim?.Player.ID);
    }
}
