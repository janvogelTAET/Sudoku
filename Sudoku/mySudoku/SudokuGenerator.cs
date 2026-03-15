using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku {
    public class SudokuGenerator {
        private Random _random = new Random();

        public SudokuCell[,] Generate(string difficulty) {
            int[,] board = new int[9, 9];
            FillBoard(board, 0, 0); // Erstellt das gelöste Board

            SudokuCell[,] gameBoard = new SudokuCell[9, 9];
            for (int r = 0; r < 9; r++) {
                for (int c = 0; c < 9; c++) {
                    gameBoard[r, c] = new SudokuCell {
                        CorrectValue = board[r, c],
                        Value = board[r, c],
                        IsFixed = true
                    };
                }
            }

            // Löcher graben basierend auf Schwierigkeit
            int holes = difficulty == "Schwer" ? 55 : (difficulty == "Mittel" ? 45 : 30);
            while (holes > 0) {
                int r = _random.Next(9);
                int c = _random.Next(9);
                if (gameBoard[r, c].IsFixed) {
                    gameBoard[r, c].Value = 0; // 0 bedeutet "leeres Feld"
                    gameBoard[r, c].IsFixed = false;
                    holes--;
                }
            }
            return gameBoard;
        }

        private bool FillBoard(int[,] board, int row, int col) {
            if (col == 9) { col = 0; row++; }
            if (row == 9) return true;
            if (board[row, col] != 0) return FillBoard(board, row, col + 1);

            var nums = Enumerable.Range(1, 9).OrderBy(x => _random.Next()).ToList();
            foreach (var num in nums) {
                if (IsSafe(board, row, col, num)) {
                    board[row, col] = num;
                    if (FillBoard(board, row, col + 1)) return true;
                    board[row, col] = 0; // Backtracking
                }
            }
            return false;
        }

        private bool IsSafe(int[,] board, int row, int col, int num) {
            // Zeile und Spalte prüfen
            for (int i = 0; i < 9; i++)
                if (board[row, i] == num || board[i, col] == num) return false;

            // 3x3 Block prüfen
            int startRow = row - (row % 3);
            int startCol = col - (col % 3);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[startRow + i, startCol + j] == num) return false;

            return true;
        }
    }
}
