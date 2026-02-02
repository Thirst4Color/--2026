namespace VideoRentalSystem.Models.Entities
{
    public class MediaItem
    {
        public int MediaItemId { get; set; }
        public int MovieId { get; set; }
        public int MediaTypeId { get; set; }
        public int? SupplierId { get; set; } // Делаем nullable
        public string Barcode { get; set; }
        public System.DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public string Condition { get; set; }

        public Movie Movie { get; set; }
        public MediaType MediaType { get; set; }
        public Supplier Supplier { get; set; } // Добавляем навигационное свойство
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
