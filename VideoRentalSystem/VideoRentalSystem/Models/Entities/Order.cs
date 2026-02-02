namespace VideoRentalSystem.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Pending, Reserved, Rented, Returned, Cancelled
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public int? EmployeeId { get; set; }

        public Customer Customer { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
