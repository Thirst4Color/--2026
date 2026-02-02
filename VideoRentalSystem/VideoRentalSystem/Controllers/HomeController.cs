using Microsoft.AspNetCore.Mvc;

namespace VideoRentalSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}