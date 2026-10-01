using System.Numerics;

namespace SudokuWeb.Game;

/// <summary>
/// Kleiner Backtracking-Löser (mit "wenigste Kandidaten zuerst"), der nur zählt,
/// wie viele Lösungen ein Rätsel hat. Wird für die Eindeutigkeits-Prüfung gebraucht.
/// </summary>
public static class SudokuSolver
{
    private const int AllDigits = 0b11_1111_1110;   // Bits 1..9

    /// <summary>Zählt die Lösungen eines Bretts (81 Werte, 0 = leer) bis maximal <paramref name="limit"/>.</summary>
    public static int CountSolutions(int[] board, int limit = 2)
    {
        var rows = new int[9];
        var cols = new int[9];
        var boxes = new int[9];
        var work = (int[])board.Clone();

        for (int i = 0; i < 81; i++)
        {
            if (work[i] == 0) continue;
            int bit = 1 << work[i];
            int r = i / 9, c = i % 9, b = r / 3 * 3 + c / 3;
            if (((rows[r] | cols[c] | boxes[b]) & bit) != 0) return 0;   // Widerspruch in den Vorgaben
            rows[r] |= bit; cols[c] |= bit; boxes[b] |= bit;
        }

        int count = 0;
        Search();
        return count;

        void Search()
        {
            // Leeres Feld mit den wenigsten Kandidaten suchen.
            int best = -1, bestFree = 0, bestCount = 10;
            for (int i = 0; i < 81; i++)
            {
                if (work[i] != 0) continue;
                int r = i / 9, c = i % 9;
                int free = AllDigits & ~(rows[r] | cols[c] | boxes[r / 3 * 3 + c / 3]);
                int n = BitOperations.PopCount((uint)free);
                if (n == 0) return;               // Sackgasse
                if (n < bestCount)
                {
                    best = i; bestFree = free; bestCount = n;
                    if (n == 1) break;
                }
            }

            if (best < 0) { count++; return; }    // alles gefüllt -> eine Lösung

            int row = best / 9, col = best % 9, box = row / 3 * 3 + col / 3;
            for (int d = 1; d <= 9; d++)
            {
                int bit = 1 << d;
                if ((bestFree & bit) == 0) continue;

                work[best] = d;
                rows[row] |= bit; cols[col] |= bit; boxes[box] |= bit;
                Search();
                rows[row] &= ~bit; cols[col] &= ~bit; boxes[box] &= ~bit;
                work[best] = 0;

                if (count >= limit) return;
            }
        }
    }
}
