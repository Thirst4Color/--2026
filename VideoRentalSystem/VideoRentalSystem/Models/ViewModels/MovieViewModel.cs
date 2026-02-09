using System.Collections.Generic;
using VideoRentalSystem.Models.Entities;

namespace VideoRentalSystem.Models.ViewModels
{
    public class MovieViewModel
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

        // Информация о доступности
        public bool IsAvailable { get; set; }
        public int AvailableCopies { get; set; }
        public List<string> AvailableFormats { get; set; } = new List<string>();

        // Добавляем недостающие свойства
        public DateTime? NextAvailableDate { get; set; }  // Добавлено
        public List<MediaItem> MediaItems { get; set; } = new List<MediaItem>();

        // Добавляем список MediaItemInfo
        public List<MediaItemInfo> MediaItemInfos { get; set; } = new List<MediaItemInfo>();

        public Dictionary<string, decimal> FormatPrices { get; set; } = new Dictionary<string, decimal>();
    }
}