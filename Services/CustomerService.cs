using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class CustomerService
    {
        private readonly DatabaseService _db;

        public CustomerService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, TotalPurchases, CreatedAt, IsActive 
                FROM Customers 
                WHERE IsActive = 1 
                ORDER BY Name
            ";

            return await _db.ExecuteQueryAsync(query, MapCustomer);
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, TotalPurchases, CreatedAt, IsActive 
                FROM Customers 
                WHERE Id = @id
            ";

            return await _db.ExecuteQuerySingleAsync(query, MapCustomer,
                DatabaseService.CreateParameter("@id", id)
            );
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, TotalPurchases, CreatedAt, IsActive 
                FROM Customers 
                WHERE Phone = @phone
            ";

            return await _db.ExecuteQuerySingleAsync(query, MapCustomer,
                DatabaseService.CreateParameter("@phone", phone)
            );
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, TotalPurchases, CreatedAt, IsActive 
                FROM Customers 
                WHERE (Name LIKE @search OR Phone LIKE @search OR Email LIKE @search) 
                AND IsActive = 1
                ORDER BY Name
            ";

            var searchParam = DatabaseService.CreateParameter("@search", $"%{searchTerm}%");

            return await _db.ExecuteQueryAsync(query, MapCustomer, searchParam);
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            const string query = @"
                INSERT INTO Customers (Name, Phone, Email, Address, IsActive)
                VALUES (@name, @phone, @email, @address, @isActive)
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@name", customer.Name),
                DatabaseService.CreateParameter("@phone", customer.Phone),
                DatabaseService.CreateParameter("@email", customer.Email),
                DatabaseService.CreateParameter("@address", customer.Address),
                DatabaseService.CreateParameter("@isActive", customer.IsActive ? 1 : 0)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            const string query = @"
                UPDATE Customers 
                SET Name = @name, Phone = @phone, Email = @email, 
                    Address = @address, IsActive = @isActive
                WHERE Id = @id
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", customer.Id),
                DatabaseService.CreateParameter("@name", customer.Name),
                DatabaseService.CreateParameter("@phone", customer.Phone),
                DatabaseService.CreateParameter("@email", customer.Email),
                DatabaseService.CreateParameter("@address", customer.Address),
                DatabaseService.CreateParameter("@isActive", customer.IsActive ? 1 : 0)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            const string query = "UPDATE Customers SET IsActive = 0 WHERE Id = @id";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", id)
            );

            return rowsAffected > 0;
        }

        public async Task<List<Sale>> GetCustomerPurchaseHistoryAsync(int customerId)
        {
            const string query = @"
                SELECT Id, InvoiceNumber, Date, SubTotal, Discount, Tax, Total, 
                       PaymentType, AmountPaid, Change, UserId, CustomerId, Notes, CreatedAt
                FROM Sales
                WHERE CustomerId = @customerId
                ORDER BY Date DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new Sale
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
            }, DatabaseService.CreateParameter("@customerId", customerId));
        }

        public async Task<List<Customer>> GetTopCustomersAsync(int limit = 10)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, TotalPurchases, CreatedAt, IsActive 
                FROM Customers 
                WHERE IsActive = 1 
                ORDER BY TotalPurchases DESC 
                LIMIT @limit
            ";

            return await _db.ExecuteQueryAsync(query, MapCustomer,
                DatabaseService.CreateParameter("@limit", limit)
            );
        }

        private static Customer MapCustomer(SqliteDataReader reader)
        {
            return new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                TotalPurchases = (decimal)reader.GetDouble(5),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                IsActive = reader.GetInt32(7) == 1
            };
        }
    }
}
