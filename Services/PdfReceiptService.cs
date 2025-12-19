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
                        // 80mm thermal printer paper (3.15 inches width)
                        page.Size(new PageSize(226, 800)); // 80mm width, dynamic height
                        page.Margin(8); // Small margins for thermal printers
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Black));

                    page.Header()
                        .PaddingBottom(5)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("MyPOS99")
                                .FontSize(14).Bold().FontColor(Colors.Black);
                            column.Item().AlignCenter().Text("Point of Sale System")
                                .FontSize(8).FontColor(Colors.Black);
                            column.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Black);
                        });

                    page.Content()
                        .PaddingVertical(5)
                        .Column(column =>
                        {
                            // Receipt Header Info
                            column.Item().Text($"Invoice: {sale.InvoiceNumber}").FontSize(8).Bold();
                            column.Item().Text($"Date: {sale.Date:dd/MM/yyyy HH:mm}").FontSize(7);
                            column.Item().Text($"Cashier: {cashierName}").FontSize(7);
                            column.Item().Text($"Customer: {customerName}").FontSize(7);

                            column.Item().PaddingVertical(3).LineHorizontal(1).LineColor(Colors.Black);

                            // Items - Simple list format for thermal printer
                            foreach (var item in items)
                            {
                                column.Item().PaddingBottom(2).Column(itemCol =>
                                {
                                    // Product name
                                    itemCol.Item().Text(item.ProductName).FontSize(8).Bold();

                                    // Quantity x Price = Total
                                    itemCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"{item.Qty} x Rs.{item.Price:N2}").FontSize(7);
                                        row.ConstantItem(50).AlignRight().Text($"Rs. {item.Total:N2}").FontSize(8).Bold();
                                    });

                                    // Discount if any
                                    if (item.Discount > 0)
                                    {
                                        itemCol.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"  Discount").FontSize(6);
                                            row.ConstantItem(50).AlignRight().Text($"-Rs. {item.Discount * item.Qty:N2}").FontSize(7);
                                        });
                                    }
                                });
                            }

                            column.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Black);

                            // Totals
                            column.Item().PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text("Sub Total:").FontSize(8);
                                row.ConstantItem(60).AlignRight().Text($"Rs. {sale.SubTotal:N2}").FontSize(8);
                            });

                            if (sale.Discount > 0)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Discount:").FontSize(8);
                                    row.ConstantItem(60).AlignRight().Text($"-Rs. {sale.Discount:N2}").FontSize(8);
                                });
                            }

                            if (sale.Tax > 0)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Tax:").FontSize(8);
                                    row.ConstantItem(60).AlignRight().Text($"Rs. {sale.Tax:N2}").FontSize(8);
                                });
                            }

                            column.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Colors.Black);

                            // Grand Total
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").FontSize(10).Bold();
                                row.ConstantItem(60).AlignRight().Text($"Rs. {sale.Total:N2}").FontSize(10).Bold();
                            });

                            column.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Black);

                            // Payment Info
                            column.Item().PaddingTop(2).Row(row =>
                            {
                                row.RelativeItem().Text($"Payment: {sale.PaymentType}").FontSize(7);
                                row.ConstantItem(60).AlignRight().Text($"Rs. {sale.AmountPaid:N2}").FontSize(7);
                            });

                            if (sale.Change > 0)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Change:").FontSize(8);
                                    row.ConstantItem(60).AlignRight().Text($"Rs. {sale.Change:N2}").FontSize(8).Bold();
                                });
                            }
                        });

                    page.Footer()
                        .PaddingTop(5)
                        .Column(column =>
                        {
                            column.Item().LineHorizontal(1).LineColor(Colors.Black);
                            column.Item().PaddingTop(3).AlignCenter().Text("Thank you!")
                                .FontSize(8).Bold();
                            column.Item().AlignCenter().Text("Please come again")
                                .FontSize(7);
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
