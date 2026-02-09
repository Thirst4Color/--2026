using System.Collections.Generic;
using System.Linq;
using VideoRentalSystem.Models.Entities;

namespace VideoRentalSystem.Models
{
    public class Cart
    {
        public string CartId { get; set; } = string.Empty;
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
        public decimal TotalPrice => Items?.Sum(i => i.TotalPrice) ?? 0;
    }
}