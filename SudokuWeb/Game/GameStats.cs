using System.Text.Json.Serialization;

namespace SudokuWeb.Game;

/// <summary>Statistik für eine Schwierigkeitsstufe.</summary>
public class DifficultyStats
{
    public int Played { get; set; }
    public int Won { get; set; }

    /// <summary>Beste Zeit in Sekunden (0 = noch keine). Zählt nur Spiele ohne Tipp.</summary>
    public int BestSeconds { get; set; }

    public int TotalSeconds { get; set; }

    [JsonIgnore]
    public int AverageSeconds => Won == 0 ? 0 : TotalSeconds / Won;
}

/// <summary>Gesamtstatistik, getrennt nach Schwierigkeit.</summary>
public class GameStats
{
    /// <summary>Schlüssel ist der Name der Schwierigkeit (z. B. "Mittel").</summary>
    public Dictionary<string, DifficultyStats> ByDifficulty { get; set; } = new();

    public DifficultyStats For(Difficulty difficulty)
    {
        string key = difficulty.ToString();
        if (!ByDifficulty.TryGetValue(key, out var stats))
            ByDifficulty[key] = stats = new DifficultyStats();
        return stats;
    }

    [JsonIgnore]
    public int TotalPlayed => ByDifficulty.Values.Sum(s => s.Played);
    [JsonIgnore]
    public int TotalWon => ByDifficulty.Values.Sum(s => s.Won);

    public void RecordStarted(Difficulty difficulty) => For(difficulty).Played++;

    /// <summary>Verbucht einen Sieg. Gibt true zurück, wenn es eine neue Bestzeit ist.</summary>
    public bool RecordWin(Difficulty difficulty, int seconds, int hintsUsed)
    {
        var stats = For(difficulty);
        stats.Won++;
        stats.TotalSeconds += seconds;

        bool isRecord = hintsUsed == 0 && seconds > 0 && (stats.BestSeconds == 0 || seconds < stats.BestSeconds);
        if (isRecord) stats.BestSeconds = seconds;
        return isRecord;
    }
}
