namespace VideoRentalSystem.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }  // Исправлено: TotalAmount, а не TotalPrice
        public string Status { get; set; } = "Pending";  // Строка, а не enum
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public int? EmployeeId { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Навигационные свойства
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Вычисляемое свойство для обратной совместимости
        public decimal TotalPrice => TotalAmount;
    }
}