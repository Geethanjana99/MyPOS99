using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private readonly DatabaseService _db;

        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Sale> _purchaseHistory;
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isEditing;

        // Form properties
        private int _customerId;
        private string _customerName = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private decimal _creditLimit;
        private bool _isCreditCustomer;
        private decimal _totalPurchases;
        private decimal _currentCredit;

        public CustomerViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            _customerService = new CustomerService(_db);

            _customers = new ObservableCollection<Customer>();
            _purchaseHistory = new ObservableCollection<Sale>();

            // Initialize commands
            SearchCommand = new RelayCommand(async () => await SearchCustomersAsync());
            AddCommand = new RelayCommand(async () => await AddCustomerAsync(), CanSaveCustomer);
            UpdateCommand = new RelayCommand(async () => await UpdateCustomerAsync(), CanUpdateCustomer);
            DeleteCommand = new RelayCommand(async () => await DeleteCustomerAsync(), CanDeleteCustomer);
            ClearFormCommand = new RelayCommand(ClearForm);
            ViewPurchaseHistoryCommand = new RelayCommand(async () => await LoadPurchaseHistoryAsync(), () => SelectedCustomer != null);

            // Load all customers on initialization
            _ = LoadCustomersAsync();
        }

        #region Properties

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<Sale> PurchaseHistory
        {
            get => _purchaseHistory;
            set => SetProperty(ref _purchaseHistory, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (value != null)
                    {
                        LoadCustomerToForm(value);
                    }
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)UpdateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ViewPurchaseHistoryCommand).RaiseCanExecuteChanged();
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
                    _ = SearchCustomersAsync();
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public int CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    ((RelayCommand)AddCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)UpdateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public decimal CreditLimit
        {
            get => _creditLimit;
            set => SetProperty(ref _creditLimit, value);
        }

        public bool IsCreditCustomer
        {
            get => _isCreditCustomer;
            set => SetProperty(ref _isCreditCustomer, value);
        }

        public decimal TotalPurchases
        {
            get => _totalPurchases;
            set => SetProperty(ref _totalPurchases, value);
        }

        public decimal CurrentCredit
        {
            get => _currentCredit;
            set => SetProperty(ref _currentCredit, value);
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand ViewPurchaseHistoryCommand { get; }

        #endregion

        #region Methods

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadCustomersAsync();
                    return;
                }

                var results = await _customerService.SearchCustomersAsync(SearchText);
                Customers.Clear();
                foreach (var customer in results)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddCustomerAsync()
        {
            try
            {
                var customer = new Customer
                {
                    Name = CustomerName.Trim(),
                    Phone = Phone?.Trim(),
                    Email = Email?.Trim(),
                    Address = Address?.Trim(),
                    CreditLimit = CreditLimit,
                    CurrentCredit = 0,
                    IsCreditCustomer = IsCreditCustomer,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var success = await _customerService.AddCustomerAsync(customer);
                
                if (success)
                {
                    MessageBox.Show("Customer added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to add customer.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateCustomerAsync()
        {
            try
            {
                if (CustomerId <= 0 || SelectedCustomer == null)
                {
                    MessageBox.Show("Please select a customer to update.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prevent updating Walk-in Customer
                if (CustomerId == 1)
                {
                    MessageBox.Show("Cannot modify the default Walk-in Customer.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var customer = new Customer
                {
                    Id = CustomerId,
                    Name = CustomerName.Trim(),
                    Phone = Phone?.Trim(),
                    Email = Email?.Trim(),
                    Address = Address?.Trim(),
                    CreditLimit = CreditLimit,
                    CurrentCredit = CurrentCredit,
                    IsCreditCustomer = IsCreditCustomer,
                    TotalPurchases = TotalPurchases,
                    IsActive = true
                };

                var success = await _customerService.UpdateCustomerAsync(customer);
                
                if (success)
                {
                    MessageBox.Show("Customer updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to update customer.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCustomerAsync()
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Please select a customer to delete.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prevent deleting Walk-in Customer
                if (SelectedCustomer.Id == 1)
                {
                    MessageBox.Show("Cannot delete the default Walk-in Customer.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to deactivate customer '{SelectedCustomer.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _customerService.DeactivateCustomerAsync(SelectedCustomer.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("Customer deactivated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        ClearForm();
                        await LoadCustomersAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to deactivate customer.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPurchaseHistoryAsync()
        {
            try
            {
                if (SelectedCustomer == null) return;

                var history = await _customerService.GetCustomerPurchaseHistoryAsync(SelectedCustomer.Id);
                PurchaseHistory.Clear();
                foreach (var sale in history)
                {
                    PurchaseHistory.Add(sale);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase history: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomerToForm(Customer customer)
        {
            CustomerId = customer.Id;
            CustomerName = customer.Name;
            Phone = customer.Phone ?? string.Empty;
            Email = customer.Email ?? string.Empty;
            Address = customer.Address ?? string.Empty;
            CreditLimit = customer.CreditLimit;
            CurrentCredit = customer.CurrentCredit;
            IsCreditCustomer = customer.IsCreditCustomer;
            TotalPurchases = customer.TotalPurchases;
            IsEditing = true;
        }

        private void ClearForm()
        {
            CustomerId = 0;
            CustomerName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            CreditLimit = 0;
            CurrentCredit = 0;
            IsCreditCustomer = false;
            TotalPurchases = 0;
            IsEditing = false;
            SelectedCustomer = null;
        }

        private bool CanSaveCustomer()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) && !IsEditing;
        }

        private bool CanUpdateCustomer()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) && IsEditing && CustomerId > 0;
        }

        private bool CanDeleteCustomer()
        {
            return SelectedCustomer != null && SelectedCustomer.Id != 1;
        }

        #endregion
    }
}
