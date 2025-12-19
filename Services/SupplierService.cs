using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class SupplierService
    {
        private readonly DatabaseService _db;

        public SupplierService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, CreatedAt, IsActive 
                FROM Suppliers 
                WHERE IsActive = 1 
                ORDER BY Name
            ";

            return await _db.ExecuteQueryAsync(query, MapSupplier);
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, CreatedAt, IsActive 
                FROM Suppliers 
                WHERE Id = @id
            ";

            return await _db.ExecuteQuerySingleAsync(query, MapSupplier,
                DatabaseService.CreateParameter("@id", id)
            );
        }

        public async Task<List<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            const string query = @"
                SELECT Id, Name, Phone, Email, Address, CreatedAt, IsActive 
                FROM Suppliers 
                WHERE (Name LIKE @search OR Phone LIKE @search OR Email LIKE @search) 
                AND IsActive = 1
                ORDER BY Name
            ";

            return await _db.ExecuteQueryAsync(query, MapSupplier,
                DatabaseService.CreateParameter("@search", $"%{searchTerm}%")
            );
        }

        public async Task<bool> AddSupplierAsync(Supplier supplier)
        {
            const string query = @"
                INSERT INTO Suppliers (Name, Phone, Email, Address, IsActive)
                VALUES (@name, @phone, @email, @address, @isActive)
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@name", supplier.Name),
                DatabaseService.CreateParameter("@phone", supplier.Phone),
                DatabaseService.CreateParameter("@email", supplier.Email),
                DatabaseService.CreateParameter("@address", supplier.Address),
                DatabaseService.CreateParameter("@isActive", supplier.IsActive ? 1 : 0)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            const string query = @"
                UPDATE Suppliers 
                SET Name = @name, Phone = @phone, Email = @email, 
                    Address = @address, IsActive = @isActive
                WHERE Id = @id
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", supplier.Id),
                DatabaseService.CreateParameter("@name", supplier.Name),
                DatabaseService.CreateParameter("@phone", supplier.Phone),
                DatabaseService.CreateParameter("@email", supplier.Email),
                DatabaseService.CreateParameter("@address", supplier.Address),
                DatabaseService.CreateParameter("@isActive", supplier.IsActive ? 1 : 0)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateSupplierAsync(int id)
        {
            const string query = "UPDATE Suppliers SET IsActive = 0 WHERE Id = @id";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", id)
            );

            return rowsAffected > 0;
        }

        public async Task<List<Purchase>> GetSupplierPurchasesAsync(int supplierId)
        {
            const string query = @"
                SELECT Id, PurchaseNumber, SupplierId, Date, SubTotal, Tax, Total, 
                       PaymentStatus, AmountPaid, Notes, UserId, CreatedAt
                FROM Purchases
                WHERE SupplierId = @supplierId
                ORDER BY Date DESC
            ";

            return await _db.ExecuteQueryAsync(query, reader => new Purchase
            {
                Id = reader.GetInt32(0),
                PurchaseNumber = reader.GetString(1),
                SupplierId = reader.GetInt32(2),
                Date = DateTime.Parse(reader.GetString(3)),
                SubTotal = (decimal)reader.GetDouble(4),
                Tax = (decimal)reader.GetDouble(5),
                Total = (decimal)reader.GetDouble(6),
                PaymentStatus = reader.GetString(7),
                AmountPaid = (decimal)reader.GetDouble(8),
                Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                UserId = reader.GetInt32(10),
                CreatedAt = DateTime.Parse(reader.GetString(11))
            }, DatabaseService.CreateParameter("@supplierId", supplierId));
        }

        public async Task<decimal> GetSupplierTotalPurchasesAsync(int supplierId)
        {
            const string query = @"
                SELECT COALESCE(SUM(Total), 0) 
                FROM Purchases 
                WHERE SupplierId = @supplierId
            ";

            var result = await _db.ExecuteScalarAsync(query,
                DatabaseService.CreateParameter("@supplierId", supplierId)
            );

            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public async Task<decimal> GetSupplierOutstandingBalanceAsync(int supplierId)
        {
            const string query = @"
                SELECT COALESCE(SUM(Total - AmountPaid), 0) 
                FROM Purchases 
                WHERE SupplierId = @supplierId 
                AND PaymentStatus != 'Paid'
            ";

            var result = await _db.ExecuteScalarAsync(query,
                DatabaseService.CreateParameter("@supplierId", supplierId)
            );

            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        private static Supplier MapSupplier(SqliteDataReader reader)
        {
            return new Supplier
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = DateTime.Parse(reader.GetString(5)),
                IsActive = reader.GetInt32(6) == 1
            };
        }
    }
}
