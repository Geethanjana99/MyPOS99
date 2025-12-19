namespace MyPOS99.Models
{
    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        // Navigation properties
        public Sale? Sale { get; set; }
        public Product? Product { get; set; }

        // Computed property
        public decimal SubTotal => Qty * Price;
    }
}
