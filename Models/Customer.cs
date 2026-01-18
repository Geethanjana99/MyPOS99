namespace MyPOS99.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentCredit { get; set; }
        public bool IsCreditCustomer { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Computed property for display
        public string DisplayName => Id == 1 ? "Walk-in Customer" : Name;
        public decimal AvailableCredit => CreditLimit - CurrentCredit;
        public string CreditStatus => IsCreditCustomer ? $"Credit: Rs. {AvailableCredit:N2} / Rs. {CreditLimit:N2}" : "Cash Customer";
    }
}
