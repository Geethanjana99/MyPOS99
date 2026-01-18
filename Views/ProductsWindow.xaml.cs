using System;
using System.Windows;
using MyPOS99.ViewModels;
using MyPOS99.Data;

namespace MyPOS99.Views
{
    public partial class ProductsWindow : Window
    {
        public ProductsWindow()
        {
            try
            {
                InitializeComponent();

                // Initialize ViewModel
                var dbService = new DatabaseService();
                var viewModel = new ProductViewModel(dbService);
                DataContext = viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Products Window:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
