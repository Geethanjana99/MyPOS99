using MyPOS99.Data;
using MyPOS99.Models.Reports;

namespace MyPOS99.Services
{
    public class ReportService
    {
        private readonly DatabaseService _db;

        public ReportService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        #region Daily Sales Report

        public async Task<List<DailySalesReport>> GetDailySalesReportAsync(DateTime fromDate, DateTime toDate)
        {
            const string query = @"
                SELECT 
                    DATE(Date) as ReportDate,
                    COUNT(*) as TotalTransactions,
                    SUM(SubTotal) as TotalSales,
                    SUM(Discount) as TotalDiscount,
                    SUM(Total) as NetSales,
                    SUM(CASE WHEN PaymentType = 'Cash' THEN Total ELSE 0 END) as CashSales,
                    SUM(CASE WHEN PaymentType = 'Card' THEN Total ELSE 0 END) as CardSales,
                    SUM(CASE WHEN PaymentType = 'Mobile' THEN Total ELSE 0 END) as MobileSales,
                    SUM(CASE WHEN PaymentType = 'Credit' THEN Total ELSE 0 END) as CreditSales
                FROM Sales
                WHERE DATE(Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                GROUP BY DATE(Date)
                ORDER BY DATE(Date) DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new DailySalesReport
            {
                Date = DateTime.Parse(reader.GetString(0)),
                TotalTransactions = reader.GetInt32(1),
                TotalSales = (decimal)reader.GetDouble(2),
                TotalDiscount = (decimal)reader.GetDouble(3),
                NetSales = (decimal)reader.GetDouble(4),
                CashSales = (decimal)reader.GetDouble(5),
                CardSales = (decimal)reader.GetDouble(6),
                MobileSales = (decimal)reader.GetDouble(7),
                CreditSales = (decimal)reader.GetDouble(8)
            },
            DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
            DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));
        }

        #endregion

        #region Daily Balance Report

        public async Task<DailyBalanceReport> GetDailyBalanceReportAsync(DateTime date)
        {
            const string salesQuery = @"
                SELECT 
                    COALESCE(SUM(CASE WHEN PaymentType = 'Cash' THEN Total ELSE 0 END), 0) as CashSales,
                    COALESCE(SUM(CASE WHEN PaymentType = 'Card' THEN Total ELSE 0 END), 0) as CardSales,
                    COALESCE(SUM(CASE WHEN PaymentType = 'Mobile' THEN Total ELSE 0 END), 0) as MobileSales,
                    COALESCE(SUM(CASE WHEN PaymentType = 'Credit' THEN Total ELSE 0 END), 0) as CreditSales,
                    COALESCE(SUM(Total), 0) as TotalSales
                FROM Sales
                WHERE DATE(Date) = DATE(@date)
            ";

            const string expensesQuery = @"
                SELECT COALESCE(SUM(Amount), 0) as TotalExpenses
                FROM Expenses
                WHERE DATE(Date) = DATE(@date)
            ";

            var report = new DailyBalanceReport { Date = date };

            var salesResult = await _db.ExecuteQuerySingleAsync(salesQuery, reader => new
            {
                CashSales = (decimal)reader.GetDouble(0),
                CardSales = (decimal)reader.GetDouble(1),
                MobileSales = (decimal)reader.GetDouble(2),
                CreditSales = (decimal)reader.GetDouble(3),
                TotalSales = (decimal)reader.GetDouble(4)
            }, DatabaseService.CreateParameter("@date", date.ToString("yyyy-MM-dd")));

            var expensesResult = await _db.ExecuteScalarAsync(expensesQuery,
                DatabaseService.CreateParameter("@date", date.ToString("yyyy-MM-dd")));

            if (salesResult != null)
            {
                report.CashSales = salesResult.CashSales;
                report.CardSales = salesResult.CardSales;
                report.MobileSales = salesResult.MobileSales;
                report.CreditSales = salesResult.CreditSales;
                report.TotalSales = salesResult.TotalSales;
            }

            report.TotalExpenses = expensesResult != DBNull.Value ? Convert.ToDecimal(expensesResult) : 0;
            report.OpeningBalance = 0; // Can be set from previous day closing or manual entry
            report.ExpectedCashInHand = report.CashSales - report.TotalExpenses;
            report.ClosingBalance = report.ExpectedCashInHand; // Actual cash count
            report.CashVariance = 0; // Difference between expected and actual

            return report;
        }

        #endregion

        #region Purchase Reports

        public async Task<List<PurchaseReport>> GetPurchaseReportAsync(DateTime fromDate, DateTime toDate)
        {
            const string query = @"
                SELECT 
                    p.PurchaseNumber,
                    p.Date,
                    s.Name as SupplierName,
                    p.SubTotal,
                    p.Tax,
                    p.Total,
                    p.PaymentStatus,
                    p.AmountPaid,
                    (p.Total - p.AmountPaid) as AmountDue
                FROM Purchases p
                INNER JOIN Suppliers s ON p.SupplierId = s.Id
                WHERE DATE(p.Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                ORDER BY p.Date DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new PurchaseReport
            {
                PurchaseNumber = reader.GetString(0),
                Date = DateTime.Parse(reader.GetString(1)),
                SupplierName = reader.GetString(2),
                SubTotal = (decimal)reader.GetDouble(3),
                Tax = (decimal)reader.GetDouble(4),
                Total = (decimal)reader.GetDouble(5),
                PaymentStatus = reader.GetString(6),
                AmountPaid = (decimal)reader.GetDouble(7),
                AmountDue = (decimal)reader.GetDouble(8)
            },
            DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
            DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));
        }

        #endregion

        #region Monthly Sales Report

        public async Task<List<MonthlySalesReport>> GetMonthlySalesReportAsync(int year)
        {
            const string query = @"
                SELECT 
                    strftime('%Y', Date) as Year,
                    strftime('%m', Date) as Month,
                    COUNT(*) as TotalTransactions,
                    SUM(SubTotal) as TotalSales
                FROM Sales
                WHERE strftime('%Y', Date) = @year
                GROUP BY strftime('%Y', Date), strftime('%m', Date)
                ORDER BY Month
            ";

            const string expensesQuery = @"
                SELECT 
                    strftime('%m', Date) as Month,
                    SUM(Amount) as TotalExpenses
                FROM Expenses
                WHERE strftime('%Y', Date) = @year
                GROUP BY strftime('%m', Date)
            ";

            var salesData = await _db.ExecuteQueryAsync(query, reader => new
            {
                Year = int.Parse(reader.GetString(0)),
                Month = int.Parse(reader.GetString(1)),
                TotalTransactions = reader.GetInt32(2),
                TotalSales = (decimal)reader.GetDouble(3)
            }, DatabaseService.CreateParameter("@year", year.ToString()));

            var expensesData = await _db.ExecuteQueryAsync(expensesQuery, reader => new
            {
                Month = int.Parse(reader.GetString(0)),
                Expenses = (decimal)reader.GetDouble(1)
            }, DatabaseService.CreateParameter("@year", year.ToString()));

            var reports = new List<MonthlySalesReport>();
            foreach (var sale in salesData)
            {
                var expense = expensesData.FirstOrDefault(e => e.Month == sale.Month);
                var totalExpenses = expense?.Expenses ?? 0;

                reports.Add(new MonthlySalesReport
                {
                    Year = sale.Year,
                    Month = sale.Month,
                    MonthName = new DateTime(sale.Year, sale.Month, 1).ToString("MMMM"),
                    TotalTransactions = sale.TotalTransactions,
                    TotalSales = sale.TotalSales,
                    TotalCost = 0, // Would need cost calculation from sale items
                    GrossProfit = sale.TotalSales, // Simplified
                    Expenses = totalExpenses,
                    NetProfit = sale.TotalSales - totalExpenses
                });
            }

            return reports;
        }

        #endregion

        #region Stock Reports

        public async Task<List<StockReport>> GetStockReportAsync()
        {
            const string query = @"
                SELECT 
                    p.Code,
                    p.Name,
                    p.Category,
                    s.Name as SupplierName,
                    p.StockQty,
                    p.MinStockLevel,
                    p.CostPrice,
                    p.SellPrice,
                    (p.StockQty * p.CostPrice) as StockValue,
                    CASE WHEN p.StockQty <= p.MinStockLevel THEN 1 ELSE 0 END as IsLowStock
                FROM Products p
                LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                ORDER BY p.Name
            ";

            return await _db.ExecuteQueryAsync(query, reader => new StockReport
            {
                ProductCode = reader.GetString(0),
                ProductName = reader.GetString(1),
                Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                StockQty = reader.GetInt32(4),
                MinStockLevel = reader.GetInt32(5),
                CostPrice = (decimal)reader.GetDouble(6),
                SellPrice = (decimal)reader.GetDouble(7),
                StockValue = (decimal)reader.GetDouble(8),
                IsLowStock = reader.GetInt32(9) == 1
            });
        }

        public async Task<List<LowStockReport>> GetLowStockReportAsync()
        {
            const string query = @"
                SELECT 
                    p.Code,
                    p.Name,
                    p.Category,
                    s.Name as SupplierName,
                    p.StockQty,
                    p.MinStockLevel,
                    (p.MinStockLevel - p.StockQty) as Deficit,
                    p.CostPrice,
                    ((p.MinStockLevel - p.StockQty) * p.CostPrice) as ReorderValue
                FROM Products p
                LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                WHERE p.StockQty <= p.MinStockLevel
                ORDER BY Deficit DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new LowStockReport
            {
                ProductCode = reader.GetString(0),
                ProductName = reader.GetString(1),
                Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                CurrentStock = reader.GetInt32(4),
                MinStockLevel = reader.GetInt32(5),
                Deficit = reader.GetInt32(6),
                CostPrice = (decimal)reader.GetDouble(7),
                ReorderValue = (decimal)reader.GetDouble(8)
            });
        }

        #endregion

        #region Profit & Loss Report

        public async Task<ProfitLossReport> GetProfitLossReportAsync(DateTime fromDate, DateTime toDate)
        {
            // Revenue
            const string revenueQuery = @"
                SELECT 
                    COALESCE(SUM(SubTotal), 0) as TotalRevenue,
                    COALESCE(SUM(Discount), 0) as TotalDiscount,
                    COALESCE(SUM(Total), 0) as NetRevenue
                FROM Sales
                WHERE DATE(Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
            ";

            // Cost of Goods Sold (from sale items)
            const string cogsQuery = @"
                SELECT COALESCE(SUM(si.Qty * p.CostPrice), 0) as COGS
                FROM SaleItems si
                INNER JOIN Sales s ON si.SaleId = s.Id
                INNER JOIN Products p ON si.ProductId = p.Id
                WHERE DATE(s.Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
            ";

            // Expenses
            const string expensesQuery = @"
                SELECT COALESCE(SUM(Amount), 0) as TotalExpenses
                FROM Expenses
                WHERE DATE(Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
            ";

            var report = new ProfitLossReport
            {
                StartDate = fromDate,
                EndDate = toDate
            };

            var revenueResult = await _db.ExecuteQuerySingleAsync(revenueQuery, reader => new
            {
                TotalRevenue = (decimal)reader.GetDouble(0),
                TotalDiscount = (decimal)reader.GetDouble(1),
                NetRevenue = (decimal)reader.GetDouble(2)
            },
            DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
            DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));

            var cogsResult = await _db.ExecuteScalarAsync(cogsQuery,
                DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
                DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));

            var expensesResult = await _db.ExecuteScalarAsync(expensesQuery,
                DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
                DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));

            if (revenueResult != null)
            {
                report.TotalRevenue = revenueResult.TotalRevenue;
                report.Discounts = revenueResult.TotalDiscount;
                report.NetRevenue = revenueResult.NetRevenue;
            }

            report.CostOfGoodsSold = cogsResult != DBNull.Value ? Convert.ToDecimal(cogsResult) : 0;
            report.GrossProfit = report.NetRevenue - report.CostOfGoodsSold;
            report.GrossProfitMargin = report.NetRevenue > 0 ? (report.GrossProfit / report.NetRevenue) * 100 : 0;

            report.TotalExpenses = expensesResult != DBNull.Value ? Convert.ToDecimal(expensesResult) : 0;
            report.NetProfit = report.GrossProfit - report.TotalExpenses;
            report.NetProfitMargin = report.NetRevenue > 0 ? (report.NetProfit / report.NetRevenue) * 100 : 0;

            return report;
        }

        #endregion

        #region Payables & Receivables

        public async Task<List<PayablesReport>> GetPayablesReportAsync()
        {
            const string query = @"
                SELECT 
                    s.Name as SupplierName,
                    COUNT(p.Id) as TotalPurchases,
                    COALESCE(SUM(p.Total), 0) as TotalPurchaseAmount,
                    COALESCE(SUM(p.AmountPaid), 0) as TotalPaid,
                    COALESCE(SUM(p.Total - p.AmountPaid), 0) as TotalDue,
                    MIN(CASE WHEN p.PaymentStatus != 'Paid' THEN p.Date END) as OldestDueDate
                FROM Suppliers s
                LEFT JOIN Purchases p ON s.Id = p.SupplierId
                GROUP BY s.Id, s.Name
                HAVING TotalDue > 0
                ORDER BY TotalDue DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader =>
            {
                var report = new PayablesReport
                {
                    SupplierName = reader.GetString(0),
                    TotalPurchases = reader.GetInt32(1),
                    TotalPurchaseAmount = (decimal)reader.GetDouble(2),
                    TotalPaid = (decimal)reader.GetDouble(3),
                    TotalDue = (decimal)reader.GetDouble(4),
                    OldestDueDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))
                };

                if (report.OldestDueDate.HasValue)
                {
                    report.OverdueDays = (DateTime.Now - report.OldestDueDate.Value).Days;
                }

                return report;
            });
        }

        public async Task<List<ReceivablesReport>> GetReceivablesReportAsync()
        {
            const string query = @"
                SELECT 
                    c.Name,
                    c.Phone,
                    c.CreditLimit,
                    c.CurrentCredit,
                    (c.CreditLimit - c.CurrentCredit) as AvailableCredit,
                    c.TotalPurchases,
                    CASE WHEN c.CurrentCredit > c.CreditLimit THEN 1 ELSE 0 END as IsOverLimit
                FROM Customers c
                WHERE c.IsCreditCustomer = 1 AND c.CurrentCredit > 0
                ORDER BY c.CurrentCredit DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new ReceivablesReport
            {
                CustomerName = reader.GetString(0),
                Phone = reader.IsDBNull(1) ? null : reader.GetString(1),
                CreditLimit = (decimal)reader.GetDouble(2),
                CurrentCredit = (decimal)reader.GetDouble(3),
                AvailableCredit = (decimal)reader.GetDouble(4),
                TotalPurchases = (decimal)reader.GetDouble(5),
                IsOverLimit = reader.GetInt32(6) == 1
            });
        }

        #endregion

        #region Sales Detail Report

        public async Task<List<SalesDetailReport>> GetSalesDetailReportAsync(DateTime fromDate, DateTime toDate)
        {
            const string query = @"
                SELECT 
                    s.InvoiceNumber,
                    s.Date,
                    c.Name as CustomerName,
                    s.PaymentType,
                    s.SubTotal,
                    s.Discount,
                    s.Total,
                    u.Username as Cashier
                FROM Sales s
                LEFT JOIN Customers c ON s.CustomerId = c.Id
                INNER JOIN Users u ON s.UserId = u.Id
                WHERE DATE(s.Date) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                ORDER BY s.Date DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new SalesDetailReport
            {
                InvoiceNumber = reader.GetString(0),
                Date = DateTime.Parse(reader.GetString(1)),
                CustomerName = reader.IsDBNull(2) ? "Walk-in" : reader.GetString(2),
                PaymentType = reader.GetString(3),
                SubTotal = (decimal)reader.GetDouble(4),
                Discount = (decimal)reader.GetDouble(5),
                Total = (decimal)reader.GetDouble(6),
                Cashier = reader.GetString(7)
            },
            DatabaseService.CreateParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
            DatabaseService.CreateParameter("@toDate", toDate.ToString("yyyy-MM-dd")));
        }

        #endregion
    }
}
