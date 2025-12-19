using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.Views
{
    public partial class ReturnExchangeDialog : Window
    {
        private readonly ProductService _productService;
        public ObservableCollection<ReturnExchangeItem> ReturnItems { get; set; }
        public decimal TotalCredit { get; private set; }
        public bool IsCashReturn { get; private set; }
        public decimal ReturnDiscount { get; private set; }

        public ReturnExchangeDialog()
        {
            InitializeComponent();

            var dbContext = new DatabaseContext();
            _productService = new ProductService(dbContext);
            ReturnItems = new ObservableCollection<ReturnExchangeItem>();
            ReturnItemsListBox.ItemsSource = ReturnItems;

            // Handle radio button changes
            if (CashReturnRadioButton != null)
            {
                CashReturnRadioButton.Checked += (s, e) => ReturnDiscountPanel.Visibility = Visibility.Visible;
                ExchangeRadioButton.Checked += (s, e) => ReturnDiscountPanel.Visibility = Visibility.Collapsed;
            }

            ProductCodeTextBox.Focus();
        }

        private async void ProductCodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var code = ProductCodeTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code))
                    return;

                try
                {
                    // Search by code
                    var product = await _productService.GetProductByCodeAsync(code);

                    if (product == null)
                    {
                        MessageBox.Show($"Product not found: {code}", "Not Found",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        ProductCodeTextBox.SelectAll();
                        return;
                    }

                    // Check if already in list
                    var existing = ReturnItems.FirstOrDefault(x => x.ProductId == product.Id);
                    if (existing != null)
                    {
                        existing.Quantity++;
                    }
                    else
                    {
                        ReturnItems.Add(new ReturnExchangeItem
                        {
                            ProductId = product.Id,
                            ProductCode = product.Code,
                            ProductName = product.Name,
                            Price = product.SellPrice,
                            Quantity = 1
                        });
                    }

                    CalculateTotal();
                    ProductCodeTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ReturnExchangeItem item)
            {
                ReturnItems.Remove(item);
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            TotalCredit = ReturnItems.Sum(x => x.Price * x.Quantity);
            TotalAmountText.Text = $"Rs. {TotalCredit:N2}";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReturnItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item to return.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get return type and discount
            IsCashReturn = CashReturnRadioButton?.IsChecked == true;

            if (IsCashReturn)
            {
                // Parse return discount
                if (!string.IsNullOrWhiteSpace(ReturnDiscountTextBox?.Text))
                {
                    if (!decimal.TryParse(ReturnDiscountTextBox.Text, out decimal discount))
                    {
                        MessageBox.Show("Invalid discount amount.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    ReturnDiscount = discount;
                }

                // For cash returns, update stock immediately
                try
                {
                    using var connection = new DatabaseService().OpenConnection();
                    foreach (var item in ReturnItems)
                    {
                        var updateCommand = connection.CreateCommand();
                        updateCommand.CommandText = @"
                            UPDATE Products 
                            SET StockQty = StockQty + @quantity, 
                                UpdatedAt = @updatedAt
                            WHERE Id = @productId
                        ";
                        updateCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                        updateCommand.Parameters.AddWithValue("@productId", item.ProductId);
                        updateCommand.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        updateCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating stock: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            // For exchange, stock will be updated when invoice is completed

            DialogResult = true;
            Close();
        }
    }

    // Simple model for return items
    public class ReturnExchangeItem : ViewModels.ViewModelBase
    {
        private int _quantity;

        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
    }
}
