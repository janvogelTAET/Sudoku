namespace SudokuWeb.Game;

/// <summary>Kleine Anzeige-Helfer (deutsche Texte) für Zeit, Schwierigkeit und Datum.</summary>
public static class GameText
{
    /// <summary>Sekunden als "m:ss" bzw. "h:mm:ss".</summary>
    public static string Time(int seconds)
        => seconds >= 3600
            ? $"{seconds / 3600}:{seconds / 60 % 60:00}:{seconds % 60:00}"
            : $"{seconds / 60}:{seconds % 60:00}";

    /// <summary>Zeit oder Strich, wenn es (noch) keine gibt.</summary>
    public static string TimeOrDash(int seconds) => seconds > 0 ? Time(seconds) : "–";

    public static string Emoji(this Difficulty d) => d switch
    {
        Difficulty.Einfach => "😊",
        Difficulty.Mittel => "🙂",
        Difficulty.Schwer => "😤",
        Difficulty.Experte => "🧠",
        _ => ""
    };

    public static string Tagline(this Difficulty d) => d switch
    {
        Difficulty.Einfach => "Zum Entspannen",
        Difficulty.Mittel => "Ein bisschen Nachdenken",
        Difficulty.Schwer => "Knifflig",
        Difficulty.Experte => "Nur für Profis",
        _ => ""
    };

    private static readonly string[] Weekdays = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];

    public static string Weekday(int mondayBasedIndex) => Weekdays[mondayBasedIndex];

    /// <summary>0 = Montag ... 6 = Sonntag.</summary>
    public static int MondayIndex(this DateOnly date) => ((int)date.DayOfWeek + 6) % 7;

    public static string Short(this DateOnly date) => $"{date.Day}.{date.Month}.";
}
