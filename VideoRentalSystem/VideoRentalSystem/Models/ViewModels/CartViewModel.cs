using System;
using System.Collections.Generic;

namespace VideoRentalSystem.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartItemViewModel
    {
        public int ShoppingCartItemId { get; set; }
        public int MediaItemId { get; set; }
        public string MovieTitle { get; set; }
        public string Format { get; set; }
        public int RentalDays { get; set; }
        public decimal DailyPrice { get; set; }
        public DateTime DesiredPickupDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}