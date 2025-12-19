using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using MyPOS99.ViewModels;
using MyPOS99.Data;
using MyPOS99.Models;

namespace MyPOS99.Views
{
    public partial class SalesWindow : Window
    {
        private readonly SalesViewModel _viewModel;

        public SalesWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            var dbService = new DatabaseService();
            _viewModel = new SalesViewModel(dbService);
            DataContext = _viewModel;

            // Subscribe to SearchResults changes to auto-open popup
            _viewModel.SearchResults.CollectionChanged += SearchResults_CollectionChanged;

            // Focus search box on load
            Loaded += (s, e) => SearchTextBox.Focus();
        }

        private void SearchResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Open popup when results are available
            if (_viewModel.SearchResults.Count > 0 && !string.IsNullOrWhiteSpace(_viewModel.SearchText))
            {
                SearchResultsPopup.IsOpen = true;

                // Select first item
                if (SearchResultsListBox.Items.Count > 0)
                {
                    SearchResultsListBox.SelectedIndex = 0;
                }
            }
                else
                {
                    SearchResultsPopup.IsOpen = false;
                }
            }

            private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                // Intercept arrow keys when popup is open to prevent TextBox cursor movement
                if (SearchResultsPopup.IsOpen && (e.Key == Key.Down || e.Key == Key.Up))
                {
                    e.Handled = true;
                    // Forward to KeyDown handler
                    SearchTextBox_KeyDown(sender, e);
                }
            }

            private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (SearchResultsPopup.IsOpen)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        // Move focus to results list
                        if (SearchResultsListBox.Items.Count > 0)
                        {
                            SearchResultsListBox.Focus();
                            if (SearchResultsListBox.SelectedIndex < 0)
                            {
                                SearchResultsListBox.SelectedIndex = 0;
                            }
                            // Get the ListBoxItem and focus it
                            var item = SearchResultsListBox.ItemContainerGenerator.ContainerFromIndex(SearchResultsListBox.SelectedIndex) as ListBoxItem;
                            item?.Focus();
                        }
                        e.Handled = true;
                        break;

                    case Key.Up:
                        // Navigate up in results if already in list
                        if (SearchResultsListBox.Items.Count > 0 && SearchResultsListBox.SelectedIndex > 0)
                        {
                            SearchResultsListBox.SelectedIndex--;
                            e.Handled = true;
                        }
                        break;

                    case Key.Enter:
                        // Add selected item
                        if (_viewModel.SelectedProduct != null)
                        {
                            SearchResultsPopup.IsOpen = false;
                            _viewModel.AddToCartCommand.Execute(null);
                            SearchTextBox.Focus();
                            SearchTextBox.SelectAll();
                        }
                        e.Handled = true;
                        break;

                    case Key.Escape:
                        // Close popup
                        SearchResultsPopup.IsOpen = false;
                        SearchTextBox.Focus();
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.Down && _viewModel.SearchResults.Count > 0)
            {
                // Open popup if closed and navigate to first item
                SearchResultsPopup.IsOpen = true;
                if (SearchResultsListBox.Items.Count > 0)
                {
                    SearchResultsListBox.SelectedIndex = 0;
                    SearchResultsListBox.Focus();
                    var item = SearchResultsListBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                    item?.Focus();
                }
                e.Handled = true;
            }
        }

        private void SearchResultsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // Add selected item and close popup
                    if (_viewModel.SelectedProduct != null)
                    {
                        SearchResultsPopup.IsOpen = false;
                        _viewModel.AddToCartCommand.Execute(null);
                        SearchTextBox.Focus();
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    // Close popup and return to search box
                    SearchResultsPopup.IsOpen = false;
                    SearchTextBox.Focus();
                    e.Handled = true;
                    break;

                case Key.Up:
                    // If at first item, return to search box
                    if (SearchResultsListBox.SelectedIndex == 0)
                    {
                        SearchTextBox.Focus();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void SearchResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-click to add
            if (_viewModel.SelectedProduct != null)
            {
                SearchResultsPopup.IsOpen = false;
                _viewModel.AddToCartCommand.Execute(null);
            }
        }

        private async void AmountPaidTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                // Single Enter press - Process sale
                if (!_viewModel.ProcessSaleCommand.CanExecute(null))
                {
                    MessageBox.Show("Cannot process sale. Please check the sale details.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Process the sale (this will show success message)
                    _viewModel.ProcessSaleCommand.Execute(null);

                    // Wait for sale to complete and data to reload
                    await Task.Delay(1000);

                    // Ask if user wants to print
                    var result = MessageBox.Show(
                        "Do you want to print the receipt?", 
                        "Print Receipt", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Print receipt (will generate and open PDF)
                        if (_viewModel.PrintReceiptCommand.CanExecute(null))
                        {
                            _viewModel.PrintReceiptCommand.Execute(null);
                        }
                        else
                        {
                            MessageBox.Show("No recent sale to print.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing sale: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RecentSalesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Sale sale)
            {
                _viewModel.ViewInvoiceCommand.Execute(sale);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
