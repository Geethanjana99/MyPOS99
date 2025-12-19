# DatabaseService Usage Guide

## Overview

The `DatabaseService` class provides a centralized, secure way to interact with the SQLite database using parameterized queries to prevent SQL injection attacks.

## Key Features

? **Parameterized Queries** - All queries use parameters for security  
? **Transaction Support** - Execute multiple operations atomically  
? **Async/Await** - All methods are asynchronous  
? **Generic Query Execution** - Flexible query execution with custom mapping  
? **Type-Safe** - Strongly typed parameter creation  
? **Connection Management** - Automatic connection handling  

## Core Methods

### 1. OpenConnection()
Opens and returns a new SQLite connection.

```csharp
using var connection = _db.OpenConnection();
```

### 2. ExecuteNonQueryAsync()
Executes INSERT, UPDATE, or DELETE commands.

```csharp
const string query = "INSERT INTO Products (Code, Name, SellPrice) VALUES (@code, @name, @price)";

var rowsAffected = await _db.ExecuteNonQueryAsync(query,
    DatabaseService.CreateParameter("@code", "P001"),
    DatabaseService.CreateParameter("@name", "Product 1"),
    DatabaseService.CreateParameter("@price", 100.50)
);
```

### 3. ExecuteScalarAsync()
Returns a single value (COUNT, SUM, etc.).

```csharp
const string query = "SELECT COUNT(*) FROM Products WHERE Category = @category";

var count = await _db.ExecuteScalarAsync(query,
    DatabaseService.CreateParameter("@category", "Electronics")
);

int totalProducts = Convert.ToInt32(count);
```

### 4. ExecuteQueryAsync<T>()
Returns a list of objects mapped from query results.

```csharp
const string query = "SELECT Id, Name, Price FROM Products WHERE Category = @category";

var products = await _db.ExecuteQueryAsync(query, reader => new Product
{
    Id = reader.GetInt32(0),
    Name = reader.GetString(1),
    SellPrice = (decimal)reader.GetDouble(2)
}, DatabaseService.CreateParameter("@category", "Electronics"));
```

### 5. ExecuteQuerySingleAsync<T>()
Returns a single object or null.

```csharp
const string query = "SELECT Id, Name, Price FROM Products WHERE Id = @id";

var product = await _db.ExecuteQuerySingleAsync(query, reader => new Product
{
    Id = reader.GetInt32(0),
    Name = reader.GetString(1),
    SellPrice = (decimal)reader.GetDouble(2)
}, DatabaseService.CreateParameter("@id", 1));
```

### 6. ExecuteTransactionAsync()
Executes multiple operations in a transaction.

```csharp
await _db.ExecuteTransactionAsync(async (connection, transaction) =>
{
    // Insert sale
    var saleCmd = connection.CreateCommand();
    saleCmd.Transaction = transaction;
    saleCmd.CommandText = "INSERT INTO Sales (...) VALUES (...)";
    await saleCmd.ExecuteNonQueryAsync();
    
    var saleId = connection.LastInsertRowId;
    
    // Insert sale items
    foreach (var item in items)
    {
        var itemCmd = connection.CreateCommand();
        itemCmd.Transaction = transaction;
        itemCmd.CommandText = "INSERT INTO SaleItems (...) VALUES (...)";
        await itemCmd.ExecuteNonQueryAsync();
    }
});
```

## Parameter Creation

### CreateParameter(string name, object? value)
Creates a parameter with automatic type detection.

```csharp
DatabaseService.CreateParameter("@name", "Product Name")
DatabaseService.CreateParameter("@price", 99.99)
DatabaseService.CreateParameter("@quantity", 10)
DatabaseService.CreateParameter("@isActive", true)
DatabaseService.CreateParameter("@optional", null) // Converts to DBNull
```

### CreateParameter(string name, object? value, DbType dbType)
Creates a parameter with explicit type.

```csharp
DatabaseService.CreateParameter("@date", dateValue, DbType.DateTime)
DatabaseService.CreateParameter("@price", priceValue, DbType.Decimal)
```

## Complete Service Example

```csharp
using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class ProductService
    {
        private readonly DatabaseService _db;

        public ProductService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        // GET ALL
        public async Task<List<Product>> GetAllProductsAsync()
        {
            const string query = @"
                SELECT Id, Code, Name, Category, CostPrice, SellPrice, 
                       StockQty, MinStockLevel, Barcode, CreatedAt, UpdatedAt 
                FROM Products
            ";

            return await _db.ExecuteQueryAsync(query, MapProduct);
        }

        // GET BY ID
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Code, Name, Category, CostPrice, SellPrice, 
                       StockQty, MinStockLevel, Barcode, CreatedAt, UpdatedAt 
                FROM Products 
                WHERE Id = @id
            ";

            return await _db.ExecuteQuerySingleAsync(query, MapProduct,
                DatabaseService.CreateParameter("@id", id)
            );
        }

        // SEARCH
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            const string query = @"
                SELECT Id, Code, Name, Category, CostPrice, SellPrice, 
                       StockQty, MinStockLevel, Barcode, CreatedAt, UpdatedAt 
                FROM Products 
                WHERE Name LIKE @search OR Code LIKE @search OR Barcode LIKE @search
            ";

            return await _db.ExecuteQueryAsync(query, MapProduct,
                DatabaseService.CreateParameter("@search", $"%{searchTerm}%")
            );
        }

        // INSERT
        public async Task<bool> AddProductAsync(Product product)
        {
            const string query = @"
                INSERT INTO Products (Code, Name, Category, CostPrice, SellPrice, 
                                    StockQty, MinStockLevel, Barcode)
                VALUES (@code, @name, @category, @costPrice, @sellPrice, 
                        @stockQty, @minStockLevel, @barcode)
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@code", product.Code),
                DatabaseService.CreateParameter("@name", product.Name),
                DatabaseService.CreateParameter("@category", product.Category),
                DatabaseService.CreateParameter("@costPrice", (double)product.CostPrice),
                DatabaseService.CreateParameter("@sellPrice", (double)product.SellPrice),
                DatabaseService.CreateParameter("@stockQty", product.StockQty),
                DatabaseService.CreateParameter("@minStockLevel", product.MinStockLevel),
                DatabaseService.CreateParameter("@barcode", product.Barcode)
            );

            return rowsAffected > 0;
        }

        // UPDATE
        public async Task<bool> UpdateProductAsync(Product product)
        {
            const string query = @"
                UPDATE Products 
                SET Code = @code, Name = @name, Category = @category,
                    CostPrice = @costPrice, SellPrice = @sellPrice,
                    StockQty = @stockQty, MinStockLevel = @minStockLevel,
                    Barcode = @barcode, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @id
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", product.Id),
                DatabaseService.CreateParameter("@code", product.Code),
                DatabaseService.CreateParameter("@name", product.Name),
                DatabaseService.CreateParameter("@category", product.Category),
                DatabaseService.CreateParameter("@costPrice", (double)product.CostPrice),
                DatabaseService.CreateParameter("@sellPrice", (double)product.SellPrice),
                DatabaseService.CreateParameter("@stockQty", product.StockQty),
                DatabaseService.CreateParameter("@minStockLevel", product.MinStockLevel),
                DatabaseService.CreateParameter("@barcode", product.Barcode)
            );

            return rowsAffected > 0;
        }

        // DELETE
        public async Task<bool> DeleteProductAsync(int id)
        {
            const string query = "DELETE FROM Products WHERE Id = @id";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", id)
            );

            return rowsAffected > 0;
        }

        // Helper method to map SqliteDataReader to Product
        private static Product MapProduct(SqliteDataReader reader)
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
                CreatedAt = DateTime.Parse(reader.GetString(9)),
                UpdatedAt = DateTime.Parse(reader.GetString(10))
            };
        }
    }
}
```

## Transaction Example

```csharp
public async Task<int> CreateSaleWithTransactionAsync(Sale sale)
{
    int saleId = 0;
    
    await _db.ExecuteTransactionAsync(async (connection, transaction) =>
    {
        // Insert sale header
        var saleCmd = connection.CreateCommand();
        saleCmd.Transaction = transaction;
        saleCmd.CommandText = @"
            INSERT INTO Sales (InvoiceNumber, Date, Total, PaymentType, UserId)
            VALUES (@invoice, @date, @total, @payment, @userId);
            SELECT last_insert_rowid();
        ";
        saleCmd.Parameters.AddWithValue("@invoice", sale.InvoiceNumber);
        saleCmd.Parameters.AddWithValue("@date", sale.Date.ToString("yyyy-MM-dd HH:mm:ss"));
        saleCmd.Parameters.AddWithValue("@total", (double)sale.Total);
        saleCmd.Parameters.AddWithValue("@payment", sale.PaymentType);
        saleCmd.Parameters.AddWithValue("@userId", sale.UserId);
        
        saleId = Convert.ToInt32(await saleCmd.ExecuteScalarAsync());
        
        // Insert sale items
        foreach (var item in sale.Items)
        {
            var itemCmd = connection.CreateCommand();
            itemCmd.Transaction = transaction;
            itemCmd.CommandText = @"
                INSERT INTO SaleItems (SaleId, ProductId, Qty, Price, Total)
                VALUES (@saleId, @productId, @qty, @price, @total)
            ";
            itemCmd.Parameters.AddWithValue("@saleId", saleId);
            itemCmd.Parameters.AddWithValue("@productId", item.ProductId);
            itemCmd.Parameters.AddWithValue("@qty", item.Qty);
            itemCmd.Parameters.AddWithValue("@price", (double)item.Price);
            itemCmd.Parameters.AddWithValue("@total", (double)item.Total);
            
            await itemCmd.ExecuteNonQueryAsync();
            
            // Update stock
            var stockCmd = connection.CreateCommand();
            stockCmd.Transaction = transaction;
            stockCmd.CommandText = "UPDATE Products SET StockQty = StockQty - @qty WHERE Id = @id";
            stockCmd.Parameters.AddWithValue("@qty", item.Qty);
            stockCmd.Parameters.AddWithValue("@id", item.ProductId);
            
            await stockCmd.ExecuteNonQueryAsync();
        }
    });
    
    return saleId;
}
```

## Best Practices

### 1. Always Use Parameters
? **BAD** - SQL Injection Risk:
```csharp
var query = $"SELECT * FROM Users WHERE Username = '{username}'";
```

? **GOOD** - Safe:
```csharp
const string query = "SELECT * FROM Users WHERE Username = @username";
var user = await _db.ExecuteQuerySingleAsync(query, MapUser,
    DatabaseService.CreateParameter("@username", username)
);
```

### 2. Use const for Query Strings
```csharp
const string query = "SELECT * FROM Products WHERE Id = @id";
```

### 3. Create Mapper Functions
```csharp
private static Product MapProduct(SqliteDataReader reader)
{
    return new Product { /* mapping */ };
}
```

### 4. Handle Nullable Values
```csharp
Category = reader.IsDBNull(3) ? null : reader.GetString(3)
```

### 5. Use Transactions for Multiple Operations
```csharp
await _db.ExecuteTransactionAsync(async (connection, transaction) => 
{
    // Multiple related operations
});
```

## Initialization

```csharp
// In your application startup or dependency injection
var databaseService = new DatabaseService();

// Pass to services
var productService = new ProductService(databaseService);
var saleService = new SaleService(databaseService);
var userService = new UserService(databaseService);
```

## Backward Compatibility

The old `DatabaseContext` class is still available for backward compatibility:

```csharp
var dbContext = new DatabaseContext();
using var connection = dbContext.GetConnection();
// Use connection directly
```

However, it's recommended to migrate to `DatabaseService` for better security and maintainability.
