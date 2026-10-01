namespace SudokuWeb.Game;

/// <summary>
/// Das tägliche Rätsel: Der Zufallsgenerator wird mit dem Datum gefüttert,
/// deshalb ist das Rätsel für einen Tag immer dasselbe (auf jedem Gerät).
/// </summary>
public static class DailyPuzzle
{
    public const Difficulty Level = Difficulty.Mittel;

    /// <summary>Seed als Zahl im Format JJJJMMTT (z. B. 20261001).</summary>
    public static int SeedFor(DateOnly date) => date.Year * 10000 + date.Month * 100 + date.Day;

    public static SudokuCell[] Create(DateOnly date)
        => new SudokuGenerator(new Random(SeedFor(date))).Generate(Level);
}
