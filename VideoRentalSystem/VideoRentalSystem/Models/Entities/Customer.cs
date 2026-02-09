namespace VideoRentalSystem.Models.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public bool IsBlocked { get; set; }
        public string BarcodeId { get; set; } = string.Empty;
        public int LoyaltyPoints { get; set; }

        // Навигационные свойства
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        // Вычисляемое свойство для полного имени
        public string FullName => $"{FirstName} {LastName}";
    }
}