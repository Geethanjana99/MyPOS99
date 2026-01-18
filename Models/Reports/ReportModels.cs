namespace MyPOS99.Models.Reports
{
    // Daily Sales Report
    public class DailySalesReport
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetSales { get; set; }
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal MobileSales { get; set; }
        public decimal CreditSales { get; set; }
    }

    // Daily Report (Cash Balancing)
    public class DailyBalanceReport
    {
        public DateTime Date { get; set; }
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal MobileSales { get; set; }
        public decimal CreditSales { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ExpectedCashInHand { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal CashVariance { get; set; }
    }

    // Purchase Report
    public class PurchaseReport
    {
        public string PurchaseNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
    }

    // Monthly Sales Report
    public class MonthlySalesReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalTransactions { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit { get; set; }
    }

    // Stock Report
    public class StockReport
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SupplierName { get; set; }
        public int StockQty { get; set; }
        public int MinStockLevel { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal StockValue { get; set; }
        public bool IsLowStock { get; set; }
    }

    // Low Stock Report
    public class LowStockReport
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SupplierName { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int Deficit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal ReorderValue { get; set; }
    }

    // Profit & Loss Report
    public class ProfitLossReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Revenue
        public decimal TotalRevenue { get; set; }
        public decimal Discounts { get; set; }
        public decimal NetRevenue { get; set; }
        
        // Cost of Goods Sold
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        
        // Operating Expenses
        public decimal TotalExpenses { get; set; }
        
        // Net Profit
        public decimal NetProfit { get; set; }
        public decimal NetProfitMargin { get; set; }
    }

    // Payables Report (Amount owed to suppliers)
    public class PayablesReport
    {
        public string SupplierName { get; set; } = string.Empty;
        public int TotalPurchases { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public DateTime? OldestDueDate { get; set; }
        public int OverdueDays { get; set; }
    }

    // Receivables Report (Amount owed by customers)
    public class ReceivablesReport
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentCredit { get; set; }
        public decimal AvailableCredit { get; set; }
        public decimal TotalPurchases { get; set; }
        public bool IsOverLimit { get; set; }
    }

    // Sales Detail Report Item
    public class SalesDetailReport
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? CustomerName { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string Cashier { get; set; } = string.Empty;
    }
}
