namespace MyPOS99.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int StockQty { get; set; }
        public int MinStockLevel { get; set; }
        public string? Barcode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public decimal Profit => SellPrice - CostPrice;
        public decimal ProfitMargin => CostPrice > 0 ? ((SellPrice - CostPrice) / CostPrice) * 100 : 0;
        public bool IsLowStock => StockQty <= MinStockLevel;
    }
}
