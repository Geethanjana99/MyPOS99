using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyPOS99.Models;

namespace MyPOS99.Views
{
    public partial class ProcessReturnDialog : Window
    {
        public Sale SelectedSale { get; set; }
        public List<SaleItem> SaleItems { get; set; }
        public List<SaleItem> SelectedItemsToReturn { get; private set; }
        public string ReturnReason { get; private set; }
        public decimal ReturnAmount { get; private set; }

        public ProcessReturnDialog(Sale sale, List<SaleItem> items)
        {
            InitializeComponent();
            
            SelectedSale = sale;
            SaleItems = items;
            SelectedItemsToReturn = new List<SaleItem>();

            LoadSaleInfo();
            LoadItems();
        }

        private void LoadSaleInfo()
        {
            InvoiceNumberText.Text = SelectedSale.InvoiceNumber;
            SaleDateText.Text = SelectedSale.Date.ToString("dd/MM/yyyy HH:mm");
            SaleTotalText.Text = $"Rs. {SelectedSale.Total:N2}";
        }

        private void LoadItems()
        {
            ItemsListBox.ItemsSource = SaleItems;
            ItemsListBox.SelectionChanged += ItemsListBox_SelectionChanged;
        }

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Calculate return amount based on selected items
            decimal total = 0;
            foreach (SaleItem item in ItemsListBox.SelectedItems)
            {
                total += item.Total;
            }
            
            ReturnAmount = total;
            ReturnAmountText.Text = $"Rs. {ReturnAmount:N2}";
        }

        private void ProcessReturnButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate selection
            if (ItemsListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item to return.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate reason
            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
            {
                MessageBox.Show("Please enter a reason for the return.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm return
            var result = MessageBox.Show(
                $"Process return for Rs. {ReturnAmount:N2}?\n\nThis will:\n- Add items back to stock\n- Reduce today's sales total\n- Cannot be undone",
                "Confirm Return",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SelectedItemsToReturn = ItemsListBox.SelectedItems.Cast<SaleItem>().ToList();
                ReturnReason = ReasonTextBox.Text.Trim();
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
