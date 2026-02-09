using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models.ViewModels;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.Entities;
using System.Linq;
using System.Collections.Generic;

namespace VideoRentalSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получить ID корзины
        private string GetCartId()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
            }
            return cartId;
        }

        // Просмотр корзины
        public IActionResult Index()
        {
            var cartId = GetCartId();
            var cartItems = _context.ShoppingCartItems
                .Include(ci => ci.MediaItem)
                    .ThenInclude(mi => mi.MediaType)
                .Include(ci => ci.MediaItem.Movie)
                .Where(ci => ci.SessionId == cartId)
                .ToList();

            var cart = new Cart
            {
                CartId = cartId,
                Items = cartItems
            };

            return View(cart);
        }

        // Добавить в корзину - УПРОЩЕННАЯ ВЕРСИЯ
        [HttpPost]
        public IActionResult AddToCart(int mediaItemId)
        {
            try
            {
                var cartId = GetCartId();

                // Проверяем доступность
                var mediaItem = _context.MediaItems
                    .Include(m => m.Movie)
                    .Include(m => m.MediaType)
                    .FirstOrDefault(m => m.MediaItemId == mediaItemId && m.IsAvailable);

                if (mediaItem == null)
                {
                    TempData["ErrorMessage"] = "Фильм недоступен";
                    return RedirectToAction("Index", "Catalog");
                }

                // Проверяем, есть ли уже в корзине
                var existingItem = _context.ShoppingCartItems
                    .FirstOrDefault(ci => ci.SessionId == cartId && ci.MediaItemId == mediaItemId);

                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    var cartItem = new ShoppingCartItem
                    {
                        SessionId = cartId,
                        MediaItemId = mediaItemId,
                        RentalDays = 1,
                        Quantity = 1,
                        DesiredPickupDate = DateTime.Now.AddDays(1),
                        AddedDate = DateTime.Now
                    };

                    _context.ShoppingCartItems.Add(cartItem);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = $"'{mediaItem.Movie.Title}' добавлен в корзину";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                return RedirectToAction("Index", "Catalog");
            }
        }

        // Удалить из корзины
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var item = _context.ShoppingCartItems.Find(id);
            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Удалено из корзины";
            }

            return RedirectToAction("Index");
        }

        // Обновить количество дней аренды
        [HttpPost]
        public IActionResult UpdateRentalDays(int id, int rentalDays)
        {
            var item = _context.ShoppingCartItems.Find(id);
            if (item != null && rentalDays > 0)
            {
                item.RentalDays = rentalDays;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Очистить корзину
        [HttpPost]
        public IActionResult ClearCart()
        {
            var cartId = GetCartId();
            var items = _context.ShoppingCartItems.Where(ci => ci.SessionId == cartId);

            _context.ShoppingCartItems.RemoveRange(items);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Корзина очищена";
            return RedirectToAction("Index");
        }

        // Оформить заказ
        public IActionResult Checkout()
        {
            var cartId = GetCartId();
            var items = _context.ShoppingCartItems
                .Include(ci => ci.MediaItem)
                    .ThenInclude(mi => mi.Movie)
                .Where(ci => ci.SessionId == cartId)
                .ToList();

            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Корзина пуста";
                return RedirectToAction("Index");
            }

            var cart = new Cart
            {
                CartId = cartId,
                Items = items
            };

            return View(cart);
        }

        // Создать заказ - ИСПРАВЛЕННАЯ ВЕРСИЯ
        [HttpPost]
        public IActionResult CreateOrder(string firstName, string lastName, string phone, string email)
        {
            try
            {
                var cartId = GetCartId();
                var cartItems = _context.ShoppingCartItems
                    .Include(ci => ci.MediaItem)
                        .ThenInclude(mi => mi.MediaType)
                    .Include(ci => ci.MediaItem.Movie)
                    .Where(ci => ci.SessionId == cartId)
                    .ToList();

                if (!cartItems.Any())
                {
                    TempData["ErrorMessage"] = "Корзина пуста";
                    return RedirectToAction("Index");
                }

                // Создаем или находим клиента
                var customer = _context.Customers
                    .FirstOrDefault(c => c.Email == email) ?? new Customer
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Phone = phone,
                        Email = email,
                        Address = "Не указан",
                        RegistrationDate = DateTime.Now,
                        IsBlocked = false,
                        BarcodeId = Guid.NewGuid().ToString().Substring(0, 8),
                        LoyaltyPoints = 0
                    };

                if (customer.CustomerId == 0)
                {
                    _context.Customers.Add(customer);
                    _context.SaveChanges();
                }

                // Создаем заказ
                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    Status = "Pending",
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(ci => ci.TotalPrice),
                    Notes = "Онлайн заказ"
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Добавляем детали заказа
                foreach (var cartItem in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        MediaItemId = cartItem.MediaItemId,
                        RentalDays = cartItem.RentalDays,
                        DailyPrice = cartItem.MediaItem.MediaType.DailyRentalPrice,
                        Subtotal = cartItem.TotalPrice,
                        LateFee = 0,
                        Status = "Active"
                    };

                    _context.OrderDetails.Add(orderDetail);

                    // Делаем носитель недоступным
                    cartItem.MediaItem.IsAvailable = false;
                }

                // Очищаем корзину
                _context.ShoppingCartItems.RemoveRange(cartItems);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Заказ #{order.OrderId} создан!";
                return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        // Подтверждение заказа
        public IActionResult OrderConfirmation(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MediaItem)
                        .ThenInclude(mi => mi.Movie)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}