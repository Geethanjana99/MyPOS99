using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MyPOS99.Models.Reports;

namespace MyPOS99.Services
{
    public class PdfReportService
    {
        public PdfReportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Daily Sales Report

        public async Task GenerateDailySalesReportPdfAsync(List<DailySalesReport> data, string filePath, DateTime fromDate, DateTime toDate)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);

                        // Header
                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("DAILY SALES REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text($"{fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        // Content
                        page.Content().PaddingTop(20).Column(column =>
                        {
                            // Summary Cards
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Blue.Lighten4).Padding(15).Column(c =>
                                {
                                    c.Item().Text("Total Transactions").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(data.Sum(x => x.TotalTransactions).ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Green.Lighten4).Padding(15).Column(c =>
                                {
                                    c.Item().Text("Total Sales").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text($"?{data.Sum(x => x.TotalSales):N2}").FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Orange.Lighten4).Padding(15).Column(c =>
                                {
                                    c.Item().Text("Net Sales").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text($"?{data.Sum(x => x.NetSales):N2}").FontSize(20).Bold().FontColor(Colors.Orange.Darken2);
                                });
                            });

                            column.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(60);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Header Row
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Date").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Trans").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Total Sales").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Discount").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Net Sales").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Cash").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Card").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Mobile").FontSize(11).Bold().FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Credit").FontSize(11).Bold().FontColor(Colors.White);
                                });

                                // Data Rows
                                foreach (var item in data)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(item.Date.ToString("dd/MM/yyyy")).FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text(item.TotalTransactions.ToString()).FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.TotalSales:N2}").FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.TotalDiscount:N2}").FontSize(10).FontColor(Colors.Red.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.NetSales:N2}").FontSize(10).Bold();
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.CashSales:N2}").FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.CardSales:N2}").FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.MobileSales:N2}").FontSize(10);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.CreditSales:N2}").FontSize(10);
                                }

                                // Total Row
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("TOTAL").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("").FontSize(11);
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.TotalSales):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.TotalDiscount):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.NetSales):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.CashSales):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.CardSales):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.MobileSales):N2}").FontSize(11).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.CreditSales):N2}").FontSize(11).Bold();
                            });
                        });

                        // Footer
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Generated on ").FontSize(9);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).Bold();
                            text.Span(" | Page ").FontSize(9);
                            text.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(filePath);
            });
        }

        #endregion

        #region Daily Balance Report

        public async Task GenerateDailyBalanceReportPdfAsync(DailyBalanceReport data, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("DAILY BALANCE REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text($"Date: {data.Date:dd/MM/yyyy}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        page.Content().PaddingTop(20).Column(column =>
                        {
                            // Sales Breakdown Section
                            column.Item().Background(Colors.Blue.Lighten4).Padding(15).Text("SALES BREAKDOWN").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                            
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Padding(10).Text("Cash Sales").FontSize(14);
                                table.Cell().Padding(10).AlignRight().Text($"?{data.CashSales:N2}").FontSize(14).Bold();

                                table.Cell().Background(Colors.Grey.Lighten4).Padding(10).Text("Card Sales").FontSize(14);
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(10).AlignRight().Text($"?{data.CardSales:N2}").FontSize(14).Bold();

                                table.Cell().Padding(10).Text("Mobile Sales").FontSize(14);
                                table.Cell().Padding(10).AlignRight().Text($"?{data.MobileSales:N2}").FontSize(14).Bold();

                                table.Cell().Background(Colors.Grey.Lighten4).Padding(10).Text("Credit Sales").FontSize(14);
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(10).AlignRight().Text($"?{data.CreditSales:N2}").FontSize(14).Bold();

                                table.Cell().Background(Colors.Green.Lighten4).Border(2).BorderColor(Colors.Green.Medium).Padding(12).Text("TOTAL SALES").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                                table.Cell().Background(Colors.Green.Lighten4).Border(2).BorderColor(Colors.Green.Medium).Padding(12).AlignRight().Text($"?{data.TotalSales:N2}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                            });

                            // Expenses Section
                            column.Item().PaddingTop(20).Background(Colors.Red.Lighten4).Padding(15).Text("EXPENSES").FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                            
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Background(Colors.Red.Lighten4).Border(2).BorderColor(Colors.Red.Medium).Padding(12).Text("Total Expenses").FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                                table.Cell().Background(Colors.Red.Lighten4).Border(2).BorderColor(Colors.Red.Medium).Padding(12).AlignRight().Text($"?{data.TotalExpenses:N2}").FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                            });

                            // Cash Balance Section
                            column.Item().PaddingTop(20).Background(Colors.Blue.Darken2).Padding(20).Column(c =>
                            {
                                c.Item().Text("EXPECTED CASH IN HAND").FontSize(18).Bold().FontColor(Colors.White);
                                c.Item().PaddingTop(5).Text("(Cash Sales - Expenses)").FontSize(12).FontColor(Colors.Grey.Lighten3);
                                c.Item().PaddingTop(10).AlignRight().Text($"?{data.ExpectedCashInHand:N2}").FontSize(28).Bold().FontColor(Colors.White);
                            });

                            column.Item().PaddingTop(20).Border(2).BorderColor(Colors.Orange.Medium).Background(Colors.Orange.Lighten4).Padding(15).Column(c =>
                            {
                                c.Item().Text("?? CASH BALANCING INSTRUCTIONS").FontSize(14).Bold().FontColor(Colors.Orange.Darken2);
                                c.Item().PaddingTop(10).Text("1. Count actual cash in register").FontSize(11);
                                c.Item().Text("2. Compare with Expected Cash in Hand above").FontSize(11);
                                c.Item().Text("3. Record any variance and investigate discrepancies").FontSize(11);
                                c.Item().Text("4. Prepare bank deposit if needed").FontSize(11);
                            });
                        });

                        page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                    });
                }).GeneratePdf(filePath);
            });
        }

        #endregion

        #region Stock Report

        public async Task GenerateStockReportPdfAsync(List<StockReport> data, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(30);
                        page.PageColor(Colors.White);

                        page.Header().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("STOCK REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                                    c.Item().Text($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(12).FontColor(Colors.Grey.Darken1);
                                });

                                row.ConstantItem(150).AlignRight().Column(c =>
                                {
                                    c.Item().Text("Total Items").FontSize(11).FontColor(Colors.Grey.Darken1);
                                    c.Item().Text(data.Count.ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                });

                                row.ConstantItem(10);

                                row.ConstantItem(150).AlignRight().Column(c =>
                                {
                                    c.Item().Text("Total Value").FontSize(11).FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"?{data.Sum(x => x.StockValue):N2}").FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                                });
                            });

                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        page.Content().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Code").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Product").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Category").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Supplier").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Stock").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Min").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Cost").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Sell").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Value").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Status").FontSize(10).Bold().FontColor(Colors.White);
                            });

                            foreach (var item in data)
                            {
                                var bgColor = item.IsLowStock ? Colors.Red.Lighten4 : Colors.White;
                                var statusColor = item.IsLowStock ? Colors.Red.Darken2 : Colors.Green.Darken2;
                                var statusText = item.IsLowStock ? "LOW" : "OK";

                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.ProductCode).FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.ProductName).FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.Category ?? "").FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.SupplierName ?? "").FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(item.StockQty.ToString()).FontSize(9).Bold();
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(item.MinStockLevel.ToString()).FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"?{item.CostPrice:N2}").FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"?{item.SellPrice:N2}").FontSize(9);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"?{item.StockValue:N2}").FontSize(9).Bold();
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(statusText).FontSize(9).Bold().FontColor(statusColor);
                            }

                            // Total Row
                            for (int i = 0; i < 8; i++) table.Cell().Background(Colors.Blue.Lighten4).Padding(8).Text(i == 0 ? "TOTAL STOCK VALUE" : "").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            table.Cell().Background(Colors.Blue.Lighten4).Padding(8).AlignRight().Text($"?{data.Sum(x => x.StockValue):N2}").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            table.Cell().Background(Colors.Blue.Lighten4).Padding(8).Text("").FontSize(11);
                        });

                        page.Footer().AlignCenter().Column(c =>
                        {
                            c.Item().Text(text =>
                            {
                                text.Span("Page ").FontSize(9);
                                text.CurrentPageNumber().FontSize(9);
                                text.Span(" of ").FontSize(9);
                                text.TotalPages().FontSize(9);
                            });
                        });
                    });
                }).GeneratePdf(filePath);
            });
        }

        #endregion

        #region Profit & Loss Report

        public async Task GenerateProfitLossReportPdfAsync(ProfitLossReport data, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("PROFIT & LOSS STATEMENT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text($"{data.StartDate:dd/MM/yyyy} to {data.EndDate:dd/MM/yyyy}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        page.Content().PaddingTop(20).Column(column =>
                        {
                            // Revenue Section
                            column.Item().Background(Colors.Green.Lighten4).Padding(12).Text("REVENUE").FontSize(16).Bold().FontColor(Colors.Green.Darken2);

                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Padding(8).Text("Total Revenue").FontSize(14);
                                table.Cell().Padding(8).AlignRight().Text($"?{data.TotalRevenue:N2}").FontSize(14).Bold();

                                table.Cell().Background(Colors.Grey.Lighten4).Padding(8).Text("Less: Discounts").FontSize(14).FontColor(Colors.Red.Medium);
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(8).AlignRight().Text($"?{data.Discounts:N2}").FontSize(14).Bold().FontColor(Colors.Red.Medium);

                                table.Cell().Background(Colors.Green.Lighten3).Border(2).BorderColor(Colors.Green.Medium).Padding(10).Text("Net Revenue").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                                table.Cell().Background(Colors.Green.Lighten3).Border(2).BorderColor(Colors.Green.Medium).Padding(10).AlignRight().Text($"?{data.NetRevenue:N2}").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                            });

                            // COGS Section
                            column.Item().PaddingTop(20).Background(Colors.Red.Lighten4).Padding(12).Text("COST OF GOODS SOLD").FontSize(16).Bold().FontColor(Colors.Red.Darken2);

                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Background(Colors.Red.Lighten4).Border(2).BorderColor(Colors.Red.Medium).Padding(10).Text("COGS").FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                                table.Cell().Background(Colors.Red.Lighten4).Border(2).BorderColor(Colors.Red.Medium).Padding(10).AlignRight().Text($"?{data.CostOfGoodsSold:N2}").FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                            });

                            // Gross Profit
                            column.Item().PaddingTop(20).Background(Colors.Green.Darken1).Padding(15).Column(c =>
                            {
                                c.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("GROSS PROFIT").FontSize(20).Bold().FontColor(Colors.White);
                                        col.Item().Text($"Margin: {data.GrossProfitMargin:F2}%").FontSize(13).FontColor(Colors.Grey.Lighten3);
                                    });
                                    row.ConstantItem(150).AlignRight().Text($"?{data.GrossProfit:N2}").FontSize(24).Bold().FontColor(Colors.White);
                                });
                            });

                            // Operating Expenses
                            column.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Padding(10).Text("Operating Expenses").FontSize(14).FontColor(Colors.Red.Medium);
                                table.Cell().Padding(10).AlignRight().Text($"?{data.TotalExpenses:N2}").FontSize(14).Bold().FontColor(Colors.Red.Medium);
                            });

                            // Net Profit
                            var profitColor = data.NetProfit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                            var profitBgColor = data.NetProfit >= 0 ? Colors.Green.Lighten4 : Colors.Red.Lighten4;

                            column.Item().PaddingTop(20).Background(Colors.Blue.Darken2).Padding(20).Column(c =>
                            {
                                c.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("NET PROFIT").FontSize(22).Bold().FontColor(Colors.White);
                                        col.Item().Text($"Net Margin: {data.NetProfitMargin:F2}%").FontSize(14).FontColor(Colors.Grey.Lighten3);
                                    });
                                    row.ConstantItem(150).AlignRight().Text($"?{data.NetProfit:N2}").FontSize(28).Bold().FontColor(Colors.White);
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                    });
                }).GeneratePdf(filePath);
            });
        }

        #endregion

        #region Payables Report

        public async Task GeneratePayablesReportPdfAsync(List<PayablesReport> data, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("ACCOUNTS PAYABLE REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text($"Generated: {DateTime.Now:dd/MM/yyyy}").FontSize(12).FontColor(Colors.Grey.Darken1);
                            
                            column.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Red.Lighten4).Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Due").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text($"?{data.Sum(x => x.TotalDue):N2}").FontSize(20).Bold().FontColor(Colors.Red.Darken2);
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background(Colors.Blue.Lighten4).Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Suppliers").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(data.Count.ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                });
                            });

                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        page.Content().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Supplier").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Orders").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Total").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Paid").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Due").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Overdue").FontSize(11).Bold().FontColor(Colors.White);
                            });

                            foreach (var item in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(item.SupplierName).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text(item.TotalPurchases.ToString()).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.TotalPurchaseAmount:N2}").FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.TotalPaid:N2}").FontSize(10).FontColor(Colors.Green.Medium);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.TotalDue:N2}").FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text($"{item.OverdueDays} days").FontSize(10);
                            }

                            // Total Row
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("TOTAL").FontSize(11).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("").FontSize(11);
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.TotalPurchaseAmount):N2}").FontSize(11).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.TotalPaid):N2}").FontSize(11).Bold();
                            table.Cell().Background(Colors.Red.Lighten4).Border(2).BorderColor(Colors.Red.Medium).Padding(8).AlignRight().Text($"?{data.Sum(x => x.TotalDue):N2}").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                            table.Cell().Background(Colors.Red.Lighten4).Padding(8).Text("").FontSize(11);
                        });

                                        page.Footer().AlignCenter().Column(c =>
                                        {
                                            c.Item().Text(text =>
                                            {
                                                text.Span("Page ").FontSize(9);
                                                text.CurrentPageNumber().FontSize(9);
                                            });
                                        });
                                    });
                                }).GeneratePdf(filePath);
                            });
                        }

                        #endregion

                        #region Receivables Report

        public async Task GenerateReceivablesReportPdfAsync(List<ReceivablesReport> data, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("ACCOUNTS RECEIVABLE REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text($"Generated: {DateTime.Now:dd/MM/yyyy}").FontSize(12).FontColor(Colors.Grey.Darken1);
                            
                            column.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Orange.Lighten4).Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Receivable").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text($"?{data.Sum(x => x.CurrentCredit):N2}").FontSize(20).Bold().FontColor(Colors.Orange.Darken2);
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background(Colors.Red.Lighten4).Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Overlimit Customers").FontSize(12).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(data.Count(x => x.IsOverLimit).ToString()).FontSize(20).Bold().FontColor(Colors.Red.Darken2);
                                });
                            });

                            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                        page.Content().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Customer").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Limit").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Current").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Available").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Status").FontSize(11).Bold().FontColor(Colors.White);
                            });

                            foreach (var item in data)
                            {
                                var bgColor = item.IsOverLimit ? Colors.Red.Lighten4 : Colors.White;
                                var statusColor = item.IsOverLimit ? Colors.Red.Darken2 : Colors.Green.Darken2;
                                var statusText = item.IsOverLimit ? "OVER" : "OK";

                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(item.CustomerName).FontSize(10);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.CreditLimit:N2}").FontSize(10);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.CurrentCredit:N2}").FontSize(10).Bold().FontColor(Colors.Orange.Darken2);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"?{item.AvailableCredit:N2}").FontSize(10).FontColor(Colors.Green.Medium);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text(statusText).FontSize(10).Bold().FontColor(statusColor);
                            }

                            // Total Row
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("TOTAL").FontSize(11).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.CreditLimit):N2}").FontSize(11).Bold();
                            table.Cell().Background(Colors.Orange.Lighten4).Border(2).BorderColor(Colors.Orange.Medium).Padding(8).AlignRight().Text($"?{data.Sum(x => x.CurrentCredit):N2}").FontSize(12).Bold().FontColor(Colors.Orange.Darken2);
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignRight().Text($"?{data.Sum(x => x.AvailableCredit):N2}").FontSize(11).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("").FontSize(11);
                        });

                                                page.Footer().AlignCenter().Column(c =>
                                                {
                                                    c.Item().Text(text =>
                                                    {
                                                        text.Span("Page ").FontSize(9);
                                                        text.CurrentPageNumber().FontSize(9);
                                                    });
                                                });
                                            });
                                        }).GeneratePdf(filePath);
                                    });
                                }

                                #endregion
                            }
                        }
