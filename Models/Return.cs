using System;

namespace MyPOS99.Models
{
    public class Return
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public string OriginalInvoiceNumber { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int ProcessedByUserId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReturnItem
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
