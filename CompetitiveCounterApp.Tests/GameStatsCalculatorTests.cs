using CompetitiveCounterApp.Services;
using static CompetitiveCounterApp.Tests.TestData;

namespace CompetitiveCounterApp.Tests;

public class GameStatsCalculatorTests
{
    [Fact]
    public void Compute_TotalsIncludeActiveSessionButResultsOnlyClosed()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 3), (Beto, 1)),
            Session(Day(10, 2), (Ana, 1), (Beto, 2)),
            Session(Day(10, 3), active: true, (Ana, 5), (Beto, 0)),
            Session(Day(10, 4), (Ana, 0), (Beto, 0)), // sin victorias: no cuenta
        };

        var stats = GameStatsCalculator.Compute(sessions);

        Assert.Equal(3, stats.SessionsPlayed);
        Assert.Equal(2, stats.ClosedSessions);
        Assert.Equal(12, stats.TotalWins);
        Assert.Equal(2, stats.DistinctPlayers);
        Assert.Equal(Day(10, 1), stats.FirstSessionDate);
        Assert.Equal(Day(10, 3), stats.LastSessionDate);
    }

    [Fact]
    public void Compute_RankingMatchesRankPlayers()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 1), (Beto, 3)),
            Session(Day(10, 2), (Ana, 1), (Beto, 3)),
            Session(Day(10, 3), (Ana, 9), (Beto, 0)),
        };

        var stats = GameStatsCalculator.Compute(sessions);

        Assert.Equal(
            PlayerGameStatsCalculator.RankPlayers(sessions).Select(r => r.Player.ID),
            stats.Ranking.Select(r => r.Player.ID));
        Assert.Equal([1, 2], stats.Ranking.Select(r => r.Rank));
    }

    [Fact]
    public void Compute_WinsShareCanHaveADifferentLeader()
    {
        var sessions = new[]
        {
            Session(Day(10, 1), (Ana, 1), (Beto, 3)),
            Session(Day(10, 2), (Ana, 1), (Beto, 3)),
            Session(Day(10, 3), (Ana, 9), (Beto, 0)),
        };

        var stats = GameStatsCalculator.Compute(sessions);

        Assert.Equal(Beto.ID, stats.SessionsWonShare[0].Player.ID);
        Assert.Equal(Ana.ID, stats.WinsShare[0].Player.ID);
    }

    [Fact]
    public void Compute_EmptyWhenThereAreNoWins()
    {
        var stats = GameStatsCalculator.Compute([Session(Day(10, 1), (Ana, 0))]);

        Assert.Equal(0, stats.SessionsPlayed);
        Assert.Empty(stats.Ranking);
        Assert.Null(stats.FirstSessionDate);
    }
}
