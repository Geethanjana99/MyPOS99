using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.Views
{
    public partial class RecentSalesWindow : Window
    {
        private readonly PdfReceiptService _pdfService;

        public RecentSalesWindow(ObservableCollection<Sale> sales)
        {
            InitializeComponent();

            _pdfService = new PdfReceiptService();
            SalesListBox.ItemsSource = sales;
        }

        private void SalesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SalesListBox.SelectedItem is Sale sale)
            {
                ViewInvoice(sale);
            }
        }

        private void ViewInvoice(Sale sale)
        {
            try
            {
                var items = GetSaleItems(sale.Id);

                if (items.Count > 0)
                {
                    var cashierName = "Unknown";
                    var customerName = "Walk-in Customer";

                    var pdfPath = _pdfService.GenerateReceipt(sale, items, customerName, cashierName);
                    _pdfService.OpenPdf(pdfPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening invoice: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Collections.Generic.List<SaleItem> GetSaleItems(int saleId)
        {
            var db = new DatabaseService();
            const string query = @"
                SELECT Id, SaleId, ProductId, ProductCode, ProductName, Qty, Price, Discount, Total
                FROM SaleItems
                WHERE SaleId = @saleId
            ";

            var items = db.ExecuteQueryAsync(query, reader => new SaleItem
            {
                Id = reader.GetInt32(0),
                SaleId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ProductCode = reader.GetString(3),
                ProductName = reader.GetString(4),
                Qty = reader.GetInt32(5),
                Price = (decimal)reader.GetDouble(6),
                Discount = (decimal)reader.GetDouble(7),
                Total = (decimal)reader.GetDouble(8)
            }, DatabaseService.CreateParameter("@saleId", saleId)).Result;

            return items.ToList();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
