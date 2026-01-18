using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly SupplierService _supplierService;
        private readonly DatabaseService _db;

        private ObservableCollection<Product> _products;
        private ObservableCollection<string> _categories;
        private ObservableCollection<Supplier> _suppliers;
        private Product? _selectedProduct;
        private string _searchText = string.Empty;
        private bool _isEditMode;
        private bool _isLoading;

        // Form fields
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string? _category;
        private decimal _costPrice;
        private decimal _sellPrice;
        private decimal? _discount;
        private int _stockQty;
        private int _minStockLevel;
        private string? _barcode;
        private Supplier? _selectedSupplier;

        public ProductViewModel(DatabaseService databaseService)
        {
            try
            {
                _db = databaseService;
                var dbContext = new DatabaseContext();
                _productService = new ProductService(dbContext);
                _categoryService = new CategoryService(databaseService);
                _supplierService = new SupplierService(_db);

                _products = new ObservableCollection<Product>();
                _categories = new ObservableCollection<string>();
                _suppliers = new ObservableCollection<Supplier>();

                // Initialize commands
                AddCommand = new RelayCommand(AddProduct);
                EditCommand = new RelayCommand(EditProduct, CanEditProduct);
                SaveCommand = new RelayCommand(async () => await SaveProductAsync(), CanSaveProduct);
                DeleteCommand = new RelayCommand(async () => await DeleteProductAsync(), CanEditProduct);
                CancelCommand = new RelayCommand(CancelEdit);
                SearchCommand = new RelayCommand(async () => await SearchProductsAsync());
                RefreshCommand = new RelayCommand(async () => await LoadProductsAsync());

                // Load data
                _ = LoadProductsAsync();
                _ = LoadCategoriesAsync();
                _ = LoadSuppliersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ProductViewModel constructor:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}", 
                    "ViewModel Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        #region Properties

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Form Fields
        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string? Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                if (SetProperty(ref _costPrice, value))
                {
                    OnPropertyChanged(nameof(ProfitMargin));
                }
            }
        }

        public decimal SellPrice
        {
            get => _sellPrice;
            set
            {
                if (SetProperty(ref _sellPrice, value))
                {
                    OnPropertyChanged(nameof(ProfitMargin));
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public decimal? Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        public int StockQty
        {
            get => _stockQty;
            set => SetProperty(ref _stockQty, value);
        }

        public int MinStockLevel
        {
            get => _minStockLevel;
            set => SetProperty(ref _minStockLevel, value);
        }

        public string? Barcode
        {
            get => _barcode;
            set => SetProperty(ref _barcode, value);
        }

        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set => SetProperty(ref _selectedSupplier, value);
        }

        public decimal ProfitMargin
        {
            get
            {
                if (CostPrice > 0)
                    return ((SellPrice - CostPrice) / CostPrice) * 100;
                return 0;
            }
        }

        public string ProfitMarginFormatted => $"{ProfitMargin:F2}%";

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                var products = await _productService.GetAllProductsAsync();
                
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category.Name);
                }
            }
            catch (Exception ex)
            {
                        MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

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

                private void AddProduct()
        {
            ClearForm();
            IsEditMode = true;
            SelectedProduct = null;
        }

        private void EditProduct()
        {
            if (SelectedProduct != null)
            {
                Code = SelectedProduct.Code;
                Name = SelectedProduct.Name;
                Category = SelectedProduct.Category;
                CostPrice = SelectedProduct.CostPrice;
                SellPrice = SelectedProduct.SellPrice;
                StockQty = SelectedProduct.StockQty;
                MinStockLevel = SelectedProduct.MinStockLevel;
                Barcode = SelectedProduct.Barcode;
                SelectedSupplier = SelectedProduct.SupplierId.HasValue 
                    ? Suppliers.FirstOrDefault(s => s.Id == SelectedProduct.SupplierId.Value) 
                    : null;

                IsEditMode = true;
            }
        }

        private async Task SaveProductAsync()
        {
            try
            {
                IsLoading = true;

                var product = new Product
                {
                    Code = Code,
                    Name = Name,
                    Category = Category,
                    CostPrice = CostPrice,
                    SellPrice = SellPrice,
                    StockQty = StockQty,
                    MinStockLevel = MinStockLevel,
                    Barcode = Barcode,
                    SupplierId = SelectedSupplier?.Id
                };

                bool success;
                if (SelectedProduct != null)
                {
                    // Update existing product
                    product.Id = SelectedProduct.Id;
                    success = await _productService.UpdateProductAsync(product);
                    
                    if (success)
                    {
                        MessageBox.Show("Product updated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Add new product
                    success = await _productService.AddProductAsync(product);
                    
                    if (success)
                    {
                        MessageBox.Show("Product added successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (success)
                {
                    await LoadProductsAsync();
                    ClearForm();
                    IsEditMode = false;
                }
                else
                {
                    MessageBox.Show("Failed to save product.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedProduct.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _productService.DeleteProductAsync(SelectedProduct.Id);

                    if (success)
                    {
                        MessageBox.Show("Product deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        await LoadProductsAsync();
                        ClearForm();
                        IsEditMode = false;
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete product.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void CancelEdit()
        {
            ClearForm();
            IsEditMode = false;
            SelectedProduct = null;
        }

        private async Task SearchProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProductsAsync();
                return;
            }

            try
            {
                IsLoading = true;

                const string query = @"
                    SELECT p.Id, p.Code, p.Name, p.Category, p.CostPrice, p.SellPrice, 
                           p.StockQty, p.MinStockLevel, p.Barcode, p.SupplierId, 
                           p.CreatedAt, p.UpdatedAt, s.Name as SupplierName
                    FROM Products p
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                    WHERE p.Code LIKE @search OR p.Name LIKE @search OR p.Category LIKE @search OR p.Barcode LIKE @search
                    ORDER BY p.Name
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
                    SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    CreatedAt = DateTime.Parse(reader.GetString(10)),
                    UpdatedAt = DateTime.Parse(reader.GetString(11)),
                    Supplier = reader.IsDBNull(12) ? null : new Supplier { Name = reader.GetString(12) }
                }, DatabaseService.CreateParameter("@search", $"%{SearchText}%"));

                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            Code = string.Empty;
            Name = string.Empty;
            Category = null;
            CostPrice = 0;
            SellPrice = 0;
            Discount = null;
            StockQty = 0;
            MinStockLevel = 0;
            Barcode = null;
            SelectedSupplier = null;
        }

        private bool CanSaveProduct()
        {
            return !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   SellPrice > 0 &&
                   !IsLoading;
        }

        private bool CanEditProduct()
        {
            return SelectedProduct != null && !IsLoading;
        }

        #endregion
    }
}
