using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class SalesViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly ProductService _productService;
        private readonly SaleService _saleService;
        private readonly PdfReceiptService _pdfService;

        private ObservableCollection<CartItem> _cartItems;
        private ObservableCollection<Sale> _recentSales;
        private ObservableCollection<Product> _searchResults;
        
        private string _searchText = string.Empty;
        private Product? _selectedProduct;
        private CartItem? _selectedCartItem;
        
        private decimal _subTotal;
        private decimal _totalDiscount;
        private decimal _tax;
        private decimal _grandTotal;
        private decimal _amountPaid;
        private decimal _change;
        private decimal _todaysTotalSales;
        private decimal _globalDiscount;

        private string _paymentType = "Cash";
        private string _customerName = "Walk-in Customer";
        private string _notes = string.Empty;

        private bool _isProcessing;
        private string _currentInvoiceNumber = string.Empty;

        public SalesViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            var dbContext = new DatabaseContext();
            _productService = new ProductService(dbContext);
            _saleService = new SaleService(dbContext);
            _pdfService = new PdfReceiptService();

            _cartItems = new ObservableCollection<CartItem>();
            _recentSales = new ObservableCollection<Sale>();
            _searchResults = new ObservableCollection<Product>();
            
            // Subscribe to cart changes
            _cartItems.CollectionChanged += (s, e) => CalculateTotals();
            
            // Initialize commands
            SearchProductCommand = new RelayCommand(async () => await SearchProductsAsync());
            AddToCartCommand = new RelayCommand(AddToCart, CanAddToCart);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart, CanRemoveFromCart);
            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity, CanRemoveFromCart);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity, CanRemoveFromCart);
            ProcessSaleCommand = new RelayCommand(async () => await ProcessSaleAsync(), CanProcessSale);
            ClearCartCommand = new RelayCommand(ClearCart);
            PrintReceiptCommand = new RelayCommand(PrintReceipt, CanPrintReceipt);
            NewSaleCommand = new RelayCommand(NewSale);
            ViewInvoiceCommand = new RelayCommand<Sale>(ViewInvoice);
            ProcessReturnCommand = new RelayCommand<Sale>(ProcessReturn, CanProcessReturn);

                // Generate invoice number
                GenerateInvoiceNumber();

                // Load recent sales and today's total
                _ = LoadRecentSalesAsync();
                _ = LoadTodaysTotalSalesAsync();
            }

        #region Properties

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public ObservableCollection<Sale> RecentSales
        {
            get => _recentSales;
            set => SetProperty(ref _recentSales, value);
        }

        public ObservableCollection<Product> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Automatically search as user types
                    _ = SearchProductsAsync();
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

        public CartItem? SelectedCartItem
        {
            get => _selectedCartItem;
            set
            {
                if (SetProperty(ref _selectedCartItem, value))
                {
                    ((RelayCommand)RemoveFromCartCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)IncreaseQuantityCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DecreaseQuantityCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        public decimal TotalDiscount
        {
            get => _totalDiscount;
            set => SetProperty(ref _totalDiscount, value);
        }

        public decimal Tax
        {
            get => _tax;
            set => SetProperty(ref _tax, value);
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            set => SetProperty(ref _grandTotal, value);
        }

        public decimal AmountPaid
        {
            get => _amountPaid;
            set
            {
                if (SetProperty(ref _amountPaid, value))
                {
                    CalculateChange();
                    ((RelayCommand)ProcessSaleCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public decimal Change
        {
            get => _change;
            set => SetProperty(ref _change, value);
        }

        public decimal GlobalDiscount
        {
            get => _globalDiscount;
            set
            {
                if (SetProperty(ref _globalDiscount, value))
                {
                    CalculateTotals();
                }
            }
        }

        public string PaymentType
        {
            get => _paymentType;
            set => SetProperty(ref _paymentType, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public string CurrentInvoiceNumber
        {
            get => _currentInvoiceNumber;
            set => SetProperty(ref _currentInvoiceNumber, value);
        }

        public decimal TodaysTotalSales
        {
            get => _todaysTotalSales;
            set => SetProperty(ref _todaysTotalSales, value);
        }

        public string SubTotalFormatted => $"Rs. {SubTotal:N2}";
        public string TotalDiscountFormatted => $"Rs. {TotalDiscount:N2}";
        public string GlobalDiscountFormatted => $"Rs. {GlobalDiscount:N2}";
        public string TaxFormatted => $"Rs. {Tax:N2}";
        public string GrandTotalFormatted => $"Rs. {GrandTotal:N2}";
        public string ChangeFormatted => $"Rs. {Change:N2}";
        public string TodaysTotalSalesFormatted => $"Rs. {TodaysTotalSales:N2}";

        public List<string> PaymentTypes => new List<string> { "Cash", "Card", "Mobile", "Credit" };

        #endregion

        #region Commands

        public ICommand SearchProductCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ProcessSaleCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand PrintReceiptCommand { get; }
        public ICommand NewSaleCommand { get; }
        public ICommand ViewInvoiceCommand { get; }
        public ICommand ProcessReturnCommand { get; }

        #endregion

        #region Methods

        private async Task SearchProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                return;
            }

            try
            {
                const string query = @"
                    SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, CreatedAt, UpdatedAt
                    FROM Products
                    WHERE (Code LIKE @search OR Name LIKE @search OR Barcode LIKE @search)
                    AND StockQty > 0
                    ORDER BY Name
                    LIMIT 10
                ";

                var products = await _db.ExecuteQueryAsync(query, reader => new Product
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CostPrice = (decimal)reader.GetDouble(4),
                    SellPrice = (decimal)reader.GetDouble(5),
                    StockQty = reader.GetInt32(6),
                    MinStockLevel = reader.GetInt32(7),
                    Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                    CreatedAt = DateTime.Parse(reader.GetString(9)),
                    UpdatedAt = DateTime.Parse(reader.GetString(10))
                }, DatabaseService.CreateParameter("@search", $"%{SearchText}%"));

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

            // Show dialog to enter quantity and discount
            var dialog = new Views.AddToCartDialog(SelectedProduct);
            if (dialog.ShowDialog() == true)
            {
                var quantity = dialog.Quantity;
                var discount = dialog.Discount;

                // Check stock availability
                if (quantity > SelectedProduct.StockQty)
                {
                    MessageBox.Show($"Only {SelectedProduct.StockQty} units available in stock!",
                        "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if product already in cart (but not return items)
                var existingItem = CartItems.FirstOrDefault(x => x.ProductId == SelectedProduct.Id && x.Price > 0);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += quantity;
                    existingItem.Discount = discount;

                    // Manually trigger totals recalculation since CollectionChanged won't fire
                    CalculateTotals();
                }
                else
                {
                    // Add new item to cart
                    var cartItem = new CartItem
                    {
                        ProductId = SelectedProduct.Id,
                        ProductCode = SelectedProduct.Code,
                        ProductName = SelectedProduct.Name,
                        Price = SelectedProduct.SellPrice,
                        Quantity = quantity,
                        Discount = discount,
                        AvailableStock = SelectedProduct.StockQty
                    };

                    CartItems.Add(cartItem);
                }
            }

            // Clear search
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

        private void IncreaseQuantity()
        {
            if (SelectedCartItem != null)
            {
                if (SelectedCartItem.Quantity < SelectedCartItem.AvailableStock)
                {
                    SelectedCartItem.Quantity++;
                    CalculateTotals(); // Recalculate totals after quantity change
                }
                else
                {
                    MessageBox.Show($"Only {SelectedCartItem.AvailableStock} units available in stock!",
                        "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DecreaseQuantity()
        {
            if (SelectedCartItem != null && SelectedCartItem.Quantity > 1)
            {
                SelectedCartItem.Quantity--;
                CalculateTotals(); // Recalculate totals after quantity change
            }
        }

        private void CalculateTotals()
        {
            // Calculate subtotal BEFORE any discounts (Price * Quantity for all items)
            SubTotal = CartItems.Sum(x => x.Price * x.Quantity);

            // Calculate total discount (item discounts + global discount)
            TotalDiscount = CartItems.Sum(x => x.TotalDiscount) + GlobalDiscount;

            // Tax on subtotal (before discount)
            Tax = SubTotal * 0; // Set tax rate if needed (e.g., 0.12 for 12%)

            // Grand total = SubTotal - Discounts + Tax
            GrandTotal = SubTotal - TotalDiscount + Tax;

            OnPropertyChanged(nameof(SubTotalFormatted));
            OnPropertyChanged(nameof(TotalDiscountFormatted));
            OnPropertyChanged(nameof(GlobalDiscountFormatted));
            OnPropertyChanged(nameof(TaxFormatted));
            OnPropertyChanged(nameof(GrandTotalFormatted));

            CalculateChange();
            ((RelayCommand)ProcessSaleCommand).RaiseCanExecuteChanged();
        }

        private void CalculateChange()
        {
            Change = AmountPaid - GrandTotal;
            OnPropertyChanged(nameof(ChangeFormatted));
        }

        public async Task ProcessSaleAsync()
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AmountPaid < GrandTotal)
            {
                MessageBox.Show("Amount paid is less than total!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsProcessing = true;

                // Get current user
                var currentUser = ((App)Application.Current).CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("User not authenticated!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create sale
                var sale = new Sale
                {
                    InvoiceNumber = CurrentInvoiceNumber,
                    Date = DateTime.Now,
                    SubTotal = SubTotal,
                    Discount = TotalDiscount,
                    Tax = Tax,
                    Total = GrandTotal,
                    PaymentType = PaymentType,
                    AmountPaid = AmountPaid,
                    Change = Change,
                    UserId = currentUser.Id,
                    Notes = Notes
                };

                // Create sale items
                var saleItems = CartItems.Select(item => new SaleItem
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    Qty = item.Quantity,
                    Price = item.Price,
                    Discount = item.Discount,
                    Total = item.LineTotal
                }).ToList();

                // Process sale in transaction
                bool success = await _db.ExecuteTransactionAsync(async (connection, transaction) =>
                {
                    // Insert sale
                    var saleCommand = connection.CreateCommand();
                    saleCommand.Transaction = transaction;
                    saleCommand.CommandText = @"
                        INSERT INTO Sales (InvoiceNumber, Date, SubTotal, Discount, Tax, Total, PaymentType, AmountPaid, Change, UserId, Notes, CreatedAt)
                        VALUES (@invoiceNumber, @date, @subTotal, @discount, @tax, @total, @paymentType, @amountPaid, @change, @userId, @notes, @createdAt);
                        SELECT last_insert_rowid();
                    ";
                    
                    saleCommand.Parameters.AddWithValue("@invoiceNumber", sale.InvoiceNumber);
                    saleCommand.Parameters.AddWithValue("@date", sale.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    saleCommand.Parameters.AddWithValue("@subTotal", sale.SubTotal);
                    saleCommand.Parameters.AddWithValue("@discount", sale.Discount);
                    saleCommand.Parameters.AddWithValue("@tax", sale.Tax);
                    saleCommand.Parameters.AddWithValue("@total", sale.Total);
                    saleCommand.Parameters.AddWithValue("@paymentType", sale.PaymentType);
                    saleCommand.Parameters.AddWithValue("@amountPaid", sale.AmountPaid);
                    saleCommand.Parameters.AddWithValue("@change", sale.Change);
                    saleCommand.Parameters.AddWithValue("@userId", sale.UserId);
                    saleCommand.Parameters.AddWithValue("@notes", sale.Notes ?? "");
                    saleCommand.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    var saleId = Convert.ToInt64(await saleCommand.ExecuteScalarAsync());

                    // Insert sale items and update stock
                    foreach (var item in saleItems)
                    {
                        // Insert sale item
                        var itemCommand = connection.CreateCommand();
                        itemCommand.Transaction = transaction;
                        itemCommand.CommandText = @"
                            INSERT INTO SaleItems (SaleId, ProductId, ProductCode, ProductName, Qty, Price, Discount, Total)
                            VALUES (@saleId, @productId, @productCode, @productName, @qty, @price, @discount, @total)
                        ";
                        
                        itemCommand.Parameters.AddWithValue("@saleId", saleId);
                        itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                        itemCommand.Parameters.AddWithValue("@productCode", item.ProductCode);
                        itemCommand.Parameters.AddWithValue("@productName", item.ProductName);
                        itemCommand.Parameters.AddWithValue("@qty", item.Qty);
                        itemCommand.Parameters.AddWithValue("@price", item.Price);
                        itemCommand.Parameters.AddWithValue("@discount", item.Discount);
                        itemCommand.Parameters.AddWithValue("@total", item.Total);
                        
                                await itemCommand.ExecuteNonQueryAsync();

                                // Update product stock
                                // For return items (negative price), add to stock
                                // For regular items (positive price), subtract from stock
                                var stockCommand = connection.CreateCommand();
                                stockCommand.Transaction = transaction;

                                if (item.Price < 0)
                                {
                                    // Return item - add back to stock
                                    stockCommand.CommandText = @"
                                        UPDATE Products 
                                        SET StockQty = StockQty + @qty, UpdatedAt = @updatedAt
                                        WHERE Id = @productId
                                    ";
                                }
                                else
                                {
                                    // Regular item - subtract from stock
                                    stockCommand.CommandText = @"
                                        UPDATE Products 
                                        SET StockQty = StockQty - @qty, UpdatedAt = @updatedAt
                                        WHERE Id = @productId
                                    ";
                                }

                                stockCommand.Parameters.AddWithValue("@qty", item.Qty);
                                stockCommand.Parameters.AddWithValue("@productId", item.ProductId);
                                stockCommand.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                await stockCommand.ExecuteNonQueryAsync();
                            }
                        });

                if (success)
                {
                    // Store invoice number before clearing
                    var completedInvoiceNumber = CurrentInvoiceNumber;

                    // Refresh recent sales and today's total FIRST
                    await LoadRecentSalesAsync();
                    await LoadTodaysTotalSalesAsync();

                    // Wait to ensure everything is loaded
                    await Task.Delay(200);

                    // Notify caller that sale was successful
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Sale completed successfully!\nInvoice: {completedInvoiceNumber}", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    });

                    // Clear for new sale
                    NewSale();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing sale: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
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

        private void PrintReceipt()
        {
            try
            {
                if (RecentSales.Count == 0)
                {
                    MessageBox.Show("No recent sale to print.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var lastSale = RecentSales[0];

                // Get sale items from database
                var items = GetSaleItems(lastSale.Id);

                if (items.Count == 0)
                {
                    MessageBox.Show("No items found for this sale.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Generate PDF
                var cashierName = ((App)Application.Current).CurrentUser?.Username ?? "Unknown";
                var customerName = "Walk-in Customer"; // Could be retrieved from database if stored
                var pdfPath = _pdfService.GenerateReceipt(lastSale, items, customerName, cashierName);

                // Open PDF
                _pdfService.OpenPdf(pdfPath);

                MessageBox.Show($"Receipt generated successfully!\nSaved to: {pdfPath}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt: {ex.Message}\n\nDetails: {ex.StackTrace}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

                private List<SaleItem> GetSaleItems(int saleId)
                {
                    try
                    {
                        const string query = @"
                            SELECT Id, SaleId, ProductId, ProductName, Qty, Price, Discount, Total
                            FROM SaleItems
                            WHERE SaleId = @saleId
                        ";

                        var items = _db.ExecuteQueryAsync(query, reader => new SaleItem
                        {
                            Id = reader.GetInt32(0),
                            SaleId = reader.GetInt32(1),
                            ProductId = reader.GetInt32(2),
                            ProductName = reader.GetString(3),
                            Qty = reader.GetInt32(4),
                            Price = (decimal)reader.GetDouble(5),
                            Discount = (decimal)reader.GetDouble(6),
                            Total = (decimal)reader.GetDouble(7)
                        }, DatabaseService.CreateParameter("@saleId", saleId)).Result.ToList();

                        return items;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading sale items: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return new List<SaleItem>();
                    }
                }

                        private void ViewInvoice(Sale? sale)
                        {
                            if (sale == null)
                            {
                                MessageBox.Show("No sale selected.", "Info",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            try
                            {
                                // Try to find existing PDF
                                var pdfPath = _pdfService.FindReceiptByInvoiceNumber(sale.InvoiceNumber);

                                if (pdfPath != null && File.Exists(pdfPath))
                                {
                                    // Open existing PDF
                                    _pdfService.OpenPdf(pdfPath);
                                }
                                else
                                {
                                    // Generate PDF for this sale
                                    var items = GetSaleItems(sale.Id);

                                    if (items.Count == 0)
                                    {
                                        MessageBox.Show($"No items found for invoice {sale.InvoiceNumber}.", "Error",
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    var cashierName = "Unknown"; // Could be fetched from UserId if needed
                                    var customerName = "Walk-in Customer"; // Could be fetched from CustomerId if needed

                                    pdfPath = _pdfService.GenerateReceipt(sale, items, customerName, cashierName);
                                    _pdfService.OpenPdf(pdfPath);

                                    MessageBox.Show($"Invoice generated successfully!\nSaved to: {pdfPath}", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show($"Error opening invoice: {ex.Message}\n\nDetails: {ex.StackTrace}", "Error",
                                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }

                            private async void ProcessReturn(Sale? sale)
                            {
                                if (sale == null) return;

                                try
                                {
                                    // Get sale items
                                    var items = GetSaleItems(sale.Id);

                                    if (items.Count == 0)
                                    {
                                        MessageBox.Show("No items found for this sale.", "Error",
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    // Show return dialog
                                    var returnDialog = new Views.ProcessReturnDialog(sale, items);
                                    if (returnDialog.ShowDialog() == true)
                                    {
                                        // Generate return number
                                        var returnNumber = $"RET-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";

                                        // Create return record
                                        var returnRecord = new Return
                                        {
                                            ReturnNumber = returnNumber,
                                            SaleId = sale.Id,
                                            OriginalInvoiceNumber = sale.InvoiceNumber,
                                            ReturnDate = DateTime.Now,
                                            TotalAmount = returnDialog.ReturnAmount,
                                            Reason = returnDialog.ReturnReason,
                                            ProcessedByUserId = ((App)Application.Current).CurrentUser?.Id ?? 1,
                                            Notes = $"Returned {returnDialog.SelectedItemsToReturn.Count} item(s)"
                                        };

                                        // Create return items
                                        var returnItems = returnDialog.SelectedItemsToReturn.Select(item => new ReturnItem
                                        {
                                            ProductId = item.ProductId,
                                            ProductName = item.ProductName,
                                            Quantity = item.Qty,
                                            Price = item.Price,
                                            Total = item.Total
                                        }).ToList();

                                        // Process return
                                        var returnService = new ReturnService();
                                        await returnService.CreateReturnAsync(returnRecord, returnItems);

                                        // Reload data
                                        await LoadRecentSalesAsync();
                                        await LoadTodaysTotalSalesAsync();

                                        MessageBox.Show(
                                            $"Return processed successfully!\n\n" +
                                            $"Return Number: {returnNumber}\n" +
                                            $"Amount: Rs. {returnDialog.ReturnAmount:N2}\n\n" +
                                            $"Items have been added back to stock.\n" +
                                            $"Sales total has been adjusted.",
                                            "Return Successful",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error processing return: {ex.Message}\n\nDetails: {ex.StackTrace}",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }

                            private void NewSale()
                            {
                                CartItems.Clear();
                                SearchText = string.Empty;
                                SearchResults.Clear();
                                SelectedProduct = null;
                                SelectedCartItem = null;
                                AmountPaid = 0;
                                GlobalDiscount = 0;
                                CustomerName = "Walk-in Customer";
                                Notes = string.Empty;
                                PaymentType = "Cash";

                                GenerateInvoiceNumber();
        }

        private void GenerateInvoiceNumber()
        {
            CurrentInvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
        }

        private async Task LoadRecentSalesAsync()
        {
            try
            {
                const string query = @"
                    SELECT Id, InvoiceNumber, Date, SubTotal, Discount, Tax, Total, PaymentType, AmountPaid, Change, UserId, CustomerId, Notes, CreatedAt
                    FROM Sales
                    ORDER BY CreatedAt DESC
                    LIMIT 10
                ";

                var sales = await _db.ExecuteQueryAsync(query, reader => new Sale
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    Date = DateTime.Parse(reader.GetString(2)),
                    SubTotal = (decimal)reader.GetDouble(3),
                    Discount = (decimal)reader.GetDouble(4),
                    Tax = (decimal)reader.GetDouble(5),
                    Total = (decimal)reader.GetDouble(6),
                    PaymentType = reader.GetString(7),
                    AmountPaid = (decimal)reader.GetDouble(8),
                    Change = (decimal)reader.GetDouble(9),
                    UserId = reader.GetInt32(10),
                    CustomerId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    Notes = reader.IsDBNull(12) ? null : reader.GetString(12),
                    CreatedAt = DateTime.Parse(reader.GetString(13))
                });

                // Update collection on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RecentSales.Clear();
                    foreach (var sale in sales)
                    {
                        RecentSales.Add(sale);
                    }

                    // Explicitly notify that collection has changed
                    OnPropertyChanged(nameof(RecentSales));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recent sales: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTodaysTotalSalesAsync()
        {
            try
            {
                const string query = @"
                    SELECT COALESCE(SUM(Total), 0) 
                    FROM Sales 
                    WHERE DATE(Date) = DATE('now')
                ";

                var result = await _db.ExecuteScalarAsync(query);
                TodaysTotalSales = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                // Explicitly notify property changes
                OnPropertyChanged(nameof(TodaysTotalSales));
                OnPropertyChanged(nameof(TodaysTotalSalesFormatted));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading today's sales: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddToCart()
        {
            return SelectedProduct != null && SelectedProduct.StockQty > 0;
        }

        private bool CanRemoveFromCart()
        {
            return SelectedCartItem != null;
        }

        private bool CanProcessSale()
        {
            return CartItems.Count > 0 && AmountPaid >= GrandTotal && !IsProcessing;
        }

        private bool CanPrintReceipt()
        {
            return RecentSales.Count > 0;
        }

        private bool CanProcessReturn(Sale? sale)
        {
            return sale != null;
        }

        #endregion
    }

    // Cart Item Helper Class
    public class CartItem : ViewModelBase
    {
        private int _quantity;
        private decimal _discount;

        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                    OnPropertyChanged(nameof(LineTotalFormatted));
                    OnPropertyChanged(nameof(TotalDiscount));
                }
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                if (SetProperty(ref _discount, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                    OnPropertyChanged(nameof(LineTotalFormatted));
                    OnPropertyChanged(nameof(TotalDiscount));
                }
            }
        }

        public int AvailableStock { get; set; }

        // Identify if this is a return item (negative price)
        public bool IsReturnItem => Price < 0;

        // Background color for UI styling
        public string BackgroundColor => IsReturnItem ? "#FFCCCB" : "Transparent";

                public decimal TotalDiscount => Discount * Quantity;
                public decimal LineTotal => (Price * Quantity) - TotalDiscount;
                public string LineTotalFormatted => $"Rs. {LineTotal:N2}";
            }
        }
