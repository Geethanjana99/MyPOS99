namespace MyPOS99.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentType { get; set; } = string.Empty; // Cash, Card, Mobile, Credit
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public int UserId { get; set; }
        public int? CustomerId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Customer? Customer { get; set; }
        public List<SaleItem> Items { get; set; } = new();
    }

    public enum PaymentType
    {
        Cash,
        Card,
        Mobile,
        Credit
    }
}
