using MyPOS99.Data;
using MyPOS99.Models;
using Microsoft.Data.Sqlite;

namespace MyPOS99.Services
{
    public class ExpenseService
    {
        private readonly DatabaseContext _dbContext;

        public ExpenseService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddExpenseAsync(Expense expense)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Expenses (Category, Amount, Date, Note, PaymentMethod, UserId)
                VALUES (@category, @amount, @date, @note, @paymentMethod, @userId)
            ";
            command.Parameters.AddWithValue("@category", expense.Category);
            command.Parameters.AddWithValue("@amount", (double)expense.Amount);
            command.Parameters.AddWithValue("@date", expense.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@note", expense.Note ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@paymentMethod", expense.PaymentMethod ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@userId", expense.UserId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Expense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var expenses = new List<Expense>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Category, Amount, Date, Note, PaymentMethod, UserId, CreatedAt
                FROM Expenses
                WHERE DATE(Date) BETWEEN DATE(@startDate) AND DATE(@endDate)
                ORDER BY Date DESC
            ";
            command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                expenses.Add(new Expense
                {
                    Id = reader.GetInt32(0),
                    Category = reader.GetString(1),
                    Amount = (decimal)reader.GetDouble(2),
                    Date = DateTime.Parse(reader.GetString(3)),
                    Note = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PaymentMethod = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UserId = reader.GetInt32(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7))
                });
            }

            return expenses;
        }

        public async Task<decimal> GetTotalExpensesByCategoryAsync(string category, DateTime startDate, DateTime endDate)
        {
            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(Amount), 0)
                FROM Expenses
                WHERE Category = @category 
                AND DATE(Date) BETWEEN DATE(@startDate) AND DATE(@endDate)
            ";
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            var result = await command.ExecuteScalarAsync();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime startDate, DateTime endDate)
        {
            var expenses = new Dictionary<string, decimal>();

            using var connection = _dbContext.GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Category, SUM(Amount) as Total
                FROM Expenses
                WHERE DATE(Date) BETWEEN DATE(@startDate) AND DATE(@endDate)
                GROUP BY Category
                ORDER BY Total DESC
            ";
            command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                expenses[reader.GetString(0)] = (decimal)reader.GetDouble(1);
            }

            return expenses;
        }
    }
}
