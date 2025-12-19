using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyPOS99.Data;
using MyPOS99.Models;

namespace MyPOS99.Services
{
    public class ReturnService
    {
        private readonly DatabaseService _db;

        public ReturnService()
        {
            _db = new DatabaseService();
        }

        public async Task<int> CreateReturnAsync(Return returnRecord, List<ReturnItem> items)
        {
            using var connection = _db.OpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert return record
                const string returnQuery = @"
                    INSERT INTO Returns (ReturnNumber, SaleId, OriginalInvoiceNumber, ReturnDate, 
                                        TotalAmount, Reason, ProcessedByUserId, Notes, CreatedAt)
                    VALUES (@returnNumber, @saleId, @originalInvoice, @returnDate, 
                            @totalAmount, @reason, @userId, @notes, @createdAt);
                    SELECT last_insert_rowid();
                ";

                var returnCommand = connection.CreateCommand();
                returnCommand.Transaction = transaction;
                returnCommand.CommandText = returnQuery;
                returnCommand.Parameters.AddWithValue("@returnNumber", returnRecord.ReturnNumber);
                returnCommand.Parameters.AddWithValue("@saleId", returnRecord.SaleId);
                returnCommand.Parameters.AddWithValue("@originalInvoice", returnRecord.OriginalInvoiceNumber);
                returnCommand.Parameters.AddWithValue("@returnDate", returnRecord.ReturnDate.ToString("yyyy-MM-dd HH:mm:ss"));
                returnCommand.Parameters.AddWithValue("@totalAmount", returnRecord.TotalAmount);
                returnCommand.Parameters.AddWithValue("@reason", returnRecord.Reason ?? string.Empty);
                returnCommand.Parameters.AddWithValue("@userId", returnRecord.ProcessedByUserId);
                returnCommand.Parameters.AddWithValue("@notes", returnRecord.Notes ?? string.Empty);
                returnCommand.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                var returnId = Convert.ToInt32(await returnCommand.ExecuteScalarAsync());

                // Insert return items and update stock
                foreach (var item in items)
                {
                    // Insert return item
                    const string itemQuery = @"
                        INSERT INTO ReturnItems (ReturnId, ProductId, ProductName, Quantity, Price, Total)
                        VALUES (@returnId, @productId, @productName, @quantity, @price, @total)
                    ";

                    var itemCommand = connection.CreateCommand();
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = itemQuery;
                    itemCommand.Parameters.AddWithValue("@returnId", returnId);
                    itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                    itemCommand.Parameters.AddWithValue("@productName", item.ProductName);
                    itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                    itemCommand.Parameters.AddWithValue("@price", item.Price);
                    itemCommand.Parameters.AddWithValue("@total", item.Total);

                    await itemCommand.ExecuteNonQueryAsync();

                    // Update product stock (add back returned quantity)
                    const string stockQuery = @"
                        UPDATE Products 
                        SET StockQty = StockQty + @quantity, UpdatedAt = @updatedAt
                        WHERE Id = @productId
                    ";

                    var stockCommand = connection.CreateCommand();
                    stockCommand.Transaction = transaction;
                    stockCommand.CommandText = stockQuery;
                    stockCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                    stockCommand.Parameters.AddWithValue("@productId", item.ProductId);
                    stockCommand.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    await stockCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return returnId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Return>> GetAllReturnsAsync()
        {
            const string query = @"
                SELECT Id, ReturnNumber, SaleId, OriginalInvoiceNumber, ReturnDate, 
                       TotalAmount, Reason, ProcessedByUserId, Notes, CreatedAt
                FROM Returns
                ORDER BY ReturnDate DESC
            ";

            var db = new DatabaseService();
            var returns = await db.ExecuteQueryAsync(query, reader => new Return
            {
                Id = reader.GetInt32(0),
                ReturnNumber = reader.GetString(1),
                SaleId = reader.GetInt32(2),
                OriginalInvoiceNumber = reader.GetString(3),
                ReturnDate = DateTime.Parse(reader.GetString(4)),
                TotalAmount = (decimal)reader.GetDouble(5),
                Reason = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                ProcessedByUserId = reader.GetInt32(7),
                Notes = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                CreatedAt = DateTime.Parse(reader.GetString(9))
            });

            return returns.ToList();
        }

        public async Task<List<ReturnItem>> GetReturnItemsAsync(int returnId)
        {
            const string query = @"
                SELECT Id, ReturnId, ProductId, ProductName, Quantity, Price, Total
                FROM ReturnItems
                WHERE ReturnId = @returnId
            ";

            var db = new DatabaseService();
            var items = await db.ExecuteQueryAsync(query, reader => new ReturnItem
            {
                Id = reader.GetInt32(0),
                ReturnId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3),
                Quantity = reader.GetInt32(4),
                Price = (decimal)reader.GetDouble(5),
                Total = (decimal)reader.GetDouble(6)
            }, DatabaseService.CreateParameter("@returnId", returnId));

            return items.ToList();
        }
    }
}
