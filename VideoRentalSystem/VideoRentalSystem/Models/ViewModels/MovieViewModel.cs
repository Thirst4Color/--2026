namespace VideoRentalSystem.Models.ViewModels
{
    public class MovieViewModel
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

        // Информация о доступности
        public bool IsAvailable { get; set; }
        public int AvailableCopies { get; set; }
        public List<string> AvailableFormats { get; set; }
        public DateTime? NextAvailableDate { get; set; }

        // Цены по форматам
        public Dictionary<string, decimal> FormatPrices { get; set; }
    }

    public class MovieDetailsViewModel : MovieViewModel
    {
        public List<MediaItemInfo> MediaItems { get; set; }
    }

    public class MediaItemInfo
    {
        public int MediaItemId { get; set; }
        public string Format { get; set; }
        public string Condition { get; set; }
        public bool IsAvailable { get; set; }
        public decimal DailyPrice { get; set; }
    }
}