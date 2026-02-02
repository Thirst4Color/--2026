namespace VideoRentalSystem.Models.Entities
{
    public class ShoppingCartItem
    {
        public int MediaItemId { get; set; }
        public int RentalDays { get; set; }
        public DateTime DesiredPickupDate { get; set; }
    }
}
