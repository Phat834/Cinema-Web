using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult RevenueStats()
    {
        var today = DateTime.UtcNow.Date;
        var vm = new RevenueStatsViewModel
        {
            FromDate = today.AddDays(-30),
            ToDate = today,
            PaymentMethod = "All"
        };
        vm = GetRevenueStats(vm.FromDate!.Value, vm.ToDate!.Value, vm.PaymentMethod);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RevenueStats(DateTime? fromDate, DateTime? toDate, string paymentMethod)
    {
        var today = DateTime.UtcNow.Date;
        var from = (fromDate ?? today.AddMonths(-1)).Date;
        var to = (toDate ?? today).Date;
        if (string.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "All";
        var vm = GetRevenueStats(from, to, paymentMethod);
        return View(vm);
    }

    private RevenueStatsViewModel GetRevenueStats(DateTime from, DateTime to, string paymentMethod)
    {
        var fromInclusive = from.Date;
        var toExclusive = to.Date.AddDays(1);

        var ticketsQuery = _context.Tickets
            .AsNoTracking()
            .Where(t => t.IsPaid && t.BookingTime >= fromInclusive && t.BookingTime < toExclusive);

        if (string.Equals(paymentMethod, "Momo", StringComparison.OrdinalIgnoreCase))
            ticketsQuery = ticketsQuery.Where(t => t.PaymentMethod == "Momo");
        else if (string.Equals(paymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            ticketsQuery = ticketsQuery.Where(t => t.PaymentMethod == "Cash");

        var tickets = ticketsQuery
            .Select(t => new
            {
                t.Price,
                MovieTitle = t.Showtime.Movie.Title,
                TheaterName = t.Seat.Room.Theater.Name,
                ShowtimeId = t.Showtime.ShowtimeId,
                ShowtimeStart = t.Showtime.StartTime
            })
            .ToList();

        return new RevenueStatsViewModel
        {
            FromDate = fromInclusive,
            ToDate = to.Date,
            PaymentMethod = paymentMethod,
            TotalRevenue = tickets.Sum(t => t.Price),
            TotalTickets = tickets.Count,
            ByMovie = tickets
                .GroupBy(t => t.MovieTitle)
                .Select(g => new RevenueByMovie { MovieTitle = g.Key, TicketsSold = g.Count(), Revenue = g.Sum(t => t.Price) })
                .OrderByDescending(x => x.Revenue)
                .ToList(),
            ByTheater = tickets
                .GroupBy(t => t.TheaterName)
                .Select(g => new RevenueByTheater { TheaterName = g.Key, TicketsSold = g.Count(), Revenue = g.Sum(t => t.Price) })
                .OrderByDescending(x => x.Revenue)
                .ToList(),
            ByShowtime = tickets
                .GroupBy(t => new { t.ShowtimeId, t.MovieTitle, t.TheaterName, t.ShowtimeStart })
                .Select(g => new RevenueByShowtime
                {
                    MovieTitle = g.Key.MovieTitle,
                    TheaterName = g.Key.TheaterName,
                    Showtime = g.Key.ShowtimeStart,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .OrderByDescending(x => x.Showtime)
                .ToList()
        };
    }
}
