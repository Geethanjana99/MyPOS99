using System;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MyPOS99.Models;

namespace MyPOS99.Services
{
    public class PdfReceiptService
    {
        private readonly string _receiptsFolder;

        public PdfReceiptService()
        {
            _receiptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
            if (!Directory.Exists(_receiptsFolder))
            {
                Directory.CreateDirectory(_receiptsFolder);
            }

            // Set QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerateReceipt(Sale sale, List<SaleItem> items, string customerName, string cashierName)
        {
            try
            {
                var fileName = $"Receipt_{sale.InvoiceNumber.Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(_receiptsFolder, fileName);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("MyPOS99")
                                .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().AlignCenter().Text("Point of Sale System")
                                .FontSize(12).FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .Padding(20)
                        .Column(column =>
                        {
                            // Receipt Header
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Invoice: {sale.InvoiceNumber}").Bold().FontSize(14);
                                    col.Item().Text($"Date: {sale.Date:yyyy-MM-dd HH:mm:ss}");
                                    col.Item().Text($"Cashier: {cashierName}");
                                    col.Item().Text($"Customer: {customerName}");
                                });
                            });

                            column.Item().PaddingVertical(20).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Items Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4); // Product - wider
                                    columns.ConstantColumn(40); // Qty - fixed
                                    columns.ConstantColumn(70); // Price - fixed
                                    columns.ConstantColumn(70); // Discount - fixed
                                    columns.ConstantColumn(80); // Total - fixed
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Product").FontSize(11).Bold();
                                    header.Cell().Element(CellStyle).AlignCenter().Text("Qty").FontSize(11).Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Price").FontSize(11).Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Disc").FontSize(11).Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total").FontSize(11).Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5);
                                    }
                                });

                                // Items
                                foreach (var item in items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.ProductName).FontSize(10);
                                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Qty.ToString()).FontSize(10);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:N2}").FontSize(10);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Discount:N2}").FontSize(10);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Total:N2}").FontSize(10);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                                    }
                                }
                            });

                            column.Item().PaddingVertical(15);

                            // Totals
                            column.Item().AlignRight().Column(col =>
                            {
                                col.Item().PaddingTop(10);

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Sub Total:").FontSize(11);
                                    row.ConstantItem(100).AlignRight().Text($"Rs. {sale.SubTotal:N2}").FontSize(11);
                                });

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Discount:").FontSize(11).FontColor(Colors.Red.Medium);
                                    row.ConstantItem(100).AlignRight().Text($"- Rs. {sale.Discount:N2}").FontSize(11).FontColor(Colors.Red.Medium);
                                });

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Tax:").FontSize(11);
                                    row.ConstantItem(100).AlignRight().Text($"Rs. {sale.Tax:N2}").FontSize(11);
                                });

                                col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Grey.Darken1);

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("GRAND TOTAL:").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                                    row.ConstantItem(100).AlignRight().Text($"Rs. {sale.Total:N2}").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                                });

                                col.Item().PaddingTop(10);

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Payment:").FontSize(11);
                                    row.ConstantItem(100).AlignRight().Text(sale.PaymentType).FontSize(11).Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Amount Paid:").FontSize(11);
                                    row.ConstantItem(100).AlignRight().Text($"Rs. {sale.AmountPaid:N2}").FontSize(11);
                                });

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Change:").FontSize(11).FontColor(Colors.Blue.Medium);
                                    row.ConstantItem(100).AlignRight().Text($"Rs. {sale.Change:N2}").FontSize(11).Bold().FontColor(Colors.Blue.Medium);
                                });
                            });
                        });

                    page.Footer()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("Thank you for your purchase!")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Darken1);
                            column.Item().AlignCenter().Text("Please come again!")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                });
                })
                .GeneratePdf(filePath);

                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to generate PDF receipt: {ex.Message}", ex);
                }
            }

            public void OpenPdf(string filePath)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"PDF file not found: {filePath}");
                    }

                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };

                    System.Diagnostics.Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to open PDF: {ex.Message}", ex);
                }
            }

        public string? FindReceiptByInvoiceNumber(string invoiceNumber)
        {
            var files = Directory.GetFiles(_receiptsFolder, $"Receipt_{invoiceNumber}_*.pdf");
            return files.FirstOrDefault();
        }
    }
}
