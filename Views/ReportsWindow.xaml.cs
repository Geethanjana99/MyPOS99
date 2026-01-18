using System.Windows;
using MyPOS99.Data;
using MyPOS99.ViewModels;

namespace MyPOS99.Views
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
            
            var dbService = new DatabaseService();
            var viewModel = new ReportViewModel(dbService);
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
