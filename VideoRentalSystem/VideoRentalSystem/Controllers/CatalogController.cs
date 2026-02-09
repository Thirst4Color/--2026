using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.Entities;
using VideoRentalSystem.Models.ViewModels;

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
            Console.WriteLine($"=== DEBUG Catalog/Index ===");
            Console.WriteLine($"Параметр search: '{search}'");
            Console.WriteLine($"QueryString: {HttpContext.Request.QueryString}");

            try
            {
                var query = _context.Movies
                    .Include(m => m.MediaItems)
                        .ThenInclude(mi => mi.MediaType)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    Console.WriteLine($"Применяем фильтр поиска: '{search}'");

                    query = query.Where(m =>
                        (m.Title != null && m.Title.ToLower().Contains(search)) ||
                        (m.Description != null && m.Description.ToLower().Contains(search)) ||
                        (m.Director != null && m.Director.ToLower().Contains(search)) ||
                        (m.Genre != null && m.Genre.ToLower().Contains(search)));

                    Console.WriteLine($"Фильмовий поиск: {query.Count()}");
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    query = query.Where(m => m.Genre != null && m.Genre.Contains(genre));
                }

                if (!string.IsNullOrEmpty(format))
                {
                    query = query.Where(m => m.MediaItems.Any(mi =>
                        mi.MediaType != null &&
                        mi.MediaType.Name != null &&
                        mi.MediaType.Name == format &&
                        mi.IsAvailable == true));
                }

                var movies = query.ToList();

                // ИСПРАВЛЕННЫЙ КОД - ТОЛЬКО "m", НИКАКИХ "movie"
                var movieViewModels = movies.Select(m => new MovieViewModel
                {
                    MovieId = m.MovieId,
                    Title = m.Title ?? "Без названия",
                    Description = m.Description,
                    Year = m.Year,
                    Genre = m.Genre ?? "Не указан",
                    Director = m.Director ?? "Не указан",
                    Duration = m.Duration,
                    Rating = m.Rating,
                    CoverImageUrl = string.IsNullOrEmpty(m.CoverImageUrl) ?
                        "/images/default-movie.jpg" : m.CoverImageUrl,

                    IsAvailable = m.MediaItems != null && m.MediaItems.Any(mi => mi.IsAvailable),
                    AvailableCopies = m.MediaItems?.Count(mi => mi.IsAvailable) ?? 0,
                    AvailableFormats = m.MediaItems?
                        .Where(mi => mi.IsAvailable && mi.MediaType != null && !string.IsNullOrEmpty(mi.MediaType.Name))
                        .Select(mi => mi.MediaType.Name)
                        .Distinct()
                        .ToList() ?? new List<string>(),

                    NextAvailableDate = null,

                    MediaItems = m.MediaItems?.Where(mi => mi.IsAvailable).ToList() ?? new List<MediaItem>(),

                    MediaItemInfos = m.MediaItems?
                        .Where(mi => mi.MediaType != null)
                        .Select(mi => new MediaItemInfo
                        {
                            MediaItemId = mi.MediaItemId,
                            Format = mi.MediaType.Name ?? "Неизвестно",
                            Condition = mi.Condition ?? "Не указано",
                            IsAvailable = mi.IsAvailable,
                            DailyPrice = mi.MediaType.DailyRentalPrice,
                            Barcode = mi.Barcode ?? ""
                        }).ToList() ?? new List<MediaItemInfo>(),

                    FormatPrices = m.MediaItems?
                        .Where(mi => mi.MediaType != null && !string.IsNullOrEmpty(mi.MediaType.Name))
                        .GroupBy(mi => mi.MediaType.Name)
                        .ToDictionary(g => g.Key, g => g.First().MediaType.DailyRentalPrice)
                        ?? new Dictionary<string, decimal>()
                }).ToList();

                ViewData["Title"] = "Каталог фильмов";
                ViewData["Search"] = search;
                ViewData["SelectedGenre"] = genre;
                ViewData["SelectedFormat"] = format;
                ViewData["Genres"] = GetAvailableGenres();
                ViewData["Formats"] = GetAvailableFormats();

                return View(movieViewModels);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка в каталоге: {ex.Message}");
                return View(new List<MovieViewModel>());
            }
        }

        // Детальная страница фильма
        public IActionResult Details(int id)
        {
            try
            {
                var movie = _context.Movies
                    .Include(m => m.MediaItems)
                        .ThenInclude(mi => mi.MediaType)
                    .FirstOrDefault(m => m.MovieId == id);

                if (movie == null)
                {
                    return NotFound();
                }

                var viewModel = new MovieDetailsViewModel
                {
                    MovieId = movie.MovieId,
                    Title = movie.Title ?? "Без названия",
                    Description = movie.Description,
                    Year = movie.Year,
                    Genre = movie.Genre ?? "Не указан",
                    Director = movie.Director ?? "Не указан",
                    Duration = movie.Duration,
                    Rating = movie.Rating,
                    CoverImageUrl = string.IsNullOrEmpty(movie.CoverImageUrl)
        ? "/images/default-movie.jpg"
        : movie.CoverImageUrl,

                    IsAvailable = movie.MediaItems != null && movie.MediaItems.Any(mi => mi.IsAvailable),
                    AvailableCopies = movie.MediaItems?.Count(mi => mi.IsAvailable) ?? 0,
                    AvailableFormats = movie.MediaItems?
        .Where(mi => mi.IsAvailable && mi.MediaType != null && !string.IsNullOrEmpty(mi.MediaType.Name))
        .Select(mi => mi.MediaType.Name)
        .Distinct()
        .ToList() ?? new List<string>(),

                    FormatPrices = movie.MediaItems?
        .Where(mi => mi.MediaType != null && !string.IsNullOrEmpty(mi.MediaType.Name))
        .GroupBy(mi => mi.MediaType.Name)
        .ToDictionary(g => g.Key, g => g.First().MediaType.DailyRentalPrice)
        ?? new Dictionary<string, decimal>(),

                    // Исправляем на MediaItemInfos
                    MediaItems = movie.MediaItems?
        .Select(mi => new MediaItemInfo  // Используем MediaItemInfo вместо MediaItemInfo
        {
            MediaItemId = mi.MediaItemId,
            Format = mi.MediaType?.Name ?? "Неизвестно",
            Condition = mi.Condition ?? "Не указано",
            IsAvailable = mi.IsAvailable,
            DailyPrice = mi.MediaType?.DailyRentalPrice ?? 0,
            Barcode = mi.Barcode ?? ""
        }).ToList() ?? new List<MediaItemInfo>()
                };

                ViewData["Title"] = movie.Title + " - Видеопрокат";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в Details: {ex.Message}");
                return View("Error");
            }
        }

        // Поиск фильмов (AJAX)
        [HttpGet]
        public IActionResult Search(string term)
        {
            Console.WriteLine($"=== AJAX Search вызван ===");
            Console.WriteLine($"Параметр term: '{term}'");

            if (string.IsNullOrEmpty(term))
            {
                Console.WriteLine("Term пустой, возвращаем пустой список");
                return Json(new List<object>());
            }

            // Декодируем term
            try
            {
                term = System.Web.HttpUtility.UrlDecode(term, System.Text.Encoding.UTF8);
                Console.WriteLine($"Декодированный term: '{term}'");
            }
            catch
            {
                Console.WriteLine($"Не удалось декодировать term: '{term}'");
            }

            string termLower = term.ToLowerInvariant();

            var movies = _context.Movies
                .Where(m => m.Title != null && EF.Functions.Like(m.Title.ToLower(), $"%{termLower}%"))
                .Select(m => new
                {
                    id = m.MovieId,
                    text = m.Title,
                    year = m.Year,
                    genre = m.Genre
                })
                .Take(10)
                .ToList();

            Console.WriteLine($"Найдено фильмов для autocomplete: {movies.Count}");
            return Json(movies);
        }

        // Получение списка жанров
        [HttpGet]
        public IActionResult GetGenres()
        {
            var genres = _context.Movies
                .Where(m => m.Genre != null && m.Genre != "")
                .Select(m => m.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            return Json(genres);
        }

        private List<string> GetAvailableGenres()
        {
            return _context.Movies
                .Where(m => m.Genre != null && m.Genre != "")
                .Select(m => m.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        private List<string> GetAvailableFormats()
        {
            return _context.MediaTypes
                .Where(mt => mt.Name != null && mt.Name != "")
                .Select(mt => mt.Name)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }
    }
}