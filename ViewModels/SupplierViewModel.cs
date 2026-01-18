using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class SupplierViewModel : ViewModelBase
    {
        private readonly SupplierService _supplierService;
        private readonly DatabaseService _db;

        private ObservableCollection<Supplier> _suppliers;
        private Supplier? _selectedSupplier;
        private string _searchText = string.Empty;
        private bool _isEditing;

        // Form properties
        private int _supplierId;
        private string _supplierName = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;

        public SupplierViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            _supplierService = new SupplierService(_db);

            _suppliers = new ObservableCollection<Supplier>();

            // Initialize commands
            SearchCommand = new RelayCommand(async () => await SearchSuppliersAsync());
            AddCommand = new RelayCommand(async () => await AddSupplierAsync(), CanSaveSupplier);
            UpdateCommand = new RelayCommand(async () => await UpdateSupplierAsync(), CanUpdateSupplier);
            DeleteCommand = new RelayCommand(async () => await DeleteSupplierAsync(), CanDeleteSupplier);
            ClearFormCommand = new RelayCommand(ClearForm);

            // Load all suppliers
            _ = LoadSuppliersAsync();
        }

        #region Properties

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                {
                    if (value != null)
                    {
                        LoadSupplierToForm(value);
                    }
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)UpdateCommand).RaiseCanExecuteChanged();
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
                    _ = SearchSuppliersAsync();
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public int SupplierId
        {
            get => _supplierId;
            set => SetProperty(ref _supplierId, value);
        }

        public string SupplierName
        {
            get => _supplierName;
            set
            {
                if (SetProperty(ref _supplierName, value))
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

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFormCommand { get; }

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

        private async Task SearchSuppliersAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadSuppliersAsync();
                    return;
                }

                var results = await _supplierService.SearchSuppliersAsync(SearchText);
                Suppliers.Clear();
                foreach (var supplier in results)
                {
                    Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching suppliers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddSupplierAsync()
        {
            try
            {
                var supplier = new Supplier
                {
                    Name = SupplierName.Trim(),
                    Phone = Phone?.Trim(),
                    Email = Email?.Trim(),
                    Address = Address?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var success = await _supplierService.AddSupplierAsync(supplier);

                if (success)
                {
                    MessageBox.Show("Supplier added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    await LoadSuppliersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to add supplier.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding supplier: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateSupplierAsync()
        {
            try
            {
                if (SupplierId <= 0 || SelectedSupplier == null)
                {
                    MessageBox.Show("Please select a supplier to update.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var supplier = new Supplier
                {
                    Id = SupplierId,
                    Name = SupplierName.Trim(),
                    Phone = Phone?.Trim(),
                    Email = Email?.Trim(),
                    Address = Address?.Trim(),
                    IsActive = true
                };

                var success = await _supplierService.UpdateSupplierAsync(supplier);

                if (success)
                {
                    MessageBox.Show("Supplier updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    await LoadSuppliersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to update supplier.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating supplier: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteSupplierAsync()
        {
            try
            {
                if (SelectedSupplier == null)
                {
                    MessageBox.Show("Please select a supplier to delete.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to deactivate supplier '{SelectedSupplier.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _supplierService.DeactivateSupplierAsync(SelectedSupplier.Id);

                    if (success)
                    {
                        MessageBox.Show("Supplier deactivated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        ClearForm();
                        await LoadSuppliersAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to deactivate supplier.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSupplierToForm(Supplier supplier)
        {
            SupplierId = supplier.Id;
            SupplierName = supplier.Name;
            Phone = supplier.Phone ?? string.Empty;
            Email = supplier.Email ?? string.Empty;
            Address = supplier.Address ?? string.Empty;
            IsEditing = true;
        }

        private void ClearForm()
        {
            SupplierId = 0;
            SupplierName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            IsEditing = false;
            SelectedSupplier = null;
        }

        private bool CanSaveSupplier()
        {
            return !string.IsNullOrWhiteSpace(SupplierName) && !IsEditing;
        }

        private bool CanUpdateSupplier()
        {
            return !string.IsNullOrWhiteSpace(SupplierName) && IsEditing && SupplierId > 0;
        }

        private bool CanDeleteSupplier()
        {
            return SelectedSupplier != null;
        }

        #endregion
    }
}
