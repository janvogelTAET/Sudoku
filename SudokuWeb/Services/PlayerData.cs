using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SudokuWeb.Game;

namespace SudokuWeb.Services;

/// <summary>
/// Alles, was dauerhaft gespeichert wird: Spielstand, Statistik und Tages-Serie.
/// Liegt im localStorage des Browsers (auf dem iPhone also in der installierten App).
/// </summary>
public class PlayerData(BrowserInterop browser)
{
    private const string FreeGameKey = "sudoku.game";           // Schlüssel aus der ersten Version beibehalten
    private const string DailyGameKey = "sudoku.daily-game";
    private const string ActiveSlotKey = "sudoku.active-slot";
    private const string StatsKey = "sudoku.stats";
    private const string DailyKey = "sudoku.daily";

    public GameStats Stats { get; private set; } = new();
    public DailyProgress Daily { get; private set; } = new();

    /// <summary>Lädt Statistik und Serie (einmal beim Start).</summary>
    public async Task LoadAsync()
    {
        Stats = await LoadAsync(StatsKey, AppJsonContext.Default.GameStats) ?? new GameStats();
        Daily = await LoadAsync(DailyKey, AppJsonContext.Default.DailyProgress) ?? new DailyProgress();
    }

    // ---- Spielstände: zwei getrennte Plätze (frei / täglich) ----

    /// <summary>Lädt beide Plätze und merkt sich, welcher zuletzt aktiv war.</summary>
    public async Task<(SavedGame? Free, SavedGame? Daily, GameSlot Active)> LoadSlotsAsync()
    {
        var free = await LoadAsync(FreeGameKey, AppJsonContext.Default.SavedGame);
        var daily = await LoadAsync(DailyGameKey, AppJsonContext.Default.SavedGame);
        string? active = await browser.GetItemAsync(ActiveSlotKey);
        return (free, daily, active == nameof(GameSlot.Daily) ? GameSlot.Daily : GameSlot.Free);
    }

    /// <summary>Speichert einen Platz (oder löscht ihn, wenn er leer ist).</summary>
    public Task SaveSlotAsync(GameSession session, GameSlot slot)
    {
        string key = slot == GameSlot.Daily ? DailyGameKey : FreeGameKey;
        return session.SavedFor(slot) is { } saved
            ? SaveAsync(key, saved, AppJsonContext.Default.SavedGame)
            : browser.RemoveItemAsync(key);
    }

    /// <summary>Speichert beide Plätze und den aktiven (nach einem Wechsel).</summary>
    public async Task SaveAllSlotsAsync(GameSession session)
    {
        await SaveSlotAsync(session, GameSlot.Free);
        await SaveSlotAsync(session, GameSlot.Daily);
        await browser.SetItemAsync(ActiveSlotKey, session.ActiveSlot.ToString());
    }

    public Task SaveStatsAsync() => SaveAsync(StatsKey, Stats, AppJsonContext.Default.GameStats);

    public Task SaveDailyAsync() => SaveAsync(DailyKey, Daily, AppJsonContext.Default.DailyProgress);

    private async Task<T?> LoadAsync<T>(string key, JsonTypeInfo<T> typeInfo) where T : class
    {
        string? json = await browser.GetItemAsync(key);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize(json, typeInfo); }
        catch (JsonException) { return null; }   // kaputte Daten -> neu anfangen
    }

    private Task SaveAsync<T>(string key, T value, JsonTypeInfo<T> typeInfo)
        => browser.SetItemAsync(key, JsonSerializer.Serialize(value, typeInfo));
}
