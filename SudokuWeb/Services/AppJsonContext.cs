using System.Text.Json.Serialization;
using SudokuWeb.Game;

namespace SudokuWeb.Services;

/// <summary>
/// Quellcode-generierter JSON-Kontext. Das ist trimming-sicher (wichtig für den
/// Release-Build von Blazor WebAssembly) und schneller als Reflection.
/// </summary>
[JsonSerializable(typeof(SavedGame))]
[JsonSerializable(typeof(GameStats))]
[JsonSerializable(typeof(DailyProgress))]
[JsonSerializable(typeof(AchievementBook))]
internal partial class AppJsonContext : JsonSerializerContext;
