using CompetitiveCounterApp.Services;
using static CompetitiveCounterApp.Tests.TestData;

namespace CompetitiveCounterApp.Tests;

public class StatsPeriodFilterTests
{
    static readonly DateTime Now = new(2026, 10, 28, 15, 0, 0);

    static readonly Models.Session[] Sessions =
    [
        Session(Day(10, 1), (Ana, 1)),
        Session(Day(10, 31), (Ana, 1)),
        Session(Day(9, 30), (Ana, 1)),
        Session(Day(1, 1), (Ana, 1)),
        Session(Day(12, 31, 2025), (Ana, 1)),
    ];

    [Theory]
    [InlineData(StatsPeriod.All, 5)]
    [InlineData(StatsPeriod.Year, 4)]
    [InlineData(StatsPeriod.Month, 2)]
    public void Apply_KeepsSessionsInThePeriod(StatsPeriod period, int expected)
    {
        Assert.Equal(expected, StatsPeriodFilter.Apply(Sessions, period, Now).Count);
    }

    [Fact]
    public void Apply_MonthUsesTheCalendarMonthNotTheLast30Days()
    {
        var month = StatsPeriodFilter.Apply(Sessions, StatsPeriod.Month, Now);

        Assert.All(month, s => Assert.Equal(10, s.SessionDate.Month));
        Assert.DoesNotContain(month, s => s.SessionDate == Day(9, 30));
    }

    [Fact]
    public void Apply_RankingIsRecomputedWithinThePeriod()
    {
        // En todo el historial gana Beto; este mes, Ana.
        var sessions = new[]
        {
            Session(Day(3, 1), (Ana, 0), (Beto, 3)),
            Session(Day(4, 1), (Ana, 0), (Beto, 3)),
            Session(Day(10, 5), (Ana, 2), (Beto, 1)),
        };

        var all = PlayerGameStatsCalculator.RankPlayers(StatsPeriodFilter.Apply(sessions, StatsPeriod.All, Now));
        var month = PlayerGameStatsCalculator.RankPlayers(StatsPeriodFilter.Apply(sessions, StatsPeriod.Month, Now));

        Assert.Equal(Beto.ID, all[0].Player.ID);
        Assert.Equal(Ana.ID, month[0].Player.ID);
    }
}
