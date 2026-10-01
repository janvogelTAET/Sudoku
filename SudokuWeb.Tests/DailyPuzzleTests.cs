namespace SudokuWeb.Tests;

public class DailyPuzzleTests
{
    private static int[] Values(SudokuCell[] cells) => cells.Select(c => c.Value).ToArray();

    [Fact]
    public void SameDate_GivesIdenticalPuzzle()
    {
        var date = new DateOnly(2026, 10, 1);
        var a = DailyPuzzle.Create(date);
        var b = DailyPuzzle.Create(date);

        Assert.Equal(Values(a), Values(b));
        Assert.Equal(a.Select(c => c.Solution), b.Select(c => c.Solution));
    }

    [Fact]
    public void DifferentDates_GiveDifferentPuzzles()
    {
        var a = DailyPuzzle.Create(new DateOnly(2026, 10, 1));
        var b = DailyPuzzle.Create(new DateOnly(2026, 10, 2));

        Assert.NotEqual(Values(a), Values(b));
    }

    [Fact]
    public void Puzzle_HasUniqueSolution_MatchingStoredSolution()
    {
        foreach (var date in new[] { new DateOnly(2026, 1, 1), new DateOnly(2026, 10, 1), new DateOnly(2027, 2, 28) })
        {
            var cells = DailyPuzzle.Create(date);
            Assert.Equal(1, SudokuSolver.CountSolutions(Values(cells), 2));
            AssertValidSolution(cells.Select(c => c.Solution).ToArray());
            Assert.All(cells.Where(c => c.IsFixed), c => Assert.Equal(c.Solution, c.Value));
            Assert.Equal(81 - SudokuGenerator.HolesFor(DailyPuzzle.Level), cells.Count(c => c.IsFixed));
        }
    }

    [Theory]
    [InlineData(Difficulty.Einfach)]
    [InlineData(Difficulty.Mittel)]
    [InlineData(Difficulty.Schwer)]
    [InlineData(Difficulty.Experte)]
    public void RandomPuzzle_IsUniqueAndHasExpectedClues(Difficulty difficulty)
    {
        var cells = new SudokuGenerator(new Random(7)).Generate(difficulty);

        Assert.Equal(1, SudokuSolver.CountSolutions(Values(cells), 2));
        AssertValidSolution(cells.Select(c => c.Solution).ToArray());
        Assert.True(cells.Count(c => c.IsFixed) >= 81 - SudokuGenerator.HolesFor(difficulty));
    }

    [Fact]
    public void Solver_DetectsMultipleAndNoSolutions()
    {
        Assert.Equal(2, SudokuSolver.CountSolutions(new int[81], 2));   // leeres Brett

        var board = new int[81];
        board[0] = 5; board[1] = 5;                                      // Widerspruch in einer Zeile
        Assert.Equal(0, SudokuSolver.CountSolutions(board, 2));
    }

    private static void AssertValidSolution(int[] s)
    {
        var digits = Enumerable.Range(1, 9).ToArray();
        for (int i = 0; i < 9; i++)
        {
            Assert.Equal(digits, Enumerable.Range(0, 9).Select(j => s[i * 9 + j]).Order());                      // Zeile
            Assert.Equal(digits, Enumerable.Range(0, 9).Select(j => s[j * 9 + i]).Order());                      // Spalte
            int br = i / 3 * 3, bc = i % 3 * 3;
            Assert.Equal(digits, Enumerable.Range(0, 9).Select(j => s[(br + j / 3) * 9 + bc + j % 3]).Order());  // Block
        }
    }
}
