using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class SaleService
    {
        private readonly DatabaseContext _dbContext;

        public SaleService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateSaleAsync(Sale sale)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Insert sale header
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Sales (InvoiceNumber, Date, SubTotal, Discount, Tax, Total, PaymentType, AmountPaid, Change, UserId, CustomerId, Notes)
                    VALUES (@invoiceNumber, @date, @subTotal, @discount, @tax, @total, @paymentType, @amountPaid, @change, @userId, @customerId, @notes);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("@invoiceNumber", sale.InvoiceNumber);
                command.Parameters.AddWithValue("@date", sale.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@subTotal", (double)sale.SubTotal);
                command.Parameters.AddWithValue("@discount", (double)sale.Discount);
                command.Parameters.AddWithValue("@tax", (double)sale.Tax);
                command.Parameters.AddWithValue("@total", (double)sale.Total);
                command.Parameters.AddWithValue("@paymentType", sale.PaymentType);
                command.Parameters.AddWithValue("@amountPaid", (double)sale.AmountPaid);
                command.Parameters.AddWithValue("@change", (double)sale.Change);
                command.Parameters.AddWithValue("@userId", sale.UserId);
                command.Parameters.AddWithValue("@customerId", sale.CustomerId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@notes", sale.Notes ?? (object)DBNull.Value);

                var saleId = Convert.ToInt32(await command.ExecuteScalarAsync());

                // Insert sale items
                foreach (var item in sale.Items)
                {
                    var itemCommand = connection.CreateCommand();
                    itemCommand.CommandText = @"
                        INSERT INTO SaleItems (SaleId, ProductId, ProductCode, ProductName, Qty, Price, Discount, Total)
                        VALUES (@saleId, @productId, @productCode, @productName, @qty, @price, @discount, @total)
                    ";
                    itemCommand.Parameters.AddWithValue("@saleId", saleId);
                    itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                    itemCommand.Parameters.AddWithValue("@productCode", item.ProductCode);
                    itemCommand.Parameters.AddWithValue("@productName", item.ProductName);
                    itemCommand.Parameters.AddWithValue("@qty", item.Qty);
                    itemCommand.Parameters.AddWithValue("@price", (double)item.Price);
                    itemCommand.Parameters.AddWithValue("@discount", (double)item.Discount);
                    itemCommand.Parameters.AddWithValue("@total", (double)item.Total);

                    await itemCommand.ExecuteNonQueryAsync();

                    // Update product stock
                    var stockCommand = connection.CreateCommand();
                    stockCommand.CommandText = "UPDATE Products SET StockQty = StockQty - @qty WHERE Id = @productId";
                    stockCommand.Parameters.AddWithValue("@qty", item.Qty);
                    stockCommand.Parameters.AddWithValue("@productId", item.ProductId);
                    await stockCommand.ExecuteNonQueryAsync();
                }

                // Update customer total purchases if customer is specified
                if (sale.CustomerId.HasValue)
                {
                    var customerCommand = connection.CreateCommand();
                    customerCommand.CommandText = "UPDATE Customers SET TotalPurchases = TotalPurchases + @total WHERE Id = @customerId";
                    customerCommand.Parameters.AddWithValue("@total", (double)sale.Total);
                    customerCommand.Parameters.AddWithValue("@customerId", sale.CustomerId.Value);
                    await customerCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return saleId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = new List<Sale>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, Date, SubTotal, Discount, Tax, Total, PaymentType, AmountPaid, Change, UserId, CustomerId, Notes, CreatedAt
                FROM Sales
                WHERE DATE(Date) BETWEEN DATE(@startDate) AND DATE(@endDate)
                ORDER BY Date DESC
            ";
            command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sales.Add(new Sale
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    Date = DateTime.Parse(reader.GetString(2)),
                    SubTotal = (decimal)reader.GetDouble(3),
                    Discount = (decimal)reader.GetDouble(4),
                    Tax = (decimal)reader.GetDouble(5),
                    Total = (decimal)reader.GetDouble(6),
                    PaymentType = reader.GetString(7),
                    AmountPaid = (decimal)reader.GetDouble(8),
                    Change = (decimal)reader.GetDouble(9),
                    UserId = reader.GetInt32(10),
                    CustomerId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    Notes = reader.IsDBNull(12) ? null : reader.GetString(12),
                    CreatedAt = DateTime.Parse(reader.GetString(13))
                });
            }

            return sales;
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, Date, SubTotal, Discount, Tax, Total, PaymentType, AmountPaid, Change, UserId, CustomerId, Notes, CreatedAt
                FROM Sales
                WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var sale = new Sale
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    Date = DateTime.Parse(reader.GetString(2)),
                    SubTotal = (decimal)reader.GetDouble(3),
                    Discount = (decimal)reader.GetDouble(4),
                    Tax = (decimal)reader.GetDouble(5),
                    Total = (decimal)reader.GetDouble(6),
                    PaymentType = reader.GetString(7),
                    AmountPaid = (decimal)reader.GetDouble(8),
                    Change = (decimal)reader.GetDouble(9),
                    UserId = reader.GetInt32(10),
                    CustomerId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    Notes = reader.IsDBNull(12) ? null : reader.GetString(12),
                    CreatedAt = DateTime.Parse(reader.GetString(13))
                };

                // Load sale items
                var itemsCommand = connection.CreateCommand();
                itemsCommand.CommandText = "SELECT Id, SaleId, ProductId, ProductCode, ProductName, Qty, Price, Discount, Total FROM SaleItems WHERE SaleId = @saleId";
                itemsCommand.Parameters.AddWithValue("@saleId", id);

                using var itemsReader = await itemsCommand.ExecuteReaderAsync();
                while (await itemsReader.ReadAsync())
                {
                    sale.Items.Add(new SaleItem
                    {
                        Id = itemsReader.GetInt32(0),
                        SaleId = itemsReader.GetInt32(1),
                        ProductId = itemsReader.GetInt32(2),
                        ProductCode = itemsReader.GetString(3),
                        ProductName = itemsReader.GetString(4),
                        Qty = itemsReader.GetInt32(5),
                        Price = (decimal)itemsReader.GetDouble(6),
                        Discount = (decimal)itemsReader.GetDouble(7),
                        Total = (decimal)itemsReader.GetDouble(8)
                    });
                }

                return sale;
            }

            return null;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Sales WHERE DATE(Date) = DATE('now')";
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            return $"INV-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
        }

        public async Task<decimal> GetTodaysTotalSalesAsync()
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COALESCE(SUM(Total), 0) FROM Sales WHERE DATE(Date) = DATE('now')";
            var result = await command.ExecuteScalarAsync();

            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }
}
