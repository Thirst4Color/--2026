namespace VideoRentalSystem.Models.Entities
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public int CustomerId { get; set; }
        public int MovieId { get; set; }
        public int? MediaTypeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public bool NotificationSent { get; set; }

        public Customer Customer { get; set; }
        public Movie Movie { get; set; }
        public MediaType MediaType { get; set; }
    }
}
