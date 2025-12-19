using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class UserService
    {
        private readonly DatabaseContext _dbContext;

        public UserService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            // Note: In production, use proper password hashing (e.g., BCrypt)
            var user = await GetUserByUsernameAsync(username);
            
            if (user != null && user.IsActive)
            {
                // TODO: Implement proper password verification with BCrypt or similar
                // For now, this is a placeholder
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Username, PasswordHash, Role, CreatedAt, IsActive FROM Users WHERE Username = @username";
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                    IsActive = reader.GetInt32(5) == 1
                };
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Username, PasswordHash, Role, CreatedAt, IsActive FROM Users ORDER BY Username";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                    IsActive = reader.GetInt32(5) == 1
                });
            }

            return users;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Username, PasswordHash, Role, IsActive)
                VALUES (@username, @passwordHash, @role, @isActive)
            ";
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET Username = @username, Role = @role, IsActive = @isActive
                WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET PasswordHash = @passwordHash WHERE Id = @id";
            command.Parameters.AddWithValue("@id", userId);
            command.Parameters.AddWithValue("@passwordHash", newPasswordHash);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
