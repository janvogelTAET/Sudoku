using System.Text.Json;
using SudokuWeb.Services;

namespace SudokuWeb.Tests;

public class GameTests
{
    private static SudokuGame NewDaily()
    {
        var game = new SudokuGame();
        game.StartDaily(new DateOnly(2026, 10, 1));
        return game;
    }

    [Fact]
    public void SaveAndRestore_KeepsEverything_ThroughJson()
    {
        var game = NewDaily();
        var empty = game.Cells.First(c => c.IsEmpty);
        game.Select(empty);
        game.ToggleNotesMode();
        game.Enter(3);
        game.Enter(7);
        game.ToggleNotesMode();
        var other = game.Cells.First(c => c.IsEmpty && c != empty);
        game.Select(other);
        game.Enter(other.Solution == 1 ? 2 : 1);
        game.Seconds = 123;

        string json = JsonSerializer.Serialize(game.ToSaved(), AppJsonContext.Default.SavedGame);
        var restored = new SudokuGame();
        Assert.True(restored.Restore(JsonSerializer.Deserialize(json, AppJsonContext.Default.SavedGame)));

        Assert.Equal(game.Cells.Select(c => c.Value), restored.Cells.Select(c => c.Value));
        Assert.Equal(game.Cells.Select(c => c.IsFixed), restored.Cells.Select(c => c.IsFixed));
        Assert.Equal(game.Cells.Select(c => c.Solution), restored.Cells.Select(c => c.Solution));
        Assert.Equal(new[] { 3, 7 }, restored.Cells[empty.Row * 9 + empty.Col].Notes.Order());
        Assert.Equal(123, restored.Seconds);
        Assert.Equal(new DateOnly(2026, 10, 1), restored.DailyDate);
        Assert.Equal(Difficulty.Mittel, restored.Difficulty);
    }

    [Fact]
    public void Restore_RejectsGarbage()
    {
        var game = new SudokuGame();
        Assert.False(game.Restore(null));
        Assert.False(game.Restore(new SavedGame { Values = [1, 2, 3] }));
    }

    [Fact]
    public void Undo_RevertsValuesNotesAndHints()
    {
        var game = NewDaily();
        var cell = game.Cells.First(c => c.IsEmpty);
        game.Select(cell);
        game.Enter(cell.Solution);
        game.Undo();
        Assert.True(cell.IsEmpty);

        game.Hint();
        Assert.True(cell.IsHint);
        game.Undo();
        Assert.False(cell.IsHint);
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void FillingCorrectly_SolvesGame()
    {
        var game = NewDaily();
        foreach (var c in game.Cells.Where(c => c.IsEmpty).ToList())
        {
            game.Select(c);
            game.Enter(c.Solution);
        }
        Assert.True(game.Solved);
    }

    [Fact]
    public void Stats_TrackBestTime_OnlyWithoutHints()
    {
        var stats = new GameStats();
        Assert.True(stats.RecordWin(Difficulty.Mittel, 300, 0));
        Assert.False(stats.RecordWin(Difficulty.Mittel, 100, 2));   // mit Tipp: keine Bestzeit
        Assert.True(stats.RecordWin(Difficulty.Mittel, 250, 0));
        Assert.Equal(250, stats.For(Difficulty.Mittel).BestSeconds);
        Assert.Equal(3, stats.TotalWon);
    }
}
