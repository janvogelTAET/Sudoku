namespace SudokuWeb.Game;

public enum Difficulty { Einfach, Mittel, Schwer, Experte }

/// <summary>
/// Erzeugt zufällige Sudoku-Rätsel mit garantiert eindeutiger Lösung.
/// (Basiert auf der ursprünglichen WPF-Version, erweitert um die
///  Eindeutigkeits-Prüfung beim "Löcher graben".)
/// </summary>
public class SudokuGenerator
{
    private readonly Random _random = new();

    // Anzahl der zu entfernenden Felder je Schwierigkeit.
    private static int HolesFor(Difficulty d) => d switch
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

            if (CountSolutions((int[])puzzle.Clone(), 2) != 1)
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

    // Zählt Lösungen bis maximal <limit> (für die Eindeutigkeits-Prüfung).
    private static int CountSolutions(int[] b, int limit)
    {
        int count = 0;
        Solve(0);
        return count;

        void Solve(int pos)
        {
            if (count >= limit) return;
            while (pos < 81 && b[pos] != 0) pos++;
            if (pos == 81) { count++; return; }

            for (int n = 1; n <= 9; n++)
            {
                if (IsSafe(b, pos, n))
                {
                    b[pos] = n;
                    Solve(pos + 1);
                    b[pos] = 0;
                    if (count >= limit) return;
                }
            }
        }
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

    private IEnumerable<int> Shuffled(IEnumerable<int> source)
        => source.OrderBy(_ => _random.Next());
}
