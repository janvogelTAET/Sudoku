using Sudoku;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace mySudoku
{


    public class MainViewModel : INotifyPropertyChanged {
        public ObservableCollection<SudokuCell> Cells { get; set; } = new ObservableCollection<SudokuCell>();
        private SudokuGenerator _generator = new SudokuGenerator();

        public void StartNewGame(string difficulty) {
            var board = _generator.Generate(difficulty);
            Cells.Clear();

            // 2D-Array in eine flache Liste für das WPF-Grid umwandeln
            for (int r = 0; r < 9; r++) {
                for (int c = 0; c < 9; c++) {
                    Cells.Add(board[r, c]);
                }
            }
        }

        public void CheckBoard() {
            foreach (var cell in Cells) {
                if (!cell.IsFixed && cell.Value != 0) {
                    // Prüfen, ob die eingetragene Zahl mit der Lösung übereinstimmt
                    cell.IsWrong = cell.Value != cell.CorrectValue;
                }
                else if (!cell.IsFixed && cell.Value == 0) {
                    cell.IsWrong = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
