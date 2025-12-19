namespace MyPOS99.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Total { get; set; }

        // Navigation properties
        public Purchase? Purchase { get; set; }
        public Product? Product { get; set; }

        // Computed property
        public decimal SubTotal => Qty * CostPrice;
    }
}
