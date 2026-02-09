using System;
using System.Collections.Generic;

namespace VideoRentalSystem.Models.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public decimal LateFee { get; set; }

        public List<OrderDetailViewModel> Details { get; set; } = new List<OrderDetailViewModel>();
    }

    public class OrderDetailViewModel
    {
        public string MovieTitle { get; set; }
        public string Format { get; set; }
        public int RentalDays { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string Status { get; set; }
    }
}