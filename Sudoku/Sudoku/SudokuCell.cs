using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku {
    public class SudokuCell {
        public int Value { get; set; }
        public int CorrectValue { get; set; } // Die Lösung
        public bool IsFixed { get; set; }     // Vom System vorgegeben?
        public bool IsWrong { get; set; }     // Falsche Eingabe des Nutzers?
    }
}
