using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.ViewModels;
using System.Linq;
using System.Collections.Generic;

namespace VideoRentalSystem.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главная страница каталога
        public IActionResult Index(string search = "", string genre = "", string format = "")
        {
            var query = _context.Movies
                .Include(m => m.MediaItems)
                .ThenInclude(mi => mi.MediaType)
                .AsQueryable();

            // Фильтрация по поиску
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(search) ||
                    m.Description.ToLower().Contains(search) ||
                    m.Director.ToLower().Contains(search) ||
                    m.Genre.ToLower().Contains(search));
            }

            // Фильтрация по жанру
            if (!string.IsNullOrEmpty(genre))
            {
                query = query.Where(m => m.Genre.Contains(genre));
            }

            var movies = query.ToList();

            // Преобразуем в ViewModel
            var movieViewModels = movies.Select(m => new MovieViewModel
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description?.Length > 150 ? m.Description.Substring(0, 150) + "..." : m.Description,
                Year = m.Year,
                Genre = m.Genre,
                Director = m.Director,
                Duration = m.Duration,
                Rating = m.Rating,
                CoverImageUrl = string.IsNullOrEmpty(m.CoverImageUrl) ? "/images/default-movie.jpg" : m.CoverImageUrl,

                // Информация о доступности
                IsAvailable = m.MediaItems.Any(mi => mi.IsAvailable),
                AvailableCopies = m.MediaItems.Count(mi => mi.IsAvailable),
                AvailableFormats = m.MediaItems
                    .Where(mi => mi.IsAvailable)
                    .Select(mi => mi.MediaType.Name)
                    .Distinct()
                    .ToList(),
                NextAvailableDate = m.MediaItems.All(mi => !mi.IsAvailable) && m.MediaItems.Any() ?
    _context.OrderDetails
        .Include(od => od.Order)
        .Where(od => od.MediaItem.MovieId == m.MovieId && od.Status == "Rented")
        .Select(od => od.Order.ReturnDueDate)
        .OrderBy(d => d)
        .FirstOrDefault() : null,

                // Цены по форматам
                FormatPrices = m.MediaItems
                    .GroupBy(mi => mi.MediaType.Name)
                    .ToDictionary(g => g.Key, g => g.First().MediaType.DailyRentalPrice)
            }).ToList();

            ViewData["Title"] = "Каталог фильмов";
            ViewData["Search"] = search;
            ViewData["Genres"] = GetAvailableGenres();
            ViewData["Formats"] = GetAvailableFormats();

            return View(movieViewModels);
        }

        // Детальная страница фильма
        public IActionResult Details(int id)
        {
            var movie = _context.Movies
                .Include(m => m.MediaItems)
                    .ThenInclude(mi => mi.MediaType)
                .Include(m => m.MediaItems)
                    .ThenInclude(mi => mi.OrderDetails)
                        .ThenInclude(od => od.Order)
                .FirstOrDefault(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            var viewModel = new MovieDetailsViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Year = movie.Year,
                Genre = movie.Genre,
                Director = movie.Director,
                Duration = movie.Duration,
                Rating = movie.Rating,
                CoverImageUrl = string.IsNullOrEmpty(movie.CoverImageUrl) ? "/images/default-movie.jpg" : movie.CoverImageUrl,

                IsAvailable = movie.MediaItems.Any(mi => mi.IsAvailable),
                AvailableCopies = movie.MediaItems.Count(mi => mi.IsAvailable),
                AvailableFormats = movie.MediaItems
                    .Where(mi => mi.IsAvailable)
                    .Select(mi => mi.MediaType.Name)
                    .Distinct()
                    .ToList(),
                NextAvailableDate = movie.MediaItems.All(mi => !mi.IsAvailable) && movie.MediaItems.Any() ?
                    movie.MediaItems
                        .Where(mi => mi.OrderDetails.Any())
                        .SelectMany(mi => mi.OrderDetails)
                        .Where(od => od.Status == "Rented")
                        .Select(od => od.Order.ReturnDueDate)
                        .OrderBy(d => d)
                        .FirstOrDefault() : null,

                FormatPrices = movie.MediaItems
                    .GroupBy(mi => mi.MediaType.Name)
                    .ToDictionary(g => g.Key, g => g.First().MediaType.DailyRentalPrice),

                MediaItems = movie.MediaItems.Select(mi => new MediaItemInfo
                {
                    MediaItemId = mi.MediaItemId,
                    Format = mi.MediaType.Name,
                    Condition = mi.Condition,
                    IsAvailable = mi.IsAvailable,
                    DailyPrice = mi.MediaType.DailyRentalPrice
                }).ToList()
            };

            ViewData["Title"] = movie.Title;
            return View(viewModel);
        }

        // Поиск фильмов (AJAX)
        [HttpGet]
        public IActionResult Search(string term)
        {
            var movies = _context.Movies
                .Where(m => m.Title.ToLower().Contains(term.ToLower()))
                .Select(m => new {
                    id = m.MovieId,
                    text = m.Title,
                    year = m.Year,
                    genre = m.Genre
                })
                .Take(10)
                .ToList();

            return Json(movies);
        }

        // Получение списка жанров
        [HttpGet]
        public IActionResult GetGenres()
        {
            var genres = _context.Movies
                .Select(m => m.Genre)
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            return Json(genres);
        }

        private List<string> GetAvailableGenres()
        {
            return _context.Movies
                .Select(m => m.Genre)
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        private List<string> GetAvailableFormats()
        {
            return _context.MediaTypes
                .Select(mt => mt.Name)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }
    }
}