using System.Windows;
using MyPOS99.ViewModels;
using MyPOS99.Data;

namespace MyPOS99.Views
{
    public partial class ProductsWindow : Window
    {
        public ProductsWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            var dbService = new DatabaseService();
            var viewModel = new ProductViewModel(dbService);
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
