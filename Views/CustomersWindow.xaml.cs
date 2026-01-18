using System.Windows;
using MyPOS99.Data;
using MyPOS99.ViewModels;

namespace MyPOS99.Views
{
    public partial class CustomersWindow : Window
    {
        private readonly CustomerViewModel _viewModel;

        public CustomersWindow()
        {
            InitializeComponent();

            var dbService = new DatabaseService();
            _viewModel = new CustomerViewModel(dbService);
            DataContext = _viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClosePurchaseHistoryPopup_Click(object sender, RoutedEventArgs e)
        {
            PurchaseHistoryPopup.IsOpen = false;
        }
    }
}
