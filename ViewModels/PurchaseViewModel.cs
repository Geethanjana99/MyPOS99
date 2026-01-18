using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class PurchaseViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;

        private ObservableCollection<PurchaseCartItem> _cartItems;
        private ObservableCollection<Supplier> _suppliers;
        private ObservableCollection<Product> _searchResults;
        private ObservableCollection<Purchase> _purchaseHistory;

        private Supplier? _selectedSupplier;
        private Product? _selectedProduct;
        private PurchaseCartItem? _selectedCartItem;

        private string _searchText = string.Empty;
        private string _purchaseNumber = string.Empty;
        private DateTime _purchaseDate = DateTime.Now;
        private decimal _subTotal;
        private decimal _tax;
        private decimal _total;
        private decimal _amountPaid;
        private string _paymentStatus = "Pending";
        private string _notes = string.Empty;

        public PurchaseViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            var dbContext = new DatabaseContext();
            _productService = new ProductService(dbContext);
            _supplierService = new SupplierService(_db);

            _cartItems = new ObservableCollection<PurchaseCartItem>();
            _suppliers = new ObservableCollection<Supplier>();
            _searchResults = new ObservableCollection<Product>();
            _purchaseHistory = new ObservableCollection<Purchase>();

            _cartItems.CollectionChanged += (s, e) => CalculateTotals();

            // Initialize commands
            SearchProductCommand = new RelayCommand(async () => await SearchProductsAsync());
            AddToCartCommand = new RelayCommand(AddToCart, CanAddToCart);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart, CanRemoveFromCart);
            SavePurchaseCommand = new RelayCommand(async () => await SavePurchaseAsync(), CanSavePurchase);
            ClearCartCommand = new RelayCommand(ClearCart);
            NewPurchaseCommand = new RelayCommand(NewPurchase);

            GeneratePurchaseNumber();
            _ = LoadSuppliersAsync();
            _ = LoadPurchaseHistoryAsync();
        }

        #region Properties

        public ObservableCollection<PurchaseCartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        public ObservableCollection<Product> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public ObservableCollection<Purchase> PurchaseHistory
        {
            get => _purchaseHistory;
            set => SetProperty(ref _purchaseHistory, value);
        }

        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                {
                    ((RelayCommand)SavePurchaseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    ((RelayCommand)AddToCartCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public PurchaseCartItem? SelectedCartItem
        {
            get => _selectedCartItem;
            set
            {
                if (SetProperty(ref _selectedCartItem, value))
                {
                    ((RelayCommand)RemoveFromCartCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = SearchProductsAsync();
                }
            }
        }

        public string PurchaseNumber
        {
            get => _purchaseNumber;
            set => SetProperty(ref _purchaseNumber, value);
        }

        public DateTime PurchaseDate
        {
            get => _purchaseDate;
            set => SetProperty(ref _purchaseDate, value);
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        public decimal Tax
        {
            get => _tax;
            set
            {
                if (SetProperty(ref _tax, value))
                {
                    CalculateTotals();
                }
            }
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public decimal AmountPaid
        {
            get => _amountPaid;
            set
            {
                if (SetProperty(ref _amountPaid, value))
                {
                    UpdatePaymentStatus();
                }
            }
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string SubTotalFormatted => $"Rs. {SubTotal:N2}";
        public string TaxFormatted => $"Rs. {Tax:N2}";
        public string TotalFormatted => $"Rs. {Total:N2}";
        public string AmountDueFormatted => $"Rs. {(Total - AmountPaid):N2}";

        public List<string> PaymentStatuses => new List<string> { "Pending", "Partial", "Paid" };

        #endregion

        #region Commands

        public ICommand SearchProductCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand SavePurchaseCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand NewPurchaseCommand { get; }

        #endregion

        #region Methods

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                Suppliers.Clear();
                foreach (var supplier in suppliers)
                {
                    Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SearchProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                return;
            }

            try
            {
                var products = await _productService.SearchProductsAsync(SearchText);
                SearchResults.Clear();
                foreach (var product in products)
                {
                    SearchResults.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToCart()
        {
            if (SelectedProduct == null) return;

            // Check if already in cart
            var existing = CartItems.FirstOrDefault(x => x.ProductId == SelectedProduct.Id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                CartItems.Add(new PurchaseCartItem
                {
                    ProductId = SelectedProduct.Id,
                    ProductCode = SelectedProduct.Code,
                    ProductName = SelectedProduct.Name,
                    CostPrice = SelectedProduct.CostPrice,
                    Quantity = 1
                });
            }

            SearchText = string.Empty;
            SearchResults.Clear();
            SelectedProduct = null;
        }

        private void RemoveFromCart()
        {
            if (SelectedCartItem != null)
            {
                CartItems.Remove(SelectedCartItem);
                SelectedCartItem = null;
            }
        }

        private void CalculateTotals()
        {
            SubTotal = CartItems.Sum(x => x.Total);
            Total = SubTotal + Tax;

            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(SubTotalFormatted));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(TotalFormatted));
            OnPropertyChanged(nameof(AmountDueFormatted));

            ((RelayCommand)SavePurchaseCommand).RaiseCanExecuteChanged();
        }

        private void UpdatePaymentStatus()
        {
            if (AmountPaid >= Total)
            {
                PaymentStatus = "Paid";
            }
            else if (AmountPaid > 0)
            {
                PaymentStatus = "Partial";
            }
            else
            {
                PaymentStatus = "Pending";
            }

            OnPropertyChanged(nameof(AmountDueFormatted));
        }

        private async Task SavePurchaseAsync()
        {
            try
            {
                if (SelectedSupplier == null)
                {
                    MessageBox.Show("Please select a supplier.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CartItems.Count == 0)
                {
                    MessageBox.Show("Please add at least one product.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var currentUser = ((App)Application.Current).CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("User not authenticated!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var success = await _db.ExecuteTransactionAsync(async (connection, transaction) =>
                {
                    // Insert purchase
                    var purchaseCommand = connection.CreateCommand();
                    purchaseCommand.Transaction = transaction;
                    purchaseCommand.CommandText = @"
                        INSERT INTO Purchases (PurchaseNumber, SupplierId, Date, SubTotal, Tax, Total, PaymentStatus, AmountPaid, Notes, UserId, CreatedAt)
                        VALUES (@purchaseNumber, @supplierId, @date, @subTotal, @tax, @total, @paymentStatus, @amountPaid, @notes, @userId, @createdAt);
                        SELECT last_insert_rowid();
                    ";

                    purchaseCommand.Parameters.AddWithValue("@purchaseNumber", PurchaseNumber);
                    purchaseCommand.Parameters.AddWithValue("@supplierId", SelectedSupplier.Id);
                    purchaseCommand.Parameters.AddWithValue("@date", PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    purchaseCommand.Parameters.AddWithValue("@subTotal", SubTotal);
                    purchaseCommand.Parameters.AddWithValue("@tax", Tax);
                    purchaseCommand.Parameters.AddWithValue("@total", Total);
                    purchaseCommand.Parameters.AddWithValue("@paymentStatus", PaymentStatus);
                    purchaseCommand.Parameters.AddWithValue("@amountPaid", AmountPaid);
                    purchaseCommand.Parameters.AddWithValue("@notes", Notes ?? "");
                    purchaseCommand.Parameters.AddWithValue("@userId", currentUser.Id);
                    purchaseCommand.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    var purchaseId = Convert.ToInt64(await purchaseCommand.ExecuteScalarAsync());

                    // Insert purchase items and update stock
                    foreach (var item in CartItems)
                    {
                        // Insert purchase item
                        var itemCommand = connection.CreateCommand();
                        itemCommand.Transaction = transaction;
                        itemCommand.CommandText = @"
                            INSERT INTO PurchaseItems (PurchaseId, ProductId, ProductCode, ProductName, Qty, CostPrice, Total)
                            VALUES (@purchaseId, @productId, @productCode, @productName, @qty, @costPrice, @total)
                        ";

                        itemCommand.Parameters.AddWithValue("@purchaseId", purchaseId);
                        itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                        itemCommand.Parameters.AddWithValue("@productCode", item.ProductCode);
                        itemCommand.Parameters.AddWithValue("@productName", item.ProductName);
                        itemCommand.Parameters.AddWithValue("@qty", item.Quantity);
                        itemCommand.Parameters.AddWithValue("@costPrice", item.CostPrice);
                        itemCommand.Parameters.AddWithValue("@total", item.Total);

                        await itemCommand.ExecuteNonQueryAsync();

                        // Update product stock and cost price
                        var stockCommand = connection.CreateCommand();
                        stockCommand.Transaction = transaction;
                        stockCommand.CommandText = @"
                            UPDATE Products 
                            SET StockQty = StockQty + @qty,
                                CostPrice = @costPrice,
                                UpdatedAt = @updatedAt
                            WHERE Id = @productId
                        ";

                        stockCommand.Parameters.AddWithValue("@qty", item.Quantity);
                        stockCommand.Parameters.AddWithValue("@costPrice", item.CostPrice);
                        stockCommand.Parameters.AddWithValue("@productId", item.ProductId);
                        stockCommand.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        await stockCommand.ExecuteNonQueryAsync();
                    }
                });

                if (success)
                {
                    MessageBox.Show($"Purchase saved successfully!\nPurchase Number: {PurchaseNumber}\nStock updated for {CartItems.Count} product(s).",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPurchaseHistoryAsync();
                    NewPurchase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving purchase: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPurchaseHistoryAsync()
        {
            try
            {
                const string query = @"
                    SELECT p.Id, p.PurchaseNumber, p.SupplierId, p.Date, p.SubTotal, p.Tax, p.Total, 
                           p.PaymentStatus, p.AmountPaid, p.Notes, p.UserId, p.CreatedAt, s.Name as SupplierName
                    FROM Purchases p
                    INNER JOIN Suppliers s ON p.SupplierId = s.Id
                    ORDER BY p.CreatedAt DESC
                    LIMIT 50
                ";

                var purchases = await _db.ExecuteQueryAsync(query, reader => new Purchase
                {
                    Id = reader.GetInt32(0),
                    PurchaseNumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    Date = DateTime.Parse(reader.GetString(3)),
                    SubTotal = (decimal)reader.GetDouble(4),
                    Tax = (decimal)reader.GetDouble(5),
                    Total = (decimal)reader.GetDouble(6),
                    PaymentStatus = reader.GetString(7),
                    AmountPaid = (decimal)reader.GetDouble(8),
                    Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                    UserId = reader.GetInt32(10),
                    CreatedAt = DateTime.Parse(reader.GetString(11)),
                    Supplier = new Supplier { Name = reader.GetString(12) }
                });

                PurchaseHistory.Clear();
                foreach (var purchase in purchases)
                {
                    PurchaseHistory.Add(purchase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase history: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearCart()
        {
            if (CartItems.Count > 0)
            {
                var result = MessageBox.Show("Clear all items from cart?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CartItems.Clear();
                    SelectedCartItem = null;
                }
            }
        }

        private void NewPurchase()
        {
            CartItems.Clear();
            SearchText = string.Empty;
            SearchResults.Clear();
            SelectedProduct = null;
            SelectedCartItem = null;
            SelectedSupplier = null;
            AmountPaid = 0;
            Tax = 0;
            Notes = string.Empty;
            PurchaseDate = DateTime.Now;
            PaymentStatus = "Pending";

            GeneratePurchaseNumber();
        }

        private void GeneratePurchaseNumber()
        {
            PurchaseNumber = $"PUR-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
        }

        private bool CanAddToCart()
        {
            return SelectedProduct != null;
        }

        private bool CanRemoveFromCart()
        {
            return SelectedCartItem != null;
        }

        private bool CanSavePurchase()
        {
            return SelectedSupplier != null && CartItems.Count > 0;
        }

        #endregion
    }

    // Purchase cart item model
    public class PurchaseCartItem : ViewModelBase
    {
        private int _quantity;
        private decimal _costPrice;

        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public decimal CostPrice
        {
            get => _costPrice;
            set => SetProperty(ref _costPrice, value);
        }

        public decimal Total => Quantity * CostPrice;
        public string TotalFormatted => $"Rs. {Total:N2}";
    }
}
