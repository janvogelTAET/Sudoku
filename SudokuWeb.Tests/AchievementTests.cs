namespace SudokuWeb.Tests;

public class AchievementTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);

    private static WinInfo Win(
        Difficulty difficulty = Difficulty.Einfach, int seconds = 600, int hints = 1,
        bool daily = false, int streak = 0, int totalWon = 1)
        => new(difficulty, seconds, hints, daily, streak, totalWon);

    private static string[] Ids(IEnumerable<Achievement> list) => list.Select(a => a.Id).ToArray();

    [Fact]
    public void FirstWin_UnlocksFirstWinOnly_WhenNothingSpecial()
    {
        var book = new AchievementBook();
        var earned = Achievements.Unlock(book, Win(), Today);

        Assert.Equal([Achievements.FirstWin], Ids(earned));
    }

    [Fact]
    public void AlreadyUnlocked_AreNotReportedAgain()
    {
        var book = new AchievementBook();
        Achievements.Unlock(book, Win(), Today);

        Assert.Empty(Achievements.Unlock(book, Win(), Today.AddDays(1)));
        Assert.Equal("2026-10-10", book.Unlocked[Achievements.FirstWin]);   // erstes Datum bleibt
    }

    [Fact]
    public void FirstDaily_AndStreakMilestones()
    {
        var book = new AchievementBook();

        Assert.Contains(Achievements.FirstDaily, Ids(Achievements.Unlock(book, Win(daily: true, streak: 1), Today)));
        Assert.False(book.IsUnlocked(Achievements.StreakId(3)));

        Assert.Contains(Achievements.StreakId(3), Ids(Achievements.Unlock(book, Win(daily: true, streak: 3), Today)));
        var seven = Ids(Achievements.Unlock(book, Win(daily: true, streak: 7), Today));
        Assert.Equal([Achievements.StreakId(7)], seven);

        Achievements.Unlock(book, Win(daily: true, streak: 100), Today);
        Assert.All(new[] { 14, 30, 100 }, d => Assert.True(book.IsUnlocked(Achievements.StreakId(d))));
    }

    [Fact]
    public void StreakBadges_NeedADailyWin()
    {
        var book = new AchievementBook();
        Achievements.Unlock(book, Win(daily: false, streak: 50), Today);

        Assert.False(book.IsUnlocked(Achievements.StreakId(3)));
        Assert.False(book.IsUnlocked(Achievements.FirstDaily));
    }

    [Fact]
    public void Expert_NoHints_AndSpeed()
    {
        var book = new AchievementBook();

        var expert = Ids(Achievements.Unlock(book, Win(Difficulty.Experte, hints: 2), Today));
        Assert.Contains(Achievements.FirstExpert, expert);
        Assert.DoesNotContain(Achievements.NoHints, expert);

        var clean = Ids(Achievements.Unlock(book, Win(Difficulty.Schwer, hints: 0), Today));
        Assert.Contains(Achievements.NoHints, clean);

        var speedy = Ids(Achievements.Unlock(book, Win(Difficulty.Mittel, seconds: 299, hints: 0), Today));
        Assert.Contains(Achievements.SpeedMittel, speedy);
    }

    [Theory]
    [InlineData(Difficulty.Mittel, 300, 0)]     // genau 5 Minuten reichen nicht
    [InlineData(Difficulty.Mittel, 200, 1)]     // mit Tipp zählt nicht
    [InlineData(Difficulty.Einfach, 100, 0)]    // falsche Stufe
    [InlineData(Difficulty.Mittel, 0, 0)]       // keine gültige Zeit
    public void Speed_Requirements(Difficulty difficulty, int seconds, int hints)
    {
        var book = new AchievementBook();
        Achievements.Unlock(book, Win(difficulty, seconds, hints), Today);

        Assert.False(book.IsUnlocked(Achievements.SpeedMittel));
    }

    [Fact]
    public void TenWins()
    {
        var book = new AchievementBook();
        Achievements.Unlock(book, Win(totalWon: 9), Today);
        Assert.False(book.IsUnlocked(Achievements.TenWins));

        Achievements.Unlock(book, Win(totalWon: 10), Today);
        Assert.True(book.IsUnlocked(Achievements.TenWins));
    }

    [Fact]
    public void Session_ReportsNewAchievementsOnWin_AndSurvivesJson()
    {
        var book = new AchievementBook();
        var session = new GameSession(new GameStats(), new DailyProgress(), book);
        session.Initialize(null, null, GameSlot.Free);
        session.StartOrResumeDaily(Today);
        foreach (var c in session.Game.Cells.Where(c => c.IsEmpty).ToList())
        {
            session.Game.Select(c);
            session.Game.Enter(c.Solution);
        }

        var result = session.HandleWin(Today);

        Assert.NotNull(result);
        Assert.Contains(Achievements.FirstDaily, Ids(result.NewAchievements));
        Assert.Contains(Achievements.FirstWin, Ids(result.NewAchievements));

        string json = System.Text.Json.JsonSerializer.Serialize(book, SudokuWeb.Services.AppJsonContext.Default.AchievementBook);
        var loaded = System.Text.Json.JsonSerializer.Deserialize(json, SudokuWeb.Services.AppJsonContext.Default.AchievementBook)!;
        Assert.True(loaded.IsUnlocked(Achievements.FirstDaily));
    }

    [Fact]
    public void AllAchievements_HaveUniqueIds()
    {
        var ids = Achievements.All.Select(a => a.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
