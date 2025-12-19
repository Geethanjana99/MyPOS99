using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly AuthenticationService _authService;
        
        private decimal _todaysSalesTotal;
        private int _totalProducts;
        private int _lowStockItemsCount;
        private int _totalCustomers;
        private string _userName = string.Empty;
        private string _userRole = string.Empty;

        public DashboardViewModel(DatabaseService databaseService, AuthenticationService authService)
        {
            _db = databaseService;
            _authService = authService;

            // Initialize commands
            NewSaleCommand = new RelayCommand(OpenNewSale);
            ProductsCommand = new RelayCommand(OpenProducts);
            CustomersCommand = new RelayCommand(OpenCustomers);
            SuppliersCommand = new RelayCommand(OpenSuppliers);
            ExpensesCommand = new RelayCommand(OpenExpenses);
            ReportsCommand = new RelayCommand(OpenReports);
            LogoutCommand = new RelayCommand(Logout);

            // Load user info
            LoadUserInfo();
            
            // Load dashboard data
            _ = LoadDashboardDataAsync();
        }

        #region Properties

        public decimal TodaysSalesTotal
        {
            get => _todaysSalesTotal;
            set => SetProperty(ref _todaysSalesTotal, value);
        }

        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
        }

        public int LowStockItemsCount
        {
            get => _lowStockItemsCount;
            set => SetProperty(ref _lowStockItemsCount, value);
        }

        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string UserRole
        {
            get => _userRole;
            set => SetProperty(ref _userRole, value);
        }

        public string TodaysSalesTotalFormatted => $"Rs. {TodaysSalesTotal:N2}";

        #endregion

        #region Commands

        public ICommand NewSaleCommand { get; }
        public ICommand ProductsCommand { get; }
        public ICommand CustomersCommand { get; }
        public ICommand SuppliersCommand { get; }
        public ICommand ExpensesCommand { get; }
        public ICommand ReportsCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        #region Methods

        private void LoadUserInfo()
        {
            var currentUser = ((App)Application.Current).CurrentUser;
            if (currentUser != null)
            {
                UserName = currentUser.Username;
                UserRole = currentUser.Role;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Get today's sales total
                const string salesQuery = @"
                    SELECT COALESCE(SUM(Total), 0) 
                    FROM Sales 
                    WHERE DATE(Date) = DATE('now')
                ";
                var salesResult = await _db.ExecuteScalarAsync(salesQuery);
                var todaysSales = salesResult != DBNull.Value ? Convert.ToDecimal(salesResult) : 0;

                // Get total products count
                const string productsQuery = "SELECT COUNT(*) FROM Products";
                var productsResult = await _db.ExecuteScalarAsync(productsQuery);
                var totalProducts = Convert.ToInt32(productsResult);

                // Get low stock items count
                const string lowStockQuery = @"
                    SELECT COUNT(*) 
                    FROM Products 
                    WHERE StockQty <= MinStockLevel
                ";
                var lowStockResult = await _db.ExecuteScalarAsync(lowStockQuery);
                var lowStock = Convert.ToInt32(lowStockResult);

                // Get total customers count
                const string customersQuery = "SELECT COUNT(*) FROM Customers WHERE IsActive = 1";
                var customersResult = await _db.ExecuteScalarAsync(customersQuery);
                var totalCustomers = Convert.ToInt32(customersResult);

                // Update properties on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TodaysSalesTotal = todaysSales;
                    OnPropertyChanged(nameof(TodaysSalesTotal));
                    OnPropertyChanged(nameof(TodaysSalesTotalFormatted));

                    TotalProducts = totalProducts;
                    OnPropertyChanged(nameof(TotalProducts));

                    LowStockItemsCount = lowStock;
                    OnPropertyChanged(nameof(LowStockItemsCount));

                    TotalCustomers = totalCustomers;
                    OnPropertyChanged(nameof(TotalCustomers));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenNewSale()
        {
            var salesWindow = new Views.SalesWindow();
            salesWindow.ShowDialog();

            // Refresh dashboard data after closing sales window
            _ = LoadDashboardDataAsync();
        }

        private void OpenProducts()
        {
            var productsWindow = new Views.ProductsWindow();
            productsWindow.ShowDialog();
            
            // Refresh dashboard data after closing products window
            _ = LoadDashboardDataAsync();
        }

        private void OpenCustomers()
        {
            MessageBox.Show("Customers module coming soon!", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenSuppliers()
        {
            MessageBox.Show("Suppliers module coming soon!", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenExpenses()
        {
            MessageBox.Show("Expenses module coming soon!", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenReports()
        {
            MessageBox.Show("Reports module coming soon!", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout()
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();
                ((App)Application.Current).CurrentUser = null;

                // Open login window
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();

                // Close current window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.DashboardWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }

        public async Task RefreshDataAsync()
        {
            await LoadDashboardDataAsync();
        }

        #endregion
    }
}
