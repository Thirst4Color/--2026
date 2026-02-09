using System.Collections.Generic;

namespace VideoRentalSystem.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public int? Duration { get; set; }
        public decimal? Rating { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
        public int AvailableCopies { get; set; }
        public List<string> AvailableFormats { get; set; } = new List<string>();
        public Dictionary<string, decimal> FormatPrices { get; set; } = new Dictionary<string, decimal>();

        // Добавьте это свойство
        public DateTime? NextAvailableDate { get; set; }

        public List<MediaItemInfo> MediaItems { get; set; } = new List<MediaItemInfo>();
    }
}