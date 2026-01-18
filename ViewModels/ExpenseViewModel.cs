using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class ExpenseViewModel : ViewModelBase
    {
        private readonly ExpenseService _expenseService;
        private readonly DatabaseService _db;

        private ObservableCollection<Expense> _expenses;
        private ObservableCollection<string> _categories;
        private Expense? _selectedExpense;

        // Form fields
        private string _selectedCategory = string.Empty;
        private decimal _amount;
        private DateTime _expenseDate = DateTime.Now;
        private string _note = string.Empty;
        private string _paymentMethod = "Cash";

        // Filter fields
        private DateTime _filterStartDate = DateTime.Now.Date;
        private DateTime _filterEndDate = DateTime.Now.Date;

        // Totals
        private decimal _todaysTotalExpenses;
        private decimal _filteredTotalExpenses;

        public ExpenseViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            var dbContext = new DatabaseContext();
            _expenseService = new ExpenseService(dbContext);

            _expenses = new ObservableCollection<Expense>();
            _categories = new ObservableCollection<string>
            {
                "Utilities",
                "Rent",
                "Salaries",
                "Transportation",
                "Supplies",
                "Marketing",
                "Maintenance",
                "Cash Out",
                "Other"
            };

            // Initialize commands
            AddExpenseCommand = new RelayCommand(async () => await AddExpenseAsync(), CanAddExpense);
            FilterExpensesCommand = new RelayCommand(async () => await LoadExpensesByDateRangeAsync());
            ClearFormCommand = new RelayCommand(ClearForm);
            ExportReportCommand = new RelayCommand(ExportReport, () => Expenses.Count > 0);

            // Load today's expenses by default
            _ = LoadTodaysTotalExpensesAsync();
            _ = LoadExpensesByDateRangeAsync();
        }

        #region Properties

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Expense? SelectedExpense
        {
            get => _selectedExpense;
            set => SetProperty(ref _selectedExpense, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ((RelayCommand)AddExpenseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (SetProperty(ref _amount, value))
                {
                    ((RelayCommand)AddExpenseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime ExpenseDate
        {
            get => _expenseDate;
            set => SetProperty(ref _expenseDate, value);
        }

        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public DateTime FilterStartDate
        {
            get => _filterStartDate;
            set => SetProperty(ref _filterStartDate, value);
        }

        public DateTime FilterEndDate
        {
            get => _filterEndDate;
            set => SetProperty(ref _filterEndDate, value);
        }

        public decimal TodaysTotalExpenses
        {
            get => _todaysTotalExpenses;
            set => SetProperty(ref _todaysTotalExpenses, value);
        }

        public decimal FilteredTotalExpenses
        {
            get => _filteredTotalExpenses;
            set => SetProperty(ref _filteredTotalExpenses, value);
        }

        public string TodaysTotalExpensesFormatted => $"Rs. {TodaysTotalExpenses:N2}";
        public string FilteredTotalExpensesFormatted => $"Rs. {FilteredTotalExpenses:N2}";

        public List<string> PaymentMethods => new List<string> { "Cash", "Card", "Bank Transfer", "Mobile" };

        #endregion

        #region Commands

        public ICommand AddExpenseCommand { get; }
        public ICommand FilterExpensesCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand ExportReportCommand { get; }

        #endregion

        #region Methods

        private async Task LoadTodaysTotalExpensesAsync()
        {
            try
            {
                const string query = @"
                    SELECT COALESCE(SUM(Amount), 0) 
                    FROM Expenses 
                    WHERE DATE(Date) = DATE('now')
                ";

                var result = await _db.ExecuteScalarAsync(query);
                TodaysTotalExpenses = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                OnPropertyChanged(nameof(TodaysTotalExpenses));
                OnPropertyChanged(nameof(TodaysTotalExpensesFormatted));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading today's expenses: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadExpensesByDateRangeAsync()
        {
            try
            {
                var expenses = await _expenseService.GetExpensesByDateRangeAsync(FilterStartDate, FilterEndDate);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Expenses.Clear();
                    foreach (var expense in expenses)
                    {
                        Expenses.Add(expense);
                    }

                    FilteredTotalExpenses = expenses.Sum(e => e.Amount);
                    OnPropertyChanged(nameof(FilteredTotalExpenses));
                    OnPropertyChanged(nameof(FilteredTotalExpensesFormatted));

                    ((RelayCommand)ExportReportCommand).RaiseCanExecuteChanged();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading expenses: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddExpenseAsync()
        {
            try
            {
                var currentUser = ((App)Application.Current).CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("User not authenticated!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var expense = new Expense
                {
                    Category = SelectedCategory,
                    Amount = Amount,
                    Date = ExpenseDate,
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                    PaymentMethod = PaymentMethod,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.Now
                };

                var success = await _expenseService.AddExpenseAsync(expense);

                if (success)
                {
                    MessageBox.Show("Expense added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    await LoadTodaysTotalExpensesAsync();
                    await LoadExpensesByDateRangeAsync();
                }
                else
                {
                    MessageBox.Show("Failed to add expense.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding expense: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            SelectedCategory = string.Empty;
            Amount = 0;
            ExpenseDate = DateTime.Now;
            Note = string.Empty;
            PaymentMethod = "Cash";
        }

        private bool CanAddExpense()
        {
            return !string.IsNullOrWhiteSpace(SelectedCategory) && Amount > 0;
        }

        private void ExportReport()
        {
            try
            {
                var fileName = $"ExpenseReport_{FilterStartDate:yyyyMMdd}_to_{FilterEndDate:yyyyMMdd}.txt";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                using var writer = new System.IO.StreamWriter(filePath);
                writer.WriteLine("EXPENSE REPORT");
                writer.WriteLine($"Period: {FilterStartDate:dd/MM/yyyy} to {FilterEndDate:dd/MM/yyyy}");
                writer.WriteLine(new string('=', 80));
                writer.WriteLine();

                // Group by category
                var groupedExpenses = Expenses.GroupBy(e => e.Category)
                    .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                    .OrderByDescending(x => x.Total);

                writer.WriteLine("SUMMARY BY CATEGORY:");
                writer.WriteLine(new string('-', 80));
                foreach (var group in groupedExpenses)
                {
                    writer.WriteLine($"{group.Category,-30} {group.Count,5} transactions    Rs. {group.Total,12:N2}");
                }
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"{"TOTAL",-30} {Expenses.Count,5} transactions    Rs. {FilteredTotalExpenses,12:N2}");
                writer.WriteLine();
                writer.WriteLine();

                writer.WriteLine("DETAILED TRANSACTIONS:");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"{"Date",-12} {"Category",-20} {"Amount",12} {"Method",-15} {"Note",-20}");
                writer.WriteLine(new string('-', 80));

                foreach (var expense in Expenses.OrderBy(e => e.Date))
                {
                    writer.WriteLine($"{expense.Date:dd/MM/yyyy,-12} {expense.Category,-20} Rs. {expense.Amount,10:N2} {expense.PaymentMethod,-15} {expense.Note,-20}");
                }

                writer.WriteLine(new string('=', 80));

                MessageBox.Show($"Report exported successfully!\n\nSaved to: {filePath}", "Export Successful",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
