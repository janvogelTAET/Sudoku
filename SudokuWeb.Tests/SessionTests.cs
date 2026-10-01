namespace SudokuWeb.Tests;

public class SessionTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);

    private readonly GameStats _stats = new();
    private readonly DailyProgress _daily = new();
    private readonly GameSession _session;

    public SessionTests()
    {
        _session = new GameSession(_stats, _daily);
        _session.Initialize(null, null, GameSlot.Free);
    }

    // Trägt in der ersten leeren Zelle die richtige Zahl ein -> der Stand zählt als "angefangen".
    private static void MakeMove(SudokuGame game)
    {
        var cell = game.Cells.First(c => c.IsEmpty);
        game.Select(cell);
        game.Enter(cell.Solution);
    }

    private static void Solve(SudokuGame game)
    {
        foreach (var c in game.Cells.Where(c => c.IsEmpty).ToList())
        {
            game.Select(c);
            game.Enter(c.Solution);
        }
    }

    [Fact]
    public void Initialize_WithoutSaves_StartsOneFreshGame_AndCountsItOnce()
    {
        Assert.Equal(GameSlot.Free, _session.ActiveSlot);
        Assert.Equal(1, _stats.TotalPlayed);
    }

    [Fact]
    public void StartingFreeGame_KeepsDailyProgress()
    {
        _session.StartOrResumeDaily(Today);
        MakeMove(_session.Game);
        var dailyValues = _session.Game.Cells.Select(c => c.Value).ToArray();

        _session.StartFree(Difficulty.Schwer);
        Assert.Equal(GameSlot.Free, _session.ActiveSlot);
        Assert.False(_session.Game.IsDaily);

        Assert.True(_session.StartOrResumeDaily(Today));
        Assert.Equal(dailyValues, _session.Game.Cells.Select(c => c.Value));
    }

    [Fact]
    public void SwitchingToDaily_AndBack_ResumesFreeGame()
    {
        MakeMove(_session.Game);
        var freeValues = _session.Game.Cells.Select(c => c.Value).ToArray();
        Assert.True(_session.FreeGameInProgress);

        _session.StartOrResumeDaily(Today);
        Assert.NotNull(_session.ParkedFreeGame);
        Assert.True(_session.FreeGameInProgress);          // das freie Spiel liegt weiter geparkt

        Assert.True(_session.ResumeFree());
        Assert.Equal(freeValues, _session.Game.Cells.Select(c => c.Value));
        Assert.Equal(GameSlot.Free, _session.ActiveSlot);
    }

    [Fact]
    public void NewFreeGame_FromDaily_DiscardsParkedFreeGame_ButNotDaily()
    {
        MakeMove(_session.Game);                            // freies Spiel angefangen
        _session.StartOrResumeDaily(Today);
        MakeMove(_session.Game);                            // Daily angefangen

        _session.StartFree(Difficulty.Einfach);

        Assert.False(_session.FreeGameInProgress);          // altes freies Spiel ist weg
        Assert.True(_session.DailyResumable(Today));        // Daily nicht
    }

    [Fact]
    public void FreeGameInProgress_IsFalseForUntouchedGame()
    {
        Assert.False(_session.FreeGameInProgress);
        _session.StartOrResumeDaily(Today);
        Assert.False(_session.FreeGameInProgress);
    }

    [Fact]
    public void Daily_IsCountedAsPlayedOnlyOncePerDay()
    {
        int before = _stats.TotalPlayed;

        _session.StartOrResumeDaily(Today);                 // frisch: +1
        MakeMove(_session.Game);
        _session.StartFree(Difficulty.Einfach);             // freies Spiel: +1
        _session.StartOrResumeDaily(Today);                 // Fortsetzen: +0
        _session.StartOrResumeDaily(Today);                 // läuft schon: +0

        Assert.Equal(before + 2, _stats.TotalPlayed);
        Assert.Equal(1, _stats.For(Difficulty.Mittel).Played);   // das Daily (Mittel), nur einmal
        Assert.Equal(2, _stats.For(Difficulty.Einfach).Played);  // Startspiel + neues freies Spiel
    }

    [Fact]
    public void Daily_StartedAgainAfterSlotLoss_IsStillNotRecounted()
    {
        _session.StartOrResumeDaily(Today);
        int played = _stats.TotalPlayed;

        // Speicher des Daily-Platzes ging verloren (z. B. Browser-Daten gelöscht), Statistik blieb.
        var fresh = new GameSession(_stats, _daily);
        fresh.Initialize(null, null, GameSlot.Free);       // zählt das freie Startspiel (+1)
        fresh.StartOrResumeDaily(Today);

        Assert.Equal(played + 1, _stats.TotalPlayed);
    }

    [Fact]
    public void ResumingAfterRestart_DoesNotCountAgain()
    {
        MakeMove(_session.Game);
        var saved = _session.SavedFor(GameSlot.Free);
        int played = _stats.TotalPlayed;

        var restarted = new GameSession(_stats, _daily);
        restarted.Initialize(saved, null, GameSlot.Free);

        Assert.Equal(played, _stats.TotalPlayed);
        Assert.Equal(_session.Game.Cells.Select(c => c.Value), restarted.Game.Cells.Select(c => c.Value));
    }

    [Fact]
    public void Initialize_MigratesDailyFoundInFreeSlot()
    {
        _session.StartOrResumeDaily(Today);
        MakeMove(_session.Game);
        var oldStyleSave = _session.SavedFor(GameSlot.Daily);   // früher landete auch das Daily im freien Platz

        var restarted = new GameSession(_stats, _daily);
        restarted.Initialize(oldStyleSave, null, GameSlot.Free);

        Assert.Equal(GameSlot.Daily, restarted.ActiveSlot);
        Assert.Equal(Today, restarted.Game.DailyDate);
    }

    [Fact]
    public void Initialize_FallsBackToOtherSlot_WhenPreferredIsEmpty()
    {
        _session.StartOrResumeDaily(Today);
        var dailySave = _session.SavedFor(GameSlot.Daily);

        var restarted = new GameSession(_stats, _daily);
        restarted.Initialize(null, dailySave, GameSlot.Free);

        Assert.Equal(GameSlot.Daily, restarted.ActiveSlot);
    }

    [Fact]
    public void Initialize_IgnoresBrokenSaves()
    {
        var restarted = new GameSession(new GameStats(), new DailyProgress());
        restarted.Initialize(new SavedGame { Values = [1] }, new SavedGame { DailyDate = "2026-10-10" }, GameSlot.Daily);

        Assert.True(restarted.Game.HasCells);
        Assert.Equal(GameSlot.Free, restarted.ActiveSlot);
    }

    [Fact]
    public void StaleDaily_IsReplacedWhenStartingToday()
    {
        var yesterday = Today.AddDays(-1);
        _session.StartOrResumeDaily(yesterday);
        MakeMove(_session.Game);

        _session.StartOrResumeDaily(Today);

        Assert.Equal(Today, _session.Game.DailyDate);
        Assert.Equal(0, _session.Game.Cells.Count(c => !c.IsFixed && c.Value != 0));
    }

    [Fact]
    public void Daily_AlreadySolvedToday_CannotBeStartedAgain()
    {
        _session.StartOrResumeDaily(Today);
        Solve(_session.Game);
        var result = _session.HandleWin(Today);

        Assert.NotNull(result);
        Assert.Equal(1, result.DailyStreak);
        Assert.False(_session.StartOrResumeDaily(Today));
    }

    [Fact]
    public void HandleWin_RecordsOnlyOnce()
    {
        _session.StartOrResumeDaily(Today);
        Solve(_session.Game);

        Assert.NotNull(_session.HandleWin(Today));
        Assert.Null(_session.HandleWin(Today));
        Assert.Equal(1, _stats.TotalWon);
        Assert.Equal(1, _daily.TotalSolved);
    }

    [Fact]
    public void SolvedGame_IsNotSavedAnymore()
    {
        _session.StartOrResumeDaily(Today);
        Solve(_session.Game);
        _session.HandleWin(Today);

        Assert.Null(_session.SavedFor(GameSlot.Daily));
    }
}
