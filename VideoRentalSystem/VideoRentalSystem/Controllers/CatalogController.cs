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
            Console.WriteLine($"=== НАЧАЛО Catalog/Index ===");

            // Декодируем параметры из URL
            if (!string.IsNullOrEmpty(search))
            {
                try
                {
                    // Пробуем декодировать URL-encoded строку
                    search = System.Web.HttpUtility.UrlDecode(search, System.Text.Encoding.UTF8);
                    Console.WriteLine($"Декодированный search (UrlDecode): '{search}'");
                }
                catch
                {
                    // Если не получается, пробуем другой метод
                    try
                    {
                        search = Uri.UnescapeDataString(search);
                        Console.WriteLine($"Декодированный search (UnescapeDataString): '{search}'");
                    }
                    catch
                    {
                        Console.WriteLine($"Не удалось декодировать search: '{search}'");
                    }
                }
            }

            // Если search все еще пустой, пробуем получить из QueryString
            if (string.IsNullOrEmpty(search))
            {
                search = HttpContext.Request.Query["search"].ToString();
                if (!string.IsNullOrEmpty(search))
                {
                    try
                    {
                        search = System.Web.HttpUtility.UrlDecode(search, System.Text.Encoding.UTF8);
                        Console.WriteLine($"Search из QueryString (декодирован): '{search}'");
                    }
                    catch
                    {
                        Console.WriteLine($"Search из QueryString (сырой): '{search}'");
                    }
                }
            }

            // Логируем параметры
            Console.WriteLine($"Окончательный search: '{search}'");
            Console.WriteLine($"Genre: '{genre}'");
            Console.WriteLine($"Format: '{format}'");
            Console.WriteLine($"QueryString: {HttpContext.Request.QueryString}");

            try
            {
                // Начинаем запрос
                var query = _context.Movies
                    .Include(m => m.MediaItems)
                        .ThenInclude(mi => mi.MediaType)
                    .AsQueryable();

                int initialCount = query.Count();
                Console.WriteLine($"Всего фильмов в базе: {initialCount}");

                // ФИЛЬТРАЦИЯ ПО ПОИСКУ
                if (!string.IsNullOrEmpty(search))
                {
                    Console.WriteLine($"Применяем фильтр поиска для: '{search}'");

                    // Для SQLite нужно использовать регистронезависимое сравнение
                    // Преобразуем search в нижний регистр для сравнения
                    string searchLower = search.ToLowerInvariant();

                    query = query.Where(m =>
                        (m.Title != null && EF.Functions.Like(m.Title.ToLower(), $"%{searchLower}%")) ||
                        (m.Description != null && EF.Functions.Like(m.Description.ToLower(), $"%{searchLower}%")) ||
                        (m.Director != null && EF.Functions.Like(m.Director.ToLower(), $"%{searchLower}%")) ||
                        (m.Genre != null && EF.Functions.Like(m.Genre.ToLower(), $"%{searchLower}%")));

                    Console.WriteLine($"Фильмов после поиска: {query.Count()}");

                    // ДЛЯ ОТЛАДКИ: выведем найденные фильмы
                    var debugMovies = query.Select(m => new { m.Title, m.Genre }).ToList();
                    Console.WriteLine($"Найдено фильмов: {debugMovies.Count}");
                    foreach (var movie in debugMovies)
                    {
                        Console.WriteLine($"  - {movie.Title} ({movie.Genre})");
                    }
                }

                // ФИЛЬТРАЦИЯ ПО ЖАНРУ
                if (!string.IsNullOrEmpty(genre))
                {
                    Console.WriteLine($"Применяем фильтр жанра: '{genre}'");
                    query = query.Where(m => m.Genre != null && m.Genre.Contains(genre));
                    Console.WriteLine($"Фильмов после жанра: {query.Count()}");
                }

                // ФИЛЬТРАЦИЯ ПО ФОРМАТУ (только доступные копии)
                if (!string.IsNullOrEmpty(format))
                {
                    Console.WriteLine($"Применяем фильтр формата: '{format}'");
                    query = query.Where(m => m.MediaItems.Any(mi =>
                        mi.MediaType != null &&
                        mi.MediaType.Name != null &&
                        mi.MediaType.Name == format &&
                        mi.IsAvailable == true));
                    Console.WriteLine($"Фильмов после формата: {query.Count()}");
                }

                // Применяем ToList только один раз
                var movies = query.ToList();
                Console.WriteLine($"Итого фильмов для отображения: {movies.Count}");

                // Преобразуем в ViewModel
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

                    // Информация о доступности
                    IsAvailable = m.MediaItems != null && m.MediaItems.Any(mi => mi.IsAvailable),
                    AvailableCopies = m.MediaItems?.Count(mi => mi.IsAvailable) ?? 0,
                    AvailableFormats = m.MediaItems?
                        .Where(mi => mi.IsAvailable && mi.MediaType != null && !string.IsNullOrEmpty(mi.MediaType.Name))
                        .Select(mi => mi.MediaType.Name)
                        .Distinct()
                        .ToList() ?? new List<string>(),

                    NextAvailableDate = null,

                    // Цены по форматам
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

                Console.WriteLine($"=== КОНЕЦ Catalog/Index ===");
                return View(movieViewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в каталоге: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ViewData["Error"] = $"Ошибка: {ex.Message}";
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

                    MediaItems = movie.MediaItems?.Select(mi => new MediaItemInfo
                    {
                        MediaItemId = mi.MediaItemId,
                        Format = mi.MediaType?.Name ?? "Неизвестно",
                        Condition = mi.Condition ?? "Не указано",
                        IsAvailable = mi.IsAvailable,
                        DailyPrice = mi.MediaType?.DailyRentalPrice ?? 0
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