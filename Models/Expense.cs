namespace MyPOS99.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public string? PaymentMethod { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
