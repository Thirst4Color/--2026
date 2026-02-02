namespace VideoRentalSystem.Models.Entities
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int MediaItemId { get; set; }
        public int RentalDays { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal LateFee { get; set; }
        public string Status { get; set; }

        public Order Order { get; set; }
        public MediaItem MediaItem { get; set; }
    }
}
