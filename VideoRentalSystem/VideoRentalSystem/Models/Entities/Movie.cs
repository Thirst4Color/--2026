namespace VideoRentalSystem.Models.Entities
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public int? Duration { get; set; }
        public decimal? Rating { get; set; }
        public string CoverImageUrl { get; set; }

        public ICollection<MediaItem> MediaItems { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
    }
}
