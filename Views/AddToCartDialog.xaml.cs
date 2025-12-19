using System.Windows;
using System.Windows.Input;
using MyPOS99.Models;

namespace MyPOS99.Views
{
    public partial class AddToCartDialog : Window
    {
        private readonly Product _product;
        private int _maxStock;

        public int Quantity { get; private set; }
        public decimal Discount { get; private set; }

        public AddToCartDialog(Product product)
        {
            InitializeComponent();

            _product = product;
            _maxStock = product.StockQty;

            // Set product details
            ProductNameText.Text = product.Name;
            ProductCodeText.Text = $"Code: {product.Code}";
            ProductPriceText.Text = $"Rs. {product.SellPrice:N2}";
            ProductStockText.Text = $"{product.StockQty} units";

            // Focus on quantity
            QuantityTextBox.Focus();
            QuantityTextBox.SelectAll();
        }

        private void QuantityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Move to discount field
                DiscountTextBox.Focus();
                DiscountTextBox.SelectAll();
                e.Handled = true;
            }
        }

        private void DiscountTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Trigger Add to Cart
                AddButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int currentQty))
            {
                if (currentQty < _maxStock)
                {
                    QuantityTextBox.Text = (currentQty + 1).ToString();
                }
                else
                {
                    MessageBox.Show($"Only {_maxStock} units available in stock!", 
                        "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int currentQty))
            {
                if (currentQty > 1)
                {
                    QuantityTextBox.Text = (currentQty - 1).ToString();
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate quantity
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 1)
            {
                MessageBox.Show("Please enter a valid quantity (minimum 1).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            if (quantity > _maxStock)
            {
                MessageBox.Show($"Only {_maxStock} units available in stock!", 
                    "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            // Validate discount
            if (!decimal.TryParse(DiscountTextBox.Text, out decimal discount) || discount < 0)
            {
                MessageBox.Show("Please enter a valid discount (0 or greater).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountTextBox.Focus();
                return;
            }

            // Set values
            Quantity = quantity;
            Discount = discount;
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
