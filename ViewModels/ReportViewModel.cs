using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MyPOS99.Data;
using MyPOS99.Models.Reports;
using MyPOS99.Services;
using Microsoft.Win32;

namespace MyPOS99.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportService _reportService;
        private readonly ExcelExportService _excelService;
        private readonly PdfReportService _pdfService;

        private string _selectedReportType;
        private DateTime _fromDate;
        private DateTime _toDate;
        private int _selectedYear;
        private bool _isLoading;

        // Report data collections
        private ObservableCollection<DailySalesReport> _dailySalesData;
        private ObservableCollection<PurchaseReport> _purchaseData;
        private ObservableCollection<MonthlySalesReport> _monthlySalesData;
        private ObservableCollection<StockReport> _stockData;
        private ObservableCollection<LowStockReport> _lowStockData;
        private ObservableCollection<PayablesReport> _payablesData;
        private ObservableCollection<ReceivablesReport> _receivablesData;
        private ObservableCollection<SalesDetailReport> _salesDetailData;
        
        private DailyBalanceReport? _dailyBalanceData;
        private ProfitLossReport? _profitLossData;

        public ReportViewModel(DatabaseService databaseService)
        {
            _reportService = new ReportService(databaseService);
            _excelService = new ExcelExportService();
            _pdfService = new PdfReportService();

            // Initialize dates
            _fromDate = DateTime.Today;
            _toDate = DateTime.Today;
            _selectedYear = DateTime.Now.Year;
            _selectedReportType = "Daily Sales";

            // Initialize collections
            _dailySalesData = new ObservableCollection<DailySalesReport>();
            _purchaseData = new ObservableCollection<PurchaseReport>();
            _monthlySalesData = new ObservableCollection<MonthlySalesReport>();
            _stockData = new ObservableCollection<StockReport>();
            _lowStockData = new ObservableCollection<LowStockReport>();
            _payablesData = new ObservableCollection<PayablesReport>();
            _receivablesData = new ObservableCollection<ReceivablesReport>();
            _salesDetailData = new ObservableCollection<SalesDetailReport>();

            // Initialize commands
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            ExportToPdfCommand = new RelayCommand(async () => await ExportToPdfAsync());
            ExportToExcelCommand = new RelayCommand(async () => await ExportToExcelAsync());
        }

        #region Properties

        public List<string> ReportTypes => new List<string>
        {
            "Daily Sales",
            "Daily Balance",
            "Purchase Report",
            "Monthly Sales",
            "Stock Report",
            "Low Stock Report",
            "Profit & Loss",
            "Accounts Payable",
            "Accounts Receivable",
            "Sales Detail"
        };

        public List<int> AvailableYears
        {
            get
            {
                var currentYear = DateTime.Now.Year;
                return Enumerable.Range(currentYear - 5, 10).ToList();
            }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (SetProperty(ref _selectedReportType, value))
                {
                    OnPropertyChanged(nameof(ShowDateRange));
                    OnPropertyChanged(nameof(ShowSingleDate));
                    OnPropertyChanged(nameof(ShowYearSelector));
                    OnPropertyChanged(nameof(ShowNoFilters));
                }
            }
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Visibility properties
        public bool ShowDateRange => SelectedReportType == "Daily Sales" || 
                                     SelectedReportType == "Purchase Report" || 
                                     SelectedReportType == "Profit & Loss" ||
                                     SelectedReportType == "Sales Detail";

        public bool ShowSingleDate => SelectedReportType == "Daily Balance";
        
        public bool ShowYearSelector => SelectedReportType == "Monthly Sales";
        
        public bool ShowNoFilters => SelectedReportType == "Stock Report" || 
                                     SelectedReportType == "Low Stock Report" || 
                                     SelectedReportType == "Accounts Payable" || 
                                     SelectedReportType == "Accounts Receivable";

        // Data collections
        public ObservableCollection<DailySalesReport> DailySalesData
        {
            get => _dailySalesData;
            set => SetProperty(ref _dailySalesData, value);
        }

        public ObservableCollection<PurchaseReport> PurchaseData
        {
            get => _purchaseData;
            set => SetProperty(ref _purchaseData, value);
        }

        public ObservableCollection<MonthlySalesReport> MonthlySalesData
        {
            get => _monthlySalesData;
            set => SetProperty(ref _monthlySalesData, value);
        }

        public ObservableCollection<StockReport> StockData
        {
            get => _stockData;
            set => SetProperty(ref _stockData, value);
        }

        public ObservableCollection<LowStockReport> LowStockData
        {
            get => _lowStockData;
            set => SetProperty(ref _lowStockData, value);
        }

        public ObservableCollection<PayablesReport> PayablesData
        {
            get => _payablesData;
            set => SetProperty(ref _payablesData, value);
        }

        public ObservableCollection<ReceivablesReport> ReceivablesData
        {
            get => _receivablesData;
            set => SetProperty(ref _receivablesData, value);
        }

        public ObservableCollection<SalesDetailReport> SalesDetailData
        {
            get => _salesDetailData;
            set => SetProperty(ref _salesDetailData, value);
        }

        public DailyBalanceReport? DailyBalanceData
        {
            get => _dailyBalanceData;
            set => SetProperty(ref _dailyBalanceData, value);
        }

        public ProfitLossReport? ProfitLossData
        {
            get => _profitLossData;
            set => SetProperty(ref _profitLossData, value);
        }

        #endregion

        #region Commands

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToPdfCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        #endregion

        #region Methods

        private async Task GenerateReportAsync()
        {
            try
            {
                IsLoading = true;

                switch (SelectedReportType)
                {
                    case "Daily Sales":
                        var dailySales = await _reportService.GetDailySalesReportAsync(FromDate, ToDate);
                        DailySalesData.Clear();
                        foreach (var item in dailySales)
                            DailySalesData.Add(item);
                        break;

                    case "Daily Balance":
                        DailyBalanceData = await _reportService.GetDailyBalanceReportAsync(FromDate);
                        break;

                    case "Purchase Report":
                        var purchases = await _reportService.GetPurchaseReportAsync(FromDate, ToDate);
                        PurchaseData.Clear();
                        foreach (var item in purchases)
                            PurchaseData.Add(item);
                        break;

                    case "Monthly Sales":
                        var monthlySales = await _reportService.GetMonthlySalesReportAsync(SelectedYear);
                        MonthlySalesData.Clear();
                        foreach (var item in monthlySales)
                            MonthlySalesData.Add(item);
                        break;

                    case "Stock Report":
                        var stock = await _reportService.GetStockReportAsync();
                        StockData.Clear();
                        foreach (var item in stock)
                            StockData.Add(item);
                        break;

                    case "Low Stock Report":
                        var lowStock = await _reportService.GetLowStockReportAsync();
                        LowStockData.Clear();
                        foreach (var item in lowStock)
                            LowStockData.Add(item);
                        break;

                    case "Profit & Loss":
                        ProfitLossData = await _reportService.GetProfitLossReportAsync(FromDate, ToDate);
                        break;

                    case "Accounts Payable":
                        var payables = await _reportService.GetPayablesReportAsync();
                        PayablesData.Clear();
                        foreach (var item in payables)
                            PayablesData.Add(item);
                        break;

                    case "Accounts Receivable":
                        var receivables = await _reportService.GetReceivablesReportAsync();
                        ReceivablesData.Clear();
                        foreach (var item in receivables)
                            ReceivablesData.Add(item);
                        break;

                    case "Sales Detail":
                        var salesDetail = await _reportService.GetSalesDetailReportAsync(FromDate, ToDate);
                        SalesDetailData.Clear();
                        foreach (var item in salesDetail)
                            SalesDetailData.Add(item);
                        break;
                }

                MessageBox.Show("Report generated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToPdfAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    switch (SelectedReportType)
                    {
                        case "Daily Sales":
                            await _pdfService.GenerateDailySalesReportPdfAsync(DailySalesData.ToList(), dialog.FileName, FromDate, ToDate);
                            break;

                        case "Daily Balance":
                            if (DailyBalanceData != null)
                                await _pdfService.GenerateDailyBalanceReportPdfAsync(DailyBalanceData, dialog.FileName);
                            break;

                        case "Stock Report":
                            await _pdfService.GenerateStockReportPdfAsync(StockData.ToList(), dialog.FileName);
                            break;

                        case "Profit & Loss":
                            if (ProfitLossData != null)
                                await _pdfService.GenerateProfitLossReportPdfAsync(ProfitLossData, dialog.FileName);
                            break;

                        case "Accounts Payable":
                            await _pdfService.GeneratePayablesReportPdfAsync(PayablesData.ToList(), dialog.FileName);
                            break;

                        case "Accounts Receivable":
                            await _pdfService.GenerateReceivablesReportPdfAsync(ReceivablesData.ToList(), dialog.FileName);
                            break;

                        default:
                            MessageBox.Show("PDF export not available for this report type.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                    }

                    MessageBox.Show($"Report exported successfully to:\n{dialog.FileName}", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to PDF: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    switch (SelectedReportType)
                    {
                        case "Daily Sales":
                            await _excelService.ExportDailySalesReportAsync(DailySalesData.ToList(), dialog.FileName);
                            break;

                        case "Stock Report":
                            await _excelService.ExportStockReportAsync(StockData.ToList(), dialog.FileName);
                            break;

                        case "Profit & Loss":
                            if (ProfitLossData != null)
                                await _excelService.ExportProfitLossReportAsync(ProfitLossData, dialog.FileName);
                            break;

                        case "Accounts Payable":
                            await _excelService.ExportPayablesReportAsync(PayablesData.ToList(), dialog.FileName);
                            break;

                        case "Accounts Receivable":
                            await _excelService.ExportReceivablesReportAsync(ReceivablesData.ToList(), dialog.FileName);
                            break;

                        default:
                            MessageBox.Show("Excel export not available for this report type.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                    }

                    MessageBox.Show($"Report exported successfully to:\n{dialog.FileName}", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
