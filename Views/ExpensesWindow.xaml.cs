using System.Windows;
using MyPOS99.Data;
using MyPOS99.ViewModels;

namespace MyPOS99.Views
{
    public partial class ExpensesWindow : Window
    {
        private readonly ExpenseViewModel _viewModel;

        public ExpensesWindow()
        {
            InitializeComponent();

            var dbService = new DatabaseService();
            _viewModel = new ExpenseViewModel(dbService);
            DataContext = _viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
