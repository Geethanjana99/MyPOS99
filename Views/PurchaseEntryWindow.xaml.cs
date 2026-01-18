using System.Windows;
using MyPOS99.Data;
using MyPOS99.ViewModels;

namespace MyPOS99.Views
{
    public partial class PurchaseEntryWindow : Window
    {
        private readonly PurchaseViewModel _viewModel;

        public PurchaseEntryWindow()
        {
            InitializeComponent();

            var dbService = new DatabaseService();
            _viewModel = new PurchaseViewModel(dbService);
            DataContext = _viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
