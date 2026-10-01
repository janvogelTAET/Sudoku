using System.Globalization;
using System.Text.Json.Serialization;

namespace SudokuWeb.Game;

/// <summary>
/// Merkt sich, an welchen Tagen das tägliche Rätsel gelöst wurde, und berechnet
/// daraus die Serie ("Streak"). Die Serie wird immer aus den gelösten Tagen
/// abgeleitet – dadurch kann sie nie "verrutschen" und doppeltes Speichern
/// am selben Tag ist harmlos.
/// </summary>
public class DailyProgress
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>Gelöste Tage (JJJJ-MM-TT) mit der benötigten Zeit in Sekunden.</summary>
    public Dictionary<string, int> SolvedDays { get; set; } = new();

    public bool IsSolved(DateOnly day) => SolvedDays.ContainsKey(Key(day));

    /// <summary>Benötigte Sekunden für den Tag (null = nicht gelöst).</summary>
    public int? SecondsFor(DateOnly day) => SolvedDays.TryGetValue(Key(day), out var s) ? s : null;

    [JsonIgnore]
    public int TotalSolved => SolvedDays.Count;

    /// <summary>Trägt den Tag als gelöst ein. Gibt false zurück, wenn er schon eingetragen war.</summary>
    public bool MarkSolved(DateOnly day, int seconds)
    {
        string key = Key(day);
        if (SolvedDays.ContainsKey(key)) return false;
        SolvedDays[key] = seconds;
        return true;
    }

    /// <summary>
    /// Aktuelle Serie: aufeinanderfolgende gelöste Tage bis heute. Ist heute noch
    /// nicht gelöst, zählt die Serie bis gestern weiter (der Tag ist ja noch nicht vorbei).
    /// Wurde gestern verpasst, ist sie 0.
    /// </summary>
    public int CurrentStreak(DateOnly today)
    {
        var day = IsSolved(today) ? today : today.AddDays(-1);
        int streak = 0;
        while (IsSolved(day))
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }

    /// <summary>Längste Serie aller Zeiten.</summary>
    [JsonIgnore]
    public int BestStreak
    {
        get
        {
            int best = 0, run = 0;
            DateOnly? previous = null;
            foreach (var day in Days().Order())
            {
                run = previous is { } p && p.AddDays(1) == day ? run + 1 : 1;
                best = Math.Max(best, run);
                previous = day;
            }
            return best;
        }
    }

    private IEnumerable<DateOnly> Days() => SolvedDays.Keys
        .Select(k => DateOnly.TryParseExact(k, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateOnly?)null)
        .OfType<DateOnly>();

    private static string Key(DateOnly day) => day.ToString(DateFormat, CultureInfo.InvariantCulture);
}
