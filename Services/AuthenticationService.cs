using MyPOS99.Data;
using MyPOS99.Models;
using BCrypt.Net;

namespace MyPOS99.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseService _db;
        private User? _currentUser;

        public AuthenticationService(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        public User? CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        public bool IsAuthenticated => _currentUser != null;

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password are required.", null);
            }

            const string query = @"
                SELECT Id, Username, PasswordHash, Role, CreatedAt, IsActive 
                FROM Users 
                WHERE Username = @username AND IsActive = 1
            ";

            var user = await _db.ExecuteQuerySingleAsync(query, reader => new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = reader.GetString(3),
                CreatedAt = DateTime.Parse(reader.GetString(4)),
                IsActive = reader.GetInt32(5) == 1
            }, DatabaseService.CreateParameter("@username", username));

            if (user == null)
            {
                return (false, "Invalid username or password.", null);
            }

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return (false, "Invalid username or password.", null);
            }

            CurrentUser = user;
            return (true, "Login successful!", user);
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            if (CurrentUser == null || CurrentUser.Id != userId)
            {
                return false;
            }

            const string query = "SELECT PasswordHash FROM Users WHERE Id = @userId";
            var currentHash = await _db.ExecuteScalarAsync(query,
                DatabaseService.CreateParameter("@userId", userId));

            if (currentHash == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, currentHash.ToString()!))
            {
                return false;
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            const string updateQuery = "UPDATE Users SET PasswordHash = @hash WHERE Id = @userId";

            var rowsAffected = await _db.ExecuteNonQueryAsync(updateQuery,
                DatabaseService.CreateParameter("@hash", newHash),
                DatabaseService.CreateParameter("@userId", userId)
            );

            return rowsAffected > 0;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool HasRole(params string[] roles)
        {
            return CurrentUser != null && roles.Contains(CurrentUser.Role);
        }
    }
}
