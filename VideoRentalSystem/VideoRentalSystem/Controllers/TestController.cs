using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using System.Linq;

namespace VideoRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("movies")]
        public IActionResult GetMovies()
        {
            var movies = _context.Movies
                .Include(m => m.MediaItems)
                .ThenInclude(mi => mi.MediaType)
                .ToList();

            return Ok(movies);
        }

        [HttpGet("available")]
        public IActionResult GetAvailableMovies()
        {
            var availableMovies = _context.Movies
                .Where(m => m.MediaItems.Any(mi => mi.IsAvailable))
                .Select(m => new
                {
                    m.Title,
                    m.Year,
                    m.Genre,
                    AvailableCopies = m.MediaItems.Count(mi => mi.IsAvailable),
                    Formats = m.MediaItems
                        .Where(mi => mi.IsAvailable)
                        .Select(mi => mi.MediaType.Name)
                        .Distinct()
                })
                .ToList();

            return Ok(availableMovies);
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Database = _context.Database.CanConnect() ? "Connected" : "Disconnected"
            });
        }
    }
}