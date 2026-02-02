namespace VideoRentalSystem.Models.Entities
{
    public class MediaType
    {
        public int MediaTypeId { get; set; }
        public string Name { get; set; } // "VHS", "DVD", "Blu-Ray", "HD-DVD"
        public decimal DailyRentalPrice { get; set; }

        public ICollection<MediaItem> MediaItems { get; set; }
    }
}
