using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku {
    public class SudokuGenerator {
        private Random _rng = new Random();

        public SudokuCell[,] Generate(string difficulty) {
            int[,] fullBoard = CreateSolvedBoard();
            SudokuCell[,] gameBoard = new SudokuCell[9, 9];

            int holes = difficulty == "Schwer" ? 55 : (difficulty == "Mittel" ? 40 : 30);

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    gameBoard[r, c] = new SudokuCell { CorrectValue = fullBoard[r, c], Value = fullBoard[r, c], IsFixed = true };

            // Löcher graben
            while (holes > 0) {
                int r = _rng.Next(9), c = _rng.Next(9);
                if (gameBoard[r, c].IsFixed) {
                    gameBoard[r, c].Value = 0;
                    gameBoard[r, c].IsFixed = false;
                    holes--;
                }
            }
            return gameBoard;
        }

        private int[,] CreateSolvedBoard() { /* ... Backtracking Algorithmus wie oben ... */ return new int[9, 9]; }
    }
}
