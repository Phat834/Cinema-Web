using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CinemaManagement.Models;
using CinemaManagement.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CinemaManagement.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public MoviesController(AppDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        protected void SetCommonData()
        {
            ViewBag.Theaters = _cache.GetOrCreate("theaters:active", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                return _context.Theaters.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.Name).ToList();
            });
            ViewBag.Genres = _cache.GetOrCreate("genres:all", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                return _context.Genres.AsNoTracking().OrderBy(g => g.Name).ToList();
            });
            ViewBag.ShowtimeHours = _cache.GetOrCreate("showtimes:hours", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                return _context.Showtimes.AsNoTracking().Select(s => s.StartTime.Hour).Distinct().OrderBy(h => h).ToList();
            });
        }

        private IQueryable<Movie> BaseMovieQuery()
            => _context.Movies.AsNoTracking()
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre);

        private static IQueryable<Movie> ApplyFilters(IQueryable<Movie> movies, string searchQuery, int? theaterId, int? genreId, string sortBy)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
                movies = movies.Where(m => m.Title.Contains(searchQuery));
            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));
            if (theaterId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.Room.TheaterId == theaterId));
            return sortBy switch
            {
                "title_asc" => movies.OrderBy(m => m.Title),
                "title_desc" => movies.OrderByDescending(m => m.Title),
                "rating_asc" => movies.OrderBy(m => m.Rating),
                "rating_desc" => movies.OrderByDescending(m => m.Rating),
                _ => movies
            };
        }

        public async Task<IActionResult> Index()
        {
            SetCommonData();
            var now = DateTime.Now;
            var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfNextMonth = firstDayOfCurrentMonth.AddMonths(1);

            ViewBag.TopRatedMovies = await BaseMovieQuery()
                .Where(m => m.Showtimes.Any(s => s.StartTime >= firstDayOfCurrentMonth && s.StartTime < firstDayOfNextMonth))
                .OrderByDescending(m => m.Rating)
                .Take(10)
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> NowShowing(string searchQuery, int? theaterId, string sortBy, int? genreId)
        {
            SetCommonData();
            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            var movies = BaseMovieQuery()
                .Where(m => m.Showtimes.Any(s => s.StartTime >= firstDay && s.StartTime < nextMonth));
            movies = ApplyFilters(movies, searchQuery, theaterId, genreId, sortBy);

            var model = new TheaterMoviesViewModel
            {
                Movies = await movies.ToListAsync(),
                IsFiltered = theaterId.HasValue,
                TheaterName = theaterId.HasValue
                    ? await _context.Theaters.AsNoTracking().Where(t => t.TheaterId == theaterId).Select(t => t.Name).FirstOrDefaultAsync()
                    : null
            };
            return View("MovieList", model);
        }

        public async Task<IActionResult> ComingSoon(string searchQuery, int? theaterId, string sortBy, int? genreId)
        {
            SetCommonData();
            var now = DateTime.Now;
            var firstDayNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var movies = BaseMovieQuery()
                .Where(m => m.Showtimes.Any(s => s.StartTime >= firstDayNextMonth));
            movies = ApplyFilters(movies, searchQuery, theaterId, genreId, sortBy);

            var model = new TheaterMoviesViewModel
            {
                Movies = await movies.ToListAsync(),
                IsFiltered = theaterId.HasValue,
                TheaterName = theaterId.HasValue
                    ? await _context.Theaters.AsNoTracking().Where(t => t.TheaterId == theaterId).Select(t => t.Name).FirstOrDefaultAsync()
                    : null
            };
            return View("MovieList", model);
        }

        [HttpGet]
        public IActionResult FilterAjax(string searchQuery, int? theaterId, int? genreId, int? time, string action)
        {
            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            var movies = BaseMovieQuery();
            if (action == "NowShowing")
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime >= firstDay && s.StartTime < nextMonth));
            else if (action == "ComingSoon")
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime >= nextMonth));

            if (!string.IsNullOrWhiteSpace(searchQuery))
                movies = movies.Where(m => m.Title.Contains(searchQuery));
            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));
            if (theaterId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.Room.TheaterId == theaterId));
            if (time.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime.Hour == time));

            return PartialView("_MovieCardsPartial", movies.ToList());
        }

        public IActionResult Details(int id)
        {
            var movie = _context.Movies.AsNoTracking()
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes).ThenInclude(s => s.Room).ThenInclude(r => r.Theater)
                .Include(m => m.MovieRatings)
                .FirstOrDefault(m => m.MovieId == id);

            if (movie == null)
            {
                TempData["Error"] = "Phim không tồn tại!";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetInt32("MovieId", id);
            return View(movie);
        }

        public IActionResult Watch(int id)
        {
            var movie = _context.Movies.AsNoTracking().FirstOrDefault(m => m.MovieId == id);
            if (movie == null)
            {
                TempData["Error"] = "Phim không tồn tại!";
                return RedirectToAction("Index");
            }
            ViewBag.MovieEmbedUrl = movie.VideoUrl;
            return View();
        }

        public IActionResult Showtimes(int movieId, int? theaterId)
        {
            var showtimes = _context.Showtimes.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room).ThenInclude(r => r.Theater)
                .Where(s => s.MovieId == movieId);
            if (theaterId.HasValue)
                showtimes = showtimes.Where(s => s.Room.TheaterId == theaterId);

            var result = showtimes.ToList();
            if (!result.Any())
            {
                TempData["Error"] = "Không có suất chiếu khả dụng.";
                return RedirectToAction("Index");
            }
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateMovie(int movieId, int rating)
        {
            if (rating < 1 || rating > 10)
            {
                TempData["Error"] = "Đánh giá không hợp lệ.";
                return RedirectToAction("Details", new { id = movieId });
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieId == movieId);
            var user = await _userManager.GetUserAsync(User);
            if (movie == null || user == null)
                return RedirectToAction("Details", new { id = movieId });

            var existing = await _context.MovieRatings.FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == user.Id);
            if (existing != null)
            {
                existing.Rating = rating;
                existing.RatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.MovieRatings.Add(new MovieRating { MovieId = movieId, UserId = user.Id, Rating = rating, RatedAt = DateTime.UtcNow });
            }
            await _context.SaveChangesAsync();

            movie.Rating = Math.Round(await _context.MovieRatings.Where(r => r.MovieId == movieId).AverageAsync(r => r.Rating), 1);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", new { id = movieId });
        }

        [HttpGet]
        public IActionResult FilterShowtimes(int movieId, string date, string city)
        {
            string decodedCity = string.IsNullOrEmpty(city) ? null : WebUtility.UrlDecode(city);
            DateTime? selectedDate = DateTime.TryParse(date, out var d) ? d.Date : null;

            var showtimes = _context.Showtimes.AsNoTracking()
                .Include(s => s.Room).ThenInclude(r => r.Theater)
                .Where(s => s.MovieId == movieId);

            if (selectedDate.HasValue)
                showtimes = showtimes.Where(s => s.StartTime.Date == selectedDate.Value);
            if (!string.IsNullOrEmpty(decodedCity))
            {
                var normalizedCity = decodedCity.Trim().ToLower();
                showtimes = showtimes.Where(s => s.Room.Theater.City != null && s.Room.Theater.City.Trim().ToLower() == normalizedCity);
            }

            return Json(showtimes
                .OrderBy(s => s.StartTime)
                .Select(s => new
                {
                    s.ShowtimeId,
                    City = s.Room.Theater.City,
                    Theater = s.Room.Theater.Name,
                    Room = s.Room.Name,
                    StartTime = s.StartTime.ToString("HH:mm"),
                    s.Price,
                    s.DiscountPercent
                })
                .ToList());
        }
    }
}
