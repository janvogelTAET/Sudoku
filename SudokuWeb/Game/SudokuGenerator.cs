namespace SudokuWeb.Game;

public enum Difficulty { Einfach, Mittel, Schwer, Experte }

/// <summary>
/// Erzeugt Sudoku-Rätsel mit garantiert eindeutiger Lösung.
/// Mit einem festen Seed im <see cref="Random"/> entsteht immer dasselbe Rätsel
/// (so funktioniert das tägliche Rätsel).
/// </summary>
public class SudokuGenerator
{
    private readonly Random _random;

    public SudokuGenerator(Random? random = null) => _random = random ?? Random.Shared;

    // Anzahl der zu entfernenden Felder je Schwierigkeit.
    public static int HolesFor(Difficulty d) => d switch
    {
        Difficulty.Einfach => 38,
        Difficulty.Mittel  => 46,
        Difficulty.Schwer  => 52,
        Difficulty.Experte => 56,
        _ => 40
    };

    public SudokuCell[] Generate(Difficulty difficulty)
    {
        // 1. Vollständig gelöstes Feld erzeugen.
        var solution = new int[81];
        Fill(solution, 0);

        // 2. Felder entfernen, solange die Lösung eindeutig bleibt.
        var puzzle = (int[])solution.Clone();
        int holes = HolesFor(difficulty);
        int dug = 0;

        foreach (int idx in Shuffled(Enumerable.Range(0, 81)))
        {
            if (dug >= holes) break;
            int backup = puzzle[idx];
            puzzle[idx] = 0;

            if (SudokuSolver.CountSolutions(puzzle, 2) != 1)
                puzzle[idx] = backup;   // nicht mehr eindeutig -> rückgängig
            else
                dug++;
        }

        // 3. In Zellen-Objekte umwandeln.
        var cells = new SudokuCell[81];
        for (int i = 0; i < 81; i++)
        {
            cells[i] = new SudokuCell
            {
                Row = i / 9,
                Col = i % 9,
                Solution = solution[i],
                Value = puzzle[i],
                IsFixed = puzzle[i] != 0
            };
        }
        return cells;
    }

    // --- Backtracking zum Füllen ---
    private bool Fill(int[] b, int pos)
    {
        if (pos == 81) return true;
        if (b[pos] != 0) return Fill(b, pos + 1);

        foreach (int n in Shuffled(Enumerable.Range(1, 9)))
        {
            if (IsSafe(b, pos, n))
            {
                b[pos] = n;
                if (Fill(b, pos + 1)) return true;
                b[pos] = 0; // Backtracking
            }
        }
        return false;
    }

    private static bool IsSafe(int[] b, int pos, int num)
    {
        int row = pos / 9, col = pos % 9;

        for (int i = 0; i < 9; i++)
            if (b[row * 9 + i] == num || b[i * 9 + col] == num) return false;

        int sr = row - row % 3, sc = col - col % 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (b[(sr + i) * 9 + sc + j] == num) return false;

        return true;
    }

    // Fisher-Yates: benutzt pro Element genau einen Zufallswert (reproduzierbar bei festem Seed).
    private List<int> Shuffled(IEnumerable<int> source)
    {
        var list = source.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
