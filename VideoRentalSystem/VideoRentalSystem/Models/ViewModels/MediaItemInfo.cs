namespace VideoRentalSystem.Models.ViewModels
{
    public class MediaItemInfo
    {
        public int MediaItemId { get; set; }
        public string Format { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal DailyPrice { get; set; }
        public string Barcode { get; set; } = string.Empty;
    }
}