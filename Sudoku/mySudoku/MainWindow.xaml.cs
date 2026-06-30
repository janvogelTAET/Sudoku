using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mySudoku {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainViewModel _viewModel;

        public MainWindow() {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel; // Wichtig für das Data Binding!

            // Startet direkt beim Öffnen ein einfaches Spiel
            _viewModel.StartNewGame("Einfach");
        }

        private void Start_Click(object sender, RoutedEventArgs e) {
            string difficulty = (DifficultyBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            _viewModel.StartNewGame(difficulty);
        }

        private void Check_Click(object sender, RoutedEventArgs e) {
            _viewModel.CheckBoard();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {

        }
    }
}