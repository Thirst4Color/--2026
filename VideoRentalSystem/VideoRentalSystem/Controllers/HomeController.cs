using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using System.Linq;

namespace VideoRentalSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Популярные фильмы
            var popularMovies = _context.Movies
                .Include(m => m.MediaItems)
                .ThenInclude(mi => mi.MediaType)
                .OrderByDescending(m => m.Rating)
                .Take(6)
                .Select(m => new
                {
                    m.MovieId,
                    m.Title,
                    m.Rating,
                    m.Genre,
                    m.Year,
                    CoverImageUrl = string.IsNullOrEmpty(m.CoverImageUrl) ?
                        "/images/default-movie.jpg" : m.CoverImageUrl,
                    IsAvailable = m.MediaItems.Any(mi => mi.IsAvailable)
                })
                .ToList();

            // Новые поступления
            var newArrivals = _context.MediaItems
                .Include(mi => mi.Movie)
                .Include(mi => mi.MediaType)
                .OrderByDescending(mi => mi.PurchaseDate)
                .Take(8)
                .Select(mi => new
                {
                    mi.Movie.MovieId,
                    mi.Movie.Title,
                    mi.MediaType.Name,
                    mi.PurchaseDate,
                    mi.Movie.CoverImageUrl
                })
                .ToList();

            // Статистика
            var stats = new
            {
                TotalMovies = _context.Movies.Count(),
                AvailableCopies = _context.MediaItems.Count(mi => mi.IsAvailable),
                TotalFormats = _context.MediaTypes.Count()
            };

            ViewData["PopularMovies"] = popularMovies;
            ViewData["NewArrivals"] = newArrivals;
            ViewData["Stats"] = stats;
            ViewData["Title"] = "Главная - Видеопрокат ФильмоМир";

            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "О нас";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Контакты";
            return View();
        }
    }
}