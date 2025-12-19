using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    /// <summary>
    /// Example service demonstrating DatabaseService usage with parameterized queries
    /// </summary>
    public class CategoryService
    {
        private readonly DatabaseService _db;

        public CategoryService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            const string query = "SELECT Id, Name, Description, CreatedAt FROM Categories ORDER BY Name";

            return await _db.ExecuteQueryAsync(query, reader => new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3))
            });
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            const string query = "SELECT Id, Name, Description, CreatedAt FROM Categories WHERE Id = @id";

            return await _db.ExecuteQuerySingleAsync(query, reader => new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3))
            }, DatabaseService.CreateParameter("@id", id));
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            const string query = "SELECT Id, Name, Description, CreatedAt FROM Categories WHERE Name = @name";

            return await _db.ExecuteQuerySingleAsync(query, reader => new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3))
            }, DatabaseService.CreateParameter("@name", name));
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            const string query = @"
                INSERT INTO Categories (Name, Description)
                VALUES (@name, @description)
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@name", category.Name),
                DatabaseService.CreateParameter("@description", category.Description)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            const string query = @"
                UPDATE Categories 
                SET Name = @name, Description = @description
                WHERE Id = @id
            ";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", category.Id),
                DatabaseService.CreateParameter("@name", category.Name),
                DatabaseService.CreateParameter("@description", category.Description)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            const string query = "DELETE FROM Categories WHERE Id = @id";

            var rowsAffected = await _db.ExecuteNonQueryAsync(query,
                DatabaseService.CreateParameter("@id", id)
            );

            return rowsAffected > 0;
        }

        public async Task<bool> CategoryExistsAsync(string name)
        {
            const string query = "SELECT COUNT(*) FROM Categories WHERE Name = @name";

            var result = await _db.ExecuteScalarAsync(query,
                DatabaseService.CreateParameter("@name", name)
            );

            return Convert.ToInt32(result) > 0;
        }

        public async Task<int> GetProductCountByCategoryAsync(string categoryName)
        {
            const string query = "SELECT COUNT(*) FROM Products WHERE Category = @category";

            var result = await _db.ExecuteScalarAsync(query,
                DatabaseService.CreateParameter("@category", categoryName)
            );

            return Convert.ToInt32(result ?? 0);
        }
    }
}
