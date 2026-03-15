using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku {
    public class SudokuCell : INotifyPropertyChanged {
        private int _value;
        private bool _isWrong;

        public int CorrectValue { get; set; }
        public bool IsFixed { get; set; }

        public int Value {
            get => _value;
            set { if (_value != value) { _value = value; OnPropertyChanged(); } }
        }

        public bool IsWrong {
            get => _isWrong;
            set { if (_isWrong != value) { _isWrong = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
