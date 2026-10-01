namespace SudokuWeb.Tests;

public class StreakTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);

    private static DailyProgress Solved(params int[] daysAgo)
    {
        var progress = new DailyProgress();
        foreach (int d in daysAgo) progress.MarkSolved(Today.AddDays(-d), 100);
        return progress;
    }

    [Fact]
    public void NothingSolved_StreakIsZero()
    {
        var p = new DailyProgress();
        Assert.Equal(0, p.CurrentStreak(Today));
        Assert.Equal(0, p.BestStreak);
    }

    [Fact]
    public void FirstSolve_StartsStreakAtOne()
    {
        Assert.Equal(1, Solved(0).CurrentStreak(Today));
    }

    [Fact]
    public void YesterdaySolved_ThenToday_IncrementsStreak()
    {
        var p = Solved(2, 1);
        Assert.Equal(2, p.CurrentStreak(Today));           // heute noch offen: Serie lebt weiter

        p.MarkSolved(Today, 90);
        Assert.Equal(3, p.CurrentStreak(Today));
    }

    [Fact]
    public void MissedDay_ResetsStreak_ButKeepsBest()
    {
        var p = Solved(5, 4, 3);                           // 3 Tage, dann Luecke (gestern verpasst)
        Assert.Equal(0, p.CurrentStreak(Today));
        Assert.Equal(3, p.BestStreak);

        p.MarkSolved(Today, 80);                           // heute wieder angefangen
        Assert.Equal(1, p.CurrentStreak(Today));
        Assert.Equal(3, p.BestStreak);
    }

    [Fact]
    public void SolvingSameDayTwice_DoesNotDoubleCount()
    {
        var p = Solved(1);
        Assert.True(p.MarkSolved(Today, 60));
        Assert.False(p.MarkSolved(Today, 30));

        Assert.Equal(2, p.CurrentStreak(Today));
        Assert.Equal(60, p.SecondsFor(Today));             // erste Zeit bleibt stehen
        Assert.Equal(2, p.TotalSolved);
    }

    [Fact]
    public void BestStreak_PicksLongestRun()
    {
        var p = Solved(20, 19, 10, 9, 8, 7, 1, 0);
        Assert.Equal(4, p.BestStreak);
        Assert.Equal(2, p.CurrentStreak(Today));
    }

    [Fact]
    public void Streak_WorksAcrossMonthAndYearBoundary()
    {
        var p = new DailyProgress();
        p.MarkSolved(new DateOnly(2026, 12, 30), 1);
        p.MarkSolved(new DateOnly(2026, 12, 31), 1);
        p.MarkSolved(new DateOnly(2027, 1, 1), 1);
        Assert.Equal(3, p.CurrentStreak(new DateOnly(2027, 1, 1)));
    }
}
