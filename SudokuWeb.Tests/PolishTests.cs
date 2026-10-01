namespace SudokuWeb.Tests;

public class PolishTests
{
    [Fact]
    public void DailyMessage_IsDeterministicPerDate()
    {
        var day = new DateOnly(2026, 10, 10);
        Assert.Equal(DailyMessages.For(day), DailyMessages.For(day));
    }

    [Fact]
    public void DailyMessage_ChangesFromDayToDay_AndUsesAllVariants()
    {
        var start = new DateOnly(2026, 1, 1);
        var days = Enumerable.Range(0, 365).Select(i => start.AddDays(i)).ToList();

        Assert.All(days.Zip(days.Skip(1)), pair => Assert.NotEqual(DailyMessages.For(pair.First), DailyMessages.For(pair.Second)));
        Assert.Equal(DailyMessages.All.Count, days.Select(DailyMessages.For).Distinct().Count());
    }

    [Fact]
    public void DailyMessages_HaveBetween10And15Variants_AllFilled()
    {
        Assert.InRange(DailyMessages.All.Count, 10, 15);
        Assert.All(DailyMessages.All, m => Assert.False(string.IsNullOrWhiteSpace(m)));
        Assert.Equal(DailyMessages.All.Count, DailyMessages.All.Distinct().Count());
    }

    [Fact]
    public void Check_CountsEveryPress_SoTheShakeRestarts()
    {
        var game = new SudokuGame();
        game.StartDaily(new DateOnly(2026, 10, 1));
        var cell = game.Cells.First(c => c.IsEmpty);
        game.Select(cell);
        game.Enter(cell.Solution == 1 ? 2 : 1);      // falsche Zahl

        game.Check();
        game.Check();

        Assert.Equal(2, game.CheckCount);
        Assert.Equal(1, game.LastCheckErrors);
        Assert.True(cell.IsWrong);
    }
}
