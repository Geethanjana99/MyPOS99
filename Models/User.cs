namespace MyPOS99.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Cashier, Manager
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public enum UserRole
    {
        Admin,
        Cashier,
        Manager
    }
}
