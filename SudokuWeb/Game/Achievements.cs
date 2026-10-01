using System.Globalization;

namespace SudokuWeb.Game;

/// <summary>Ein Abzeichen, das man freischalten kann.</summary>
public record Achievement(string Id, string Emoji, string Title, string Description);

/// <summary>Die Angaben zu einem gerade gewonnenen Spiel, aus denen sich Abzeichen ergeben.</summary>
public record WinInfo(
    Difficulty Difficulty, int Seconds, int HintsUsed,
    bool IsDaily, int DailyStreak, int TotalWon);

/// <summary>Welche Abzeichen schon freigeschaltet sind (mit Datum). Wird im localStorage gespeichert.</summary>
public class AchievementBook
{
    /// <summary>Id des Abzeichens -> Datum der Freischaltung (JJJJ-MM-TT).</summary>
    public Dictionary<string, string> Unlocked { get; set; } = new();

    public bool IsUnlocked(string id) => Unlocked.ContainsKey(id);

    /// <summary>Schaltet frei; gibt false zurück, wenn es schon freigeschaltet war.</summary>
    public bool Unlock(string id, DateOnly day)
        => Unlocked.TryAdd(id, day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

/// <summary>Alle Abzeichen und die Regeln, wann sie freigeschaltet werden.</summary>
public static class Achievements
{
    public const string FirstWin = "first-win";
    public const string TenWins = "ten-wins";
    public const string FirstDaily = "first-daily";
    public const string FirstExpert = "first-expert";
    public const string NoHints = "no-hints";
    public const string SpeedMittel = "speed-mittel";

    /// <summary>Minuten-Grenze für "Blitzschnell" (Mittel, ohne Tipp).</summary>
    public const int SpeedLimitSeconds = 5 * 60;

    private static readonly int[] StreakSteps = [3, 7, 14, 30, 100];

    public static string StreakId(int days) => $"streak-{days}";

    public static IReadOnlyList<Achievement> All { get; } =
    [
        new(FirstWin, "🌱", "Erster Sieg", "Dein erstes Sudoku gelöst"),
        new(FirstDaily, "☀️", "Tagesstart", "Das erste tägliche Rätsel gelöst"),
        new(StreakId(3), "🔥", "Kleiner Funke", "3 Tage in Folge"),
        new(StreakId(7), "🌟", "Wochenheldin", "7 Tage in Folge"),
        new(StreakId(14), "💎", "Zwei Wochen", "14 Tage in Folge"),
        new(StreakId(30), "👑", "Monatskönigin", "30 Tage in Folge"),
        new(StreakId(100), "🏆", "Hundert Tage", "100 Tage in Folge"),
        new(NoHints, "🦉", "Ohne Hilfe", "Ein Sudoku ohne Tipp gelöst"),
        new(SpeedMittel, "⚡", "Blitzschnell", "Mittel unter 5 Minuten, ohne Tipp"),
        new(FirstExpert, "🧠", "Köpfchen", "Das erste Experten-Sudoku gelöst"),
        new(TenWins, "🌸", "Rätselfreundin", "10 Sudokus gelöst"),
    ];

    /// <summary>
    /// Prüft alle Regeln für den Sieg und schaltet neu erfüllte Abzeichen im Buch frei.
    /// Gibt nur die NEU freigeschalteten zurück (in der Reihenfolge von <see cref="All"/>).
    /// </summary>
    public static IReadOnlyList<Achievement> Unlock(AchievementBook book, WinInfo win, DateOnly today)
    {
        var earned = new HashSet<string> { FirstWin };

        if (win.TotalWon >= 10) earned.Add(TenWins);
        if (win.HintsUsed == 0) earned.Add(NoHints);
        if (win.Difficulty == Difficulty.Experte) earned.Add(FirstExpert);
        if (win.Difficulty == Difficulty.Mittel && win.HintsUsed == 0 && win.Seconds > 0 && win.Seconds < SpeedLimitSeconds)
            earned.Add(SpeedMittel);

        if (win.IsDaily)
        {
            earned.Add(FirstDaily);
            foreach (int step in StreakSteps.Where(s => win.DailyStreak >= s))
                earned.Add(StreakId(step));
        }

        return All.Where(a => earned.Contains(a.Id) && book.Unlock(a.Id, today)).ToList();
    }
}
