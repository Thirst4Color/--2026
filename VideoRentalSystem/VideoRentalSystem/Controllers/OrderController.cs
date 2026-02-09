using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.ViewModels;
using System.Linq;

namespace VideoRentalSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Список всех заказов
        public IActionResult Index(string status = "")
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewData["StatusFilter"] = status;
            return View(orders);
        }

        // Детали заказа
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MediaItem)
                        .ThenInclude(mi => mi.Movie)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MediaItem)
                        .ThenInclude(mi => mi.MediaType)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Подтвердить заказ (выдать в аренду)
        [HttpPost]
        public IActionResult ConfirmOrder(int id, int rentalDays)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Confirmed";  // Исправлено
            order.PickupDate = DateTime.Now;
            order.ReturnDueDate = DateTime.Now.AddDays(rentalDays);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Заказ подтвержден и выдан в аренду";
            return RedirectToAction("Details", new { id });
        }

        // Отметить как возвращенный
        [HttpPost]
        public IActionResult MarkAsReturned(int id, string condition, decimal? damageFee = null)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MediaItem)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Returned";  // Исправлено
            order.ActualReturnDate = DateTime.Now;

            // Рассчитываем штраф за просрочку
            if (order.ReturnDueDate.HasValue && order.ActualReturnDate > order.ReturnDueDate)
            {
                int daysLate = (order.ActualReturnDate.Value - order.ReturnDueDate.Value).Days;
                decimal lateFeePerDay = 5.00m;

                // Обновляем LateFee в OrderDetails
                foreach (var item in order.OrderDetails)
                {
                    item.LateFee = daysLate * lateFeePerDay;
                    item.Status = "Returned";
                }
            }

            // Обновляем каждый элемент заказа
            foreach (var item in order.OrderDetails)
            {
                // Возвращаем носитель в доступные
                item.MediaItem.IsAvailable = true;
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Фильмы возвращены в прокат";
            return RedirectToAction("Details", new { id });
        }

        // Отменить заказ
        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MediaItem)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Возвращаем все носители в доступные
            foreach (var item in order.OrderDetails)
            {
                item.MediaItem.IsAvailable = true;
                item.Status = "Cancelled";
            }

            order.Status = "Cancelled";  // Исправлено
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Заказ отменен";
            return RedirectToAction("Details", new { id });
        }

        // Отметить как выданный
        [HttpPost]
        public IActionResult MarkAsRented(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Rented";  // Исправлено

            foreach (var item in order.OrderDetails)
            {
                item.Status = "Rented";
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Заказ отмечен как выданный";
            return RedirectToAction("Details", new { id });
        }
    }
}