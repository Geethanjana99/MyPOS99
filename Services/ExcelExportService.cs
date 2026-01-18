using OfficeOpenXml;
using OfficeOpenXml.Style;
using MyPOS99.Models.Reports;
using System.Drawing;
using System.IO;

namespace MyPOS99.Services
{
    public class ExcelExportService
    {
        public ExcelExportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task ExportDailySalesReportAsync(List<DailySalesReport> data, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Daily Sales Report");

            // Header
            worksheet.Cells["A1:I1"].Merge = true;
            worksheet.Cells["A1"].Value = "Daily Sales Report";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Column headers
            var headers = new[] { "Date", "Transactions", "Total Sales", "Discount", "Net Sales",
                "Cash Sales", "Card Sales", "Mobile Sales", "Credit Sales" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Data
            int row = 4;
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.Date.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = item.TotalTransactions;
                worksheet.Cells[row, 3].Value = item.TotalSales;
                worksheet.Cells[row, 4].Value = item.TotalDiscount;
                worksheet.Cells[row, 5].Value = item.NetSales;
                worksheet.Cells[row, 6].Value = item.CashSales;
                worksheet.Cells[row, 7].Value = item.CardSales;
                worksheet.Cells[row, 8].Value = item.MobileSales;
                worksheet.Cells[row, 9].Value = item.CreditSales;

                // Format currency
                for (int col = 3; col <= 9; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "?#,##0.00";
                }

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        public async Task ExportStockReportAsync(List<StockReport> data, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stock Report");

            // Header
            worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1"].Value = "Stock Report - " + DateTime.Now.ToString("dd/MM/yyyy");
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Column headers
            var headers = new[] { "Code", "Product Name", "Category", "Supplier", "Stock Qty",
                "Min Level", "Cost Price", "Sell Price", "Stock Value", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Data
            int row = 4;
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.ProductCode;
                worksheet.Cells[row, 2].Value = item.ProductName;
                worksheet.Cells[row, 3].Value = item.Category;
                worksheet.Cells[row, 4].Value = item.SupplierName;
                worksheet.Cells[row, 5].Value = item.StockQty;
                worksheet.Cells[row, 6].Value = item.MinStockLevel;
                worksheet.Cells[row, 7].Value = item.CostPrice;
                worksheet.Cells[row, 8].Value = item.SellPrice;
                worksheet.Cells[row, 9].Value = item.StockValue;
                worksheet.Cells[row, 10].Value = item.IsLowStock ? "LOW STOCK" : "OK";

                // Format currency
                worksheet.Cells[row, 7].Style.Numberformat.Format = "?#,##0.00";
                worksheet.Cells[row, 8].Style.Numberformat.Format = "?#,##0.00";
                worksheet.Cells[row, 9].Style.Numberformat.Format = "?#,##0.00";

                // Highlight low stock
                if (item.IsLowStock)
                {
                    worksheet.Cells[row, 1, row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                }

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        public async Task ExportProfitLossReportAsync(ProfitLossReport data, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Profit & Loss");

            // Title
            worksheet.Cells["A1:C1"].Merge = true;
            worksheet.Cells["A1"].Value = "Profit & Loss Statement";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            worksheet.Cells["A2:C2"].Merge = true;
            worksheet.Cells["A2"].Value = $"{data.StartDate:dd/MM/yyyy} to {data.EndDate:dd/MM/yyyy}";

            int row = 4;

            // Revenue section
            worksheet.Cells[row, 1].Value = "REVENUE";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            worksheet.Cells[row, 1].Value = "Total Revenue";
            worksheet.Cells[row, 2].Value = data.TotalRevenue;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row++;

            worksheet.Cells[row, 1].Value = "Less: Discounts";
            worksheet.Cells[row, 2].Value = data.Discounts;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row++;

            worksheet.Cells[row, 1].Value = "Net Revenue";
            worksheet.Cells[row, 2].Value = data.NetRevenue;
            worksheet.Cells[row, 2].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row += 2;

            // COGS section
            worksheet.Cells[row, 1].Value = "Cost of Goods Sold";
            worksheet.Cells[row, 2].Value = data.CostOfGoodsSold;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row += 2;

            // Gross Profit
            worksheet.Cells[row, 1].Value = "GROSS PROFIT";
            worksheet.Cells[row, 2].Value = data.GrossProfit;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row++;

            worksheet.Cells[row, 1].Value = "Gross Profit Margin";
            worksheet.Cells[row, 2].Value = data.GrossProfitMargin / 100;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
            row += 2;

            // Expenses
            worksheet.Cells[row, 1].Value = "Operating Expenses";
            worksheet.Cells[row, 2].Value = data.TotalExpenses;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            row += 2;

            // Net Profit
            worksheet.Cells[row, 1].Value = "NET PROFIT";
            worksheet.Cells[row, 2].Value = data.NetProfit;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "?#,##0.00";
            worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            row++;

            worksheet.Cells[row, 1].Value = "Net Profit Margin";
            worksheet.Cells[row, 2].Value = data.NetProfitMargin / 100;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        public async Task ExportPayablesReportAsync(List<PayablesReport> data, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Payables");

            worksheet.Cells["A1:G1"].Merge = true;
            worksheet.Cells["A1"].Value = "Accounts Payable Report";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            var headers = new[] { "Supplier", "Purchases", "Total Amount", "Paid", "Due", "Oldest Due", "Overdue Days" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 4;
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.SupplierName;
                worksheet.Cells[row, 2].Value = item.TotalPurchases;
                worksheet.Cells[row, 3].Value = item.TotalPurchaseAmount;
                worksheet.Cells[row, 4].Value = item.TotalPaid;
                worksheet.Cells[row, 5].Value = item.TotalDue;
                worksheet.Cells[row, 6].Value = item.OldestDueDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 7].Value = item.OverdueDays;

                for (int col = 3; col <= 5; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "?#,##0.00";
                }

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        public async Task ExportReceivablesReportAsync(List<ReceivablesReport> data, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Receivables");

            worksheet.Cells["A1:G1"].Merge = true;
            worksheet.Cells["A1"].Value = "Accounts Receivable Report";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            var headers = new[] { "Customer", "Phone", "Credit Limit", "Current Credit", "Available", "Total Purchases", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 4;
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.CustomerName;
                worksheet.Cells[row, 2].Value = item.Phone;
                worksheet.Cells[row, 3].Value = item.CreditLimit;
                worksheet.Cells[row, 4].Value = item.CurrentCredit;
                worksheet.Cells[row, 5].Value = item.AvailableCredit;
                worksheet.Cells[row, 6].Value = item.TotalPurchases;
                worksheet.Cells[row, 7].Value = item.IsOverLimit ? "OVERLIMIT" : "OK";

                for (int col = 3; col <= 6; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "?#,##0.00";
                }

                if (item.IsOverLimit)
                {
                    worksheet.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                }

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }
    }
}
