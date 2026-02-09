namespace VideoRentalSystem.Models.Entities
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int MediaItemId { get; set; }
        public int RentalDays { get; set; }
        public DateTime DesiredPickupDate { get; set; }
        public DateTime AddedDate { get; set; }

        // Добавляем свойство Quantity
        public int Quantity { get; set; } = 1;

        // Навигационные свойства
        public MediaItem MediaItem { get; set; } = null!;

        // Вычисляемое свойство для общей цены
        public decimal TotalPrice
        {
            get
            {
                if (MediaItem?.MediaType == null) return 0;
                return MediaItem.MediaType.DailyRentalPrice * RentalDays * Quantity;
            }
        }
    }
}