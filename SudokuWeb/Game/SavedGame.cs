using System.Text.Json.Serialization;

namespace SudokuWeb.Game;

/// <summary>
/// Speicherbarer Spielstand (wird als JSON im localStorage abgelegt).
/// Arrays haben immer 81 Einträge, Notizen sind als Bitmaske pro Feld gespeichert
/// (Bit n gesetzt = Notiz n vorhanden).
/// </summary>
public class SavedGame
{
    public int Version { get; set; } = 1;
    public Difficulty Difficulty { get; set; }
    public int Seconds { get; set; }
    public bool NotesMode { get; set; }
    public int HintsUsed { get; set; }

    /// <summary>Datum (JJJJ-MM-TT) falls es das tägliche Rätsel ist, sonst null.</summary>
    public string? DailyDate { get; set; }

    public int[] Solution { get; set; } = [];
    public int[] Values { get; set; } = [];
    public bool[] Given { get; set; } = [];
    public bool[] Hinted { get; set; } = [];
    public int[] Notes { get; set; } = [];

    /// <summary>Wurde schon gespielt (Zeit gelaufen oder eigene Zahl/Notiz eingetragen)?</summary>
    [JsonIgnore]
    public bool HasProgress =>
        Seconds > 0 || Enumerable.Range(0, Math.Min(Values.Length, Given.Length))
            .Any(i => (Values[i] != 0 && !Given[i]) || (i < Notes.Length && Notes[i] != 0));

    /// <summary>Sieht der Spielstand vollständig und plausibel aus?</summary>
    [JsonIgnore]
    public bool IsValid =>
        Solution.Length == 81 && Values.Length == 81 && Given.Length == 81 &&
        Hinted.Length == 81 && Notes.Length == 81 &&
        Solution.All(v => v is >= 1 and <= 9) && Values.All(v => v is >= 0 and <= 9);
}
