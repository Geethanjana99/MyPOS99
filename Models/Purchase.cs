namespace MyPOS99.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public string PurchaseNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public DateTime Date { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid
        public decimal AmountPaid { get; set; }
        public string? Notes { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Supplier? Supplier { get; set; }
        public User? User { get; set; }
        public List<PurchaseItem> Items { get; set; } = new();

        // Computed properties
        public decimal AmountDue => Total - AmountPaid;
        public bool IsPaid => AmountPaid >= Total;
    }

    public enum PurchasePaymentStatus
    {
        Pending,
        Partial,
        Paid
    }
}
