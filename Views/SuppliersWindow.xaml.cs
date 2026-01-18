using System.Windows;
using MyPOS99.Data;
using MyPOS99.ViewModels;

namespace MyPOS99.Views
{
    public partial class SuppliersWindow : Window
    {
        private readonly SupplierViewModel _viewModel;

        public SuppliersWindow()
        {
            InitializeComponent();

            var dbService = new DatabaseService();
            _viewModel = new SupplierViewModel(dbService);
            DataContext = _viewModel;
        }

                private void CloseButton_Click(object sender, RoutedEventArgs e)
                {
                    Close();
                }

                private void PurchaseEntryButton_Click(object sender, RoutedEventArgs e)
                {
                    var purchaseWindow = new PurchaseEntryWindow();
                    purchaseWindow.ShowDialog();
                }
            }
        }
