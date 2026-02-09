using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;

namespace VideoRentalSystem.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartSummaryViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Получаем ID корзины из сессии
            var cartId = HttpContext.Session.GetString("CartId");
            int itemCount = 0;

            if (!string.IsNullOrEmpty(cartId))
            {
                // Считаем количество товаров в корзине
                itemCount = _context.ShoppingCartItems
                    .Count(sci => sci.SessionId == cartId);
            }

            return View(itemCount);
        }
    }
}