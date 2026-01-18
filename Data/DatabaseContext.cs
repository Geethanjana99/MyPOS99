using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mypos99.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        /// <summary>
        /// Opens and returns a new SQLite connection
        /// </summary>
        public SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE) with parameters
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string commandText, params SqliteParameter[] parameters)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a scalar query (returns single value) with parameters
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string commandText, params SqliteParameter[] parameters)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Executes a query and returns a data reader with parameters
        /// </summary>
        public async Task<List<T>> ExecuteQueryAsync<T>(string commandText, Func<SqliteDataReader, T> mapFunction, params SqliteParameter[] parameters)
        {
            var results = new List<T>();

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(mapFunction(reader));
            }

            return results;
        }

        /// <summary>
        /// Executes a query and returns a single result with parameters
        /// </summary>
        public async Task<T?> ExecuteQuerySingleAsync<T>(string commandText, Func<SqliteDataReader, T> mapFunction, params SqliteParameter[] parameters) where T : class
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return mapFunction(reader);
            }

            return null;
        }

        /// <summary>
        /// Executes multiple commands within a transaction
        /// </summary>
        public async Task<bool> ExecuteTransactionAsync(Func<SqliteConnection, SqliteTransaction, Task> transactionWork)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                await transactionWork(connection, transaction);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Creates a new SqliteParameter for parameterized queries
        /// </summary>
        public static SqliteParameter CreateParameter(string name, object? value)
        {
            return new SqliteParameter(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// Creates a new SqliteParameter with specific DbType
        /// </summary>
        public static SqliteParameter CreateParameter(string name, object? value, DbType dbType)
        {
            var parameter = new SqliteParameter(name, value ?? DBNull.Value);
            parameter.DbType = dbType;
            return parameter;
        }

        private void InitializeDatabase()
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                -- Users Table
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Cashier', 'Manager')),
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    IsActive INTEGER DEFAULT 1
                );

                -- Products Table
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Category TEXT,
                    CostPrice REAL NOT NULL DEFAULT 0,
                    SellPrice REAL NOT NULL,
                    StockQty INTEGER DEFAULT 0,
                    MinStockLevel INTEGER DEFAULT 0,
                    Barcode TEXT,
                    SupplierId INTEGER,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                );

                -- Suppliers Table
                CREATE TABLE IF NOT EXISTS Suppliers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    Address TEXT,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    IsActive INTEGER DEFAULT 1
                );

                -- Customers Table
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    Address TEXT,
                    TotalPurchases REAL DEFAULT 0,
                    CreditLimit REAL DEFAULT 0,
                    CurrentCredit REAL DEFAULT 0,
                    IsCreditCustomer INTEGER DEFAULT 0,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    IsActive INTEGER DEFAULT 1
                );

                -- Sales Table
                CREATE TABLE IF NOT EXISTS Sales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT NOT NULL UNIQUE,
                    Date TEXT NOT NULL,
                    SubTotal REAL NOT NULL,
                    Discount REAL DEFAULT 0,
                    Tax REAL DEFAULT 0,
                    Total REAL NOT NULL,
                    PaymentType TEXT NOT NULL CHECK(PaymentType IN ('Cash', 'Card', 'Mobile', 'Credit')),
                    AmountPaid REAL NOT NULL,
                    Change REAL DEFAULT 0,
                    UserId INTEGER NOT NULL,
                    CustomerId INTEGER,
                    Notes TEXT,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id),
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                );

                -- SaleItems Table
                CREATE TABLE IF NOT EXISTS SaleItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductCode TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    Qty INTEGER NOT NULL,
                    Price REAL NOT NULL,
                    Discount REAL DEFAULT 0,
                    Total REAL NOT NULL,
                    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );

                -- Purchases Table
                CREATE TABLE IF NOT EXISTS Purchases (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PurchaseNumber TEXT NOT NULL UNIQUE,
                    SupplierId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    SubTotal REAL NOT NULL,
                    Tax REAL DEFAULT 0,
                    Total REAL NOT NULL,
                    PaymentStatus TEXT DEFAULT 'Pending' CHECK(PaymentStatus IN ('Pending', 'Partial', 'Paid')),
                    AmountPaid REAL DEFAULT 0,
                    Notes TEXT,
                    UserId INTEGER NOT NULL,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                -- PurchaseItems Table
                CREATE TABLE IF NOT EXISTS PurchaseItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PurchaseId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductCode TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    Qty INTEGER NOT NULL,
                    CostPrice REAL NOT NULL,
                    Total REAL NOT NULL,
                    FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );

                -- Expenses Table
                CREATE TABLE IF NOT EXISTS Expenses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Date TEXT NOT NULL,
                    Note TEXT,
                    PaymentMethod TEXT,
                    UserId INTEGER NOT NULL,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                    -- Categories Table (for product categories)
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        Description TEXT,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                    );

                    -- Returns Table
                    CREATE TABLE IF NOT EXISTS Returns (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ReturnNumber TEXT NOT NULL UNIQUE,
                        SaleId INTEGER NOT NULL,
                        OriginalInvoiceNumber TEXT NOT NULL,
                        ReturnDate TEXT NOT NULL,
                        TotalAmount REAL NOT NULL,
                        Reason TEXT,
                        ProcessedByUserId INTEGER NOT NULL,
                        Notes TEXT,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (SaleId) REFERENCES Sales(Id),
                        FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id)
                    );

                    -- ReturnItems Table
                    CREATE TABLE IF NOT EXISTS ReturnItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ReturnId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        ProductName TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Price REAL NOT NULL,
                        Total REAL NOT NULL,
                        FOREIGN KEY (ReturnId) REFERENCES Returns(Id) ON DELETE CASCADE,
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    );

                    -- Create indexes for better performance
                    CREATE INDEX IF NOT EXISTS idx_products_code ON Products(Code);
                    CREATE INDEX IF NOT EXISTS idx_products_barcode ON Products(Barcode);
                    CREATE INDEX IF NOT EXISTS idx_sales_date ON Sales(Date);
                    CREATE INDEX IF NOT EXISTS idx_sales_user ON Sales(UserId);
                    CREATE INDEX IF NOT EXISTS idx_saleitems_sale ON SaleItems(SaleId);
                    CREATE INDEX IF NOT EXISTS idx_purchases_supplier ON Purchases(SupplierId);
                    CREATE INDEX IF NOT EXISTS idx_purchases_date ON Purchases(Date);
                    CREATE INDEX IF NOT EXISTS idx_expenses_date ON Expenses(Date);
                    CREATE INDEX IF NOT EXISTS idx_returns_sale ON Returns(SaleId);
                    CREATE INDEX IF NOT EXISTS idx_returns_date ON Returns(ReturnDate);
                    CREATE INDEX IF NOT EXISTS idx_returnitems_return ON ReturnItems(ReturnId);
                ";
            
            command.ExecuteNonQuery();

            // Migrate existing Customers table to add new columns if they don't exist
            MigrateCustomersTable(connection);

            // Migrate existing Products table to add SupplierId column if it doesn't exist
            MigrateProductsTable(connection);

            // Insert default admin user with BCrypt hashed password (password: admin123)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT OR IGNORE INTO Users (Id, Username, PasswordHash, Role) 
                VALUES (1, 'admin', @passwordHash, 'Admin')
            ";
            insertCommand.Parameters.AddWithValue("@passwordHash", hashedPassword);
                insertCommand.ExecuteNonQuery();

                // Insert default Walk-in Customer (after migration to ensure columns exist)
                try
                {
                    var insertCustomerCommand = connection.CreateCommand();
                    insertCustomerCommand.CommandText = @"
                        INSERT OR IGNORE INTO Customers (Id, Name, Phone, Email, Address, CreditLimit, CurrentCredit, IsCreditCustomer, IsActive) 
                        VALUES (1, 'Walk-in Customer', NULL, NULL, NULL, 0, 0, 0, 1)
                    ";
                    insertCustomerCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // If the full insert fails (old schema), try basic insert
                    var basicInsertCommand = connection.CreateCommand();
                    basicInsertCommand.CommandText = @"
                        INSERT OR IGNORE INTO Customers (Id, Name, IsActive) 
                        VALUES (1, 'Walk-in Customer', 1)
                    ";
                    basicInsertCommand.ExecuteNonQuery();
                }
            }

            private void MigrateCustomersTable(SqliteConnection connection)
            {
                try
                {
                    // Check if new columns exist and add them if they don't
                    var columnCheckCommand = connection.CreateCommand();
                    columnCheckCommand.CommandText = "PRAGMA table_info(Customers)";

                    var existingColumns = new HashSet<string>();
                    using (var reader = columnCheckCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingColumns.Add(reader.GetString(1).ToLower()); // Column name is at index 1
                        }
                    }

                    // Add CreditLimit column if it doesn't exist
                    if (!existingColumns.Contains("creditlimit"))
                    {
                        var addColumnCommand = connection.CreateCommand();
                        addColumnCommand.CommandText = "ALTER TABLE Customers ADD COLUMN CreditLimit REAL DEFAULT 0";
                        addColumnCommand.ExecuteNonQuery();
                    }

                    // Add CurrentCredit column if it doesn't exist
                    if (!existingColumns.Contains("currentcredit"))
                    {
                        var addColumnCommand = connection.CreateCommand();
                        addColumnCommand.CommandText = "ALTER TABLE Customers ADD COLUMN CurrentCredit REAL DEFAULT 0";
                        addColumnCommand.ExecuteNonQuery();
                    }

                    // Add IsCreditCustomer column if it doesn't exist
                    if (!existingColumns.Contains("iscreditcustomer"))
                    {
                        var addColumnCommand = connection.CreateCommand();
                        addColumnCommand.CommandText = "ALTER TABLE Customers ADD COLUMN IsCreditCustomer INTEGER DEFAULT 0";
                        addColumnCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    // If migration fails, table might not exist yet, which is fine
                                    // CREATE TABLE IF NOT EXISTS will handle it
                                }
                            }

                            private void MigrateProductsTable(SqliteConnection connection)
                            {
                                try
                                {
                                    // Check if SupplierId column exists and add it if it doesn't
                                    var columnCheckCommand = connection.CreateCommand();
                                    columnCheckCommand.CommandText = "PRAGMA table_info(Products)";

                                    var existingColumns = new HashSet<string>();
                                    using (var reader = columnCheckCommand.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            existingColumns.Add(reader.GetString(1).ToLower()); // Column name is at index 1
                                        }
                                    }

                                    // Add SupplierId column if it doesn't exist
                                    if (!existingColumns.Contains("supplierid"))
                                    {
                                        var addColumnCommand = connection.CreateCommand();
                                        addColumnCommand.CommandText = "ALTER TABLE Products ADD COLUMN SupplierId INTEGER";
                                        addColumnCommand.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception)
                                {
                                    // If migration fails, table might not exist yet, which is fine
                                    // CREATE TABLE IF NOT EXISTS will handle it
                                }
                            }
                    }

    // Keep DatabaseContext for backward compatibility
    public class DatabaseContext
    {
        private readonly DatabaseService _databaseService;

        public DatabaseContext()
        {
            _databaseService = new DatabaseService();
        }

        public SqliteConnection GetConnection()
        {
            return _databaseService.OpenConnection();
        }
    }
}
