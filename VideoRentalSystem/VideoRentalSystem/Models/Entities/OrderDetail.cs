namespace VideoRentalSystem.Models.Entities
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int MediaItemId { get; set; }
        public int RentalDays { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal Subtotal { get; set; }  // Исправлено: Subtotal, а не TotalPrice
        public decimal LateFee { get; set; }
        public string Status { get; set; } = "Active";

        // Навигационные свойства
        public Order Order { get; set; } = null!;
        public MediaItem MediaItem { get; set; } = null!;

        // Вычисляемое свойство для обратной совместимости
        public decimal TotalPrice => Subtotal;
    }
}