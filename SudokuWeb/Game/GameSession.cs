namespace SudokuWeb.Game;

/// <summary>Die zwei Speicherplätze: das freie Spiel und das tägliche Rätsel.</summary>
public enum GameSlot { Free, Daily }

/// <summary>Was nach einem Sieg verbucht wurde (für den Gewinn-Dialog).</summary>
public record WinResult(bool NewRecord, int? DailyStreak, IReadOnlyList<Achievement> NewAchievements);

/// <summary>
/// Verwaltet das aktive Spiel und die zwei Speicherplätze. Das freie Spiel und das tägliche
/// Rätsel liegen getrennt: Wer zwischen beiden wechselt, nimmt immer genau dort wieder auf,
/// wo er aufgehört hat. Hier entscheidet sich auch, wann ein Spiel als "gespielt" zählt:
/// nur wenn es frisch beginnt, nie beim Fortsetzen.
/// </summary>
public class GameSession(GameStats stats, DailyProgress daily, AchievementBook? achievements = null)
{
    private readonly AchievementBook _achievements = achievements ?? new AchievementBook();

    // Der jeweils NICHT aktive Platz liegt hier "geparkt". Der aktive steckt in Game.
    private readonly Dictionary<GameSlot, SavedGame?> _parked = new()
    {
        [GameSlot.Free] = null,
        [GameSlot.Daily] = null,
    };

    private bool _winRecorded;

    public SudokuGame Game { get; } = new();

    public GameSlot ActiveSlot => Game.IsDaily ? GameSlot.Daily : GameSlot.Free;

    /// <summary>Der gespeicherte Stand eines Platzes (null = leer oder bereits gelöst).</summary>
    public SavedGame? SavedFor(GameSlot slot)
        => slot == ActiveSlot
            ? (Game.HasCells && !Game.Solved ? Game.ToSaved() : null)
            : _parked[slot];

    /// <summary>
    /// Würde ein neues freies Spiel einen angefangenen Stand verwerfen?
    /// (Das tägliche Rätsel bleibt davon unberührt.)
    /// </summary>
    public bool FreeGameInProgress => SavedFor(GameSlot.Free)?.HasProgress == true;

    /// <summary>Der freie Stand, der beim Wechsel zum Daily geparkt wurde (zum Fortsetzen anbieten).</summary>
    public SavedGame? ParkedFreeGame => ActiveSlot == GameSlot.Daily ? _parked[GameSlot.Free] : null;

    /// <summary>Gibt es für heute ein angefangenes tägliches Rätsel, das man fortsetzen kann?</summary>
    public bool DailyResumable(DateOnly today)
        => SavedFor(GameSlot.Daily) is { } saved && IsDateOf(saved, today);

    // ---- Start ------------------------------------------------------------

    /// <summary>
    /// Lädt die gespeicherten Plätze beim App-Start und setzt den zuletzt aktiven fort.
    /// Alte Spielstände (vor den getrennten Plätzen) lagen immer im freien Platz – ein
    /// darin gespeichertes Daily wandert in den Daily-Platz.
    /// </summary>
    public void Initialize(SavedGame? free, SavedGame? dailySaved, GameSlot preferred)
    {
        if (free is { IsValid: true, DailyDate: not null })
        {
            dailySaved ??= free;
            free = null;
        }

        _parked[GameSlot.Free] = free is { IsValid: true } ? free : null;
        _parked[GameSlot.Daily] = dailySaved is { IsValid: true, DailyDate: not null } ? dailySaved : null;

        var other = preferred == GameSlot.Free ? GameSlot.Daily : GameSlot.Free;
        foreach (var slot in new[] { preferred, other })
        {
            if (_parked[slot] is not { } saved) continue;
            _parked[slot] = null;
            if (Game.Restore(saved))
            {
                _winRecorded = false;
                return;
            }
        }

        StartFree(Difficulty.Einfach);
    }

    /// <summary>Startet ein neues freies Spiel. Der Daily-Platz bleibt erhalten, ein freier Stand wird verworfen.</summary>
    public void StartFree(Difficulty difficulty)
    {
        Park();
        _parked[GameSlot.Free] = null;

        Game.NewGame(difficulty);
        _winRecorded = false;
        stats.RecordStarted(difficulty);
    }

    /// <summary>Setzt das beim Wechsel zum Daily geparkte freie Spiel fort.</summary>
    public bool ResumeFree()
    {
        if (ActiveSlot != GameSlot.Daily || _parked[GameSlot.Free] is not { } saved) return false;

        Park();
        _parked[GameSlot.Free] = null;
        if (!Game.Restore(saved)) return false;
        _winRecorded = false;
        return true;
    }

    /// <summary>
    /// Wechselt zum heutigen Daily: setzt es fort, falls angefangen, sonst beginnt es neu.
    /// Gibt false zurück, wenn es heute schon gelöst wurde.
    /// </summary>
    public bool StartOrResumeDaily(DateOnly today)
    {
        if (daily.IsSolved(today)) return false;
        if (Game.IsDaily && Game.DailyDate == today && !Game.Solved) return true;   // läuft schon

        Park();
        var saved = _parked[GameSlot.Daily];
        _parked[GameSlot.Daily] = null;

        if (saved is not null && IsDateOf(saved, today) && Game.Restore(saved))
        {
            _winRecorded = false;
            return true;
        }

        // Frischer Start (ein Stand von gestern wird dabei verworfen).
        Game.StartDaily(today);
        _winRecorded = false;
        stats.RecordDailyStarted(today);
        return true;
    }

    // ---- Sieg -------------------------------------------------------------

    /// <summary>
    /// Verbucht einen gelösten Stand (Statistik, Tages-Serie). Gibt null zurück, wenn das Spiel
    /// nicht gelöst ist oder der Sieg schon verbucht wurde.
    /// </summary>
    public WinResult? HandleWin(DateOnly today)
    {
        if (!Game.Solved || _winRecorded) return null;
        _winRecorded = true;

        bool record = stats.RecordWin(Game.Difficulty, Game.Seconds, Game.HintsUsed);

        int? streak = null;
        if (Game.DailyDate is { } day)
        {
            daily.MarkSolved(day, Game.Seconds);
            streak = daily.CurrentStreak(today);
        }

        var info = new WinInfo(Game.Difficulty, Game.Seconds, Game.HintsUsed, Game.IsDaily, streak ?? 0, stats.TotalWon);
        return new WinResult(record, streak, Achievements.Unlock(_achievements, info, today));
    }

    // ---- intern -----------------------------------------------------------

    // Legt das aktive Spiel in seinem Platz ab (gelöste Spiele nicht).
    private void Park()
    {
        _parked[ActiveSlot] = Game.HasCells && !Game.Solved ? Game.ToSaved() : null;
    }

    private static bool IsDateOf(SavedGame saved, DateOnly day)
        => DateOnly.TryParseExact(saved.DailyDate, "yyyy-MM-dd", out var d) && d == day;
}
