using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class ProductService
    {
        private readonly DatabaseContext _dbContext;

        public ProductService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.Code, p.Name, p.Category, p.CostPrice, p.SellPrice, 
                       p.StockQty, p.MinStockLevel, p.Barcode, p.SupplierId, 
                       p.CreatedAt, p.UpdatedAt, s.Name as SupplierName
                FROM Products p
                LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            ";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CostPrice = (decimal)reader.GetDouble(4),
                    SellPrice = (decimal)reader.GetDouble(5),
                    StockQty = reader.GetInt32(6),
                    MinStockLevel = reader.GetInt32(7),
                    Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                    SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    CreatedAt = DateTime.Parse(reader.GetString(10)),
                    UpdatedAt = DateTime.Parse(reader.GetString(11)),
                    Supplier = reader.IsDBNull(12) ? null : new Supplier { Name = reader.GetString(12) }
                });
            }

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId, CreatedAt, UpdatedAt FROM Products WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CostPrice = (decimal)reader.GetDouble(4),
                    SellPrice = (decimal)reader.GetDouble(5),
                    StockQty = reader.GetInt32(6),
                    MinStockLevel = reader.GetInt32(7),
                    Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                    SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    CreatedAt = DateTime.Parse(reader.GetString(10)),
                    UpdatedAt = DateTime.Parse(reader.GetString(11))
                };
            }

            return null;
        }

        public async Task<Product?> GetProductByCodeAsync(string code)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId, CreatedAt, UpdatedAt FROM Products WHERE Code = @code";
            command.Parameters.AddWithValue("@code", code);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CostPrice = (decimal)reader.GetDouble(4),
                    SellPrice = (decimal)reader.GetDouble(5),
                    StockQty = reader.GetInt32(6),
                    MinStockLevel = reader.GetInt32(7),
                    Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                    SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    CreatedAt = DateTime.Parse(reader.GetString(10)),
                    UpdatedAt = DateTime.Parse(reader.GetString(11))
                };
            }

            return null;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Products (Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId)
                    VALUES (@code, @name, @category, @costPrice, @sellPrice, @stockQty, @minStockLevel, @barcode, @supplierId)
                ";
                command.Parameters.AddWithValue("@code", product.Code);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@category", product.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@costPrice", (double)product.CostPrice);
                command.Parameters.AddWithValue("@sellPrice", (double)product.SellPrice);
                command.Parameters.AddWithValue("@stockQty", product.StockQty);
                command.Parameters.AddWithValue("@minStockLevel", product.MinStockLevel);
                command.Parameters.AddWithValue("@barcode", product.Barcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@supplierId", product.SupplierId.HasValue ? (object)product.SupplierId.Value : DBNull.Value);

                return await command.ExecuteNonQueryAsync() > 0;
            }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Products 
                SET Code = @code, Name = @name, Category = @category, 
                    CostPrice = @costPrice, SellPrice = @sellPrice, 
                    StockQty = @stockQty, MinStockLevel = @minStockLevel, 
                    Barcode = @barcode, SupplierId = @supplierId, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @id
            ";
                command.Parameters.AddWithValue("@id", product.Id);
                command.Parameters.AddWithValue("@code", product.Code);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@category", product.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@costPrice", (double)product.CostPrice);
                command.Parameters.AddWithValue("@sellPrice", (double)product.SellPrice);
                command.Parameters.AddWithValue("@stockQty", product.StockQty);
                command.Parameters.AddWithValue("@minStockLevel", product.MinStockLevel);
                command.Parameters.AddWithValue("@barcode", product.Barcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@supplierId", product.SupplierId.HasValue ? (object)product.SupplierId.Value : DBNull.Value);

                return await command.ExecuteNonQueryAsync() > 0;
            }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Products WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Product>> GetLowStockProductsAsync()
        {
            var products = new List<Product>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId, CreatedAt, UpdatedAt FROM Products WHERE StockQty <= MinStockLevel";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    products.Add(new Product
                    {
                        Id = reader.GetInt32(0),
                        Code = reader.GetString(1),
                        Name = reader.GetString(2),
                        Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CostPrice = (decimal)reader.GetDouble(4),
                        SellPrice = (decimal)reader.GetDouble(5),
                        StockQty = reader.GetInt32(6),
                        MinStockLevel = reader.GetInt32(7),
                        Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                        CreatedAt = DateTime.Parse(reader.GetString(10)),
                        UpdatedAt = DateTime.Parse(reader.GetString(11))
                    });
                }

                return products;
            }

                public async Task<bool> UpdateStockAsync(int productId, int quantity)
                {
                    using var connection = _dbContext.GetConnection();
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        UPDATE Products 
                        SET StockQty = StockQty + @quantity, UpdatedAt = CURRENT_TIMESTAMP
                        WHERE Id = @id
                    ";
                    command.Parameters.AddWithValue("@id", productId);
                    command.Parameters.AddWithValue("@quantity", quantity);

                    return await command.ExecuteNonQueryAsync() > 0;
                }

                public async Task<Product?> GetProductByBarcodeAsync(string barcode)
                {
                    using var connection = _dbContext.GetConnection();
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId, CreatedAt, UpdatedAt FROM Products WHERE Barcode = @barcode";
                    command.Parameters.AddWithValue("@barcode", barcode);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return new Product
                        {
                            Id = reader.GetInt32(0),
                            Code = reader.GetString(1),
                            Name = reader.GetString(2),
                            Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CostPrice = (decimal)reader.GetDouble(4),
                            SellPrice = (decimal)reader.GetDouble(5),
                            StockQty = reader.GetInt32(6),
                            MinStockLevel = reader.GetInt32(7),
                            Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                            SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            CreatedAt = DateTime.Parse(reader.GetString(10)),
                            UpdatedAt = DateTime.Parse(reader.GetString(11))
                        };
                    }

                    return null;
                }

                public async Task<List<Product>> SearchProductsAsync(string searchTerm)
                {
                    var products = new List<Product>();

                    using var connection = _dbContext.GetConnection();
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT Id, Code, Name, Category, CostPrice, SellPrice, StockQty, MinStockLevel, Barcode, SupplierId, CreatedAt, UpdatedAt 
                        FROM Products 
                        WHERE Name LIKE @search OR Code LIKE @search OR Barcode LIKE @search
                        LIMIT 10
                    ";
                    command.Parameters.AddWithValue("@search", $"%{searchTerm}%");

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Code = reader.GetString(1),
                            Name = reader.GetString(2),
                            Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CostPrice = (decimal)reader.GetDouble(4),
                            SellPrice = (decimal)reader.GetDouble(5),
                            StockQty = reader.GetInt32(6),
                            MinStockLevel = reader.GetInt32(7),
                            Barcode = reader.IsDBNull(8) ? null : reader.GetString(8),
                            SupplierId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            CreatedAt = DateTime.Parse(reader.GetString(10)),
                            UpdatedAt = DateTime.Parse(reader.GetString(11))
                        });
                    }

                    return products;
                }
            }
        }
