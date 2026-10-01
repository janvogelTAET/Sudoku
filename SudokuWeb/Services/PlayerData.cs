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
    private const string GameKey = "sudoku.game";
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

    public Task<SavedGame?> LoadGameAsync() => LoadAsync(GameKey, AppJsonContext.Default.SavedGame);

    public Task SaveGameAsync(SudokuGame game)
        => SaveAsync(GameKey, game.ToSaved(), AppJsonContext.Default.SavedGame);

    public Task ClearGameAsync() => browser.RemoveItemAsync(GameKey);

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
