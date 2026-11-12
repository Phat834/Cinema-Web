using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using CinemaManagement.ViewModel;
using Newtonsoft.Json;
namespace CinemaManagement.Controllers
{
[Authorize]
public class BookingController : Controller
{
    private readonly AppDbContext _context; private readonly IPricingService _pricing; private readonly IMoMoPaymentService _momo; private readonly EmailService _email; private readonly ILogger<BookingController> _log; private readonly UserManager<ApplicationUser> _users; private readonly int _reservationTimeout;
    public BookingController(AppDbContext context, IPricingService pricing, IMoMoPaymentService momo, ILogger<BookingController> log, EmailService email, UserManager<ApplicationUser> users, IOptions<BookingSettings> settings) { _context = context; _pricing = pricing; _momo = momo; _log = log; _email = email; _users = users; _reservationTimeout = settings.Value.ReservationTimeoutMinutes; }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? movieId)
    {
        if (!movieId.HasValue) return NotFound("Không tìm thấy phim!");
        var showtimes = await _context.Showtimes.Where(s => s.MovieId == movieId).Include(s => s.Movie).Include(s => s.Room).ThenInclude(r => r.Theater).ToListAsync();
        if (!showtimes.Any()) return NotFound("Không tìm thấy suất chiếu nào!");
        return View(new BookingViewModel { Movie = showtimes[0].Movie, Showtimes = showtimes });
    }

    public async Task<IActionResult> SelectSeats(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        if (await _users.IsInRoleAsync(user, "Admin")) { TempData["Error"] = "Quản trị viên không thể mua vé."; return RedirectToAction("Index", "Movies"); }
        var showtime = await _context.Showtimes.Include(s => s.Movie).Include(s => s.Room).FirstOrDefaultAsync(s => s.ShowtimeId == id);
        if (showtime == null) return NotFound("Lịch chiếu không tồn tại");
        var seats = await _context.Seats.Where(s => s.RoomId == showtime.RoomId).Select(s => new Seat { SeatId = s.SeatId, SeatNumber = s.SeatNumber, SeatType = s.SeatType, IsBooked = false }).ToListAsync();
        var paidSeatIds = await _context.Tickets.Where(t => t.ShowtimeId == id && t.IsPaid).Select(t => t.SeatId).ToListAsync();
        var reservedSeatIds = await _context.Tickets.Where(t => t.ShowtimeId == id && !t.IsPaid && t.BookingTime >= DateTime.Now.AddMinutes(-_reservationTimeout)).Select(t => t.SeatId).ToListAsync();
        var basePrice = _pricing.CalculateBasePrice(showtime, showtime.Room);
        var rates = await _pricing.GetSeatTypeRatesAsync(showtime.RoomId);
        return View(new SeatSelectionViewModel { Showtime = showtime, Movie = showtime.Movie, Room = showtime.Room, AvailableSeats = seats, BasePrice = basePrice, PriceRegular = rates.Regular, PriceVip = rates.Vip, PriceCouple = rates.Couple, DiscountPercent = showtime.DiscountPercent, DiscountStart = showtime.DiscountStart, DiscountEnd = showtime.DiscountEnd, PaidSeatIds = paidSeatIds, ReservedSeatIds = reservedSeatIds, ReservationTimeoutMinutes = _reservationTimeout });
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest req)
    {
        if (req?.SeatIds == null || !req.SeatIds.Any()) return Json(new { success = false, error = "Chưa chọn ghế" });
        var showtime = await _context.Showtimes.Include(s => s.Room).FirstOrDefaultAsync(s => s.ShowtimeId == req.ShowtimeId);
        if (showtime == null) return Json(new { success = false, error = "Suất chiếu không hợp lệ" });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var takenSeats = await _context.Tickets.Where(t => t.ShowtimeId == req.ShowtimeId && (t.IsPaid || (!t.IsPaid && t.BookingTime >= DateTime.Now.AddMinutes(-_reservationTimeout)))).Select(t => t.SeatId).ToListAsync();
        if (req.SeatIds.Any(id => takenSeats.Contains(id))) return Json(new { success = false, error = "Ghế đã được đặt" });
        var basePrice = _pricing.CalculateBasePrice(showtime, showtime.Room);
        var rates = await _pricing.GetSeatTypeRatesAsync(showtime.RoomId);
        var discountActive = showtime.DiscountPercent.HasValue && showtime.DiscountStart <= DateTime.Now && showtime.DiscountEnd >= DateTime.Now;
        string orderId = $"ORDER_{Guid.NewGuid():N}".Substring(0, 20);
        DateTime bookingTime = DateTime.Now;
        foreach (var seatId in req.SeatIds)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            decimal seatExtra = seat.SeatType switch { SeatType.Vip => rates.Vip, SeatType.Couple => rates.Couple, _ => rates.Regular };
            _context.Tickets.Add(new Ticket { UserId = userId, ShowtimeId = showtime.ShowtimeId, SeatId = seatId, BookingTime = bookingTime, Price = _pricing.CalculateTicketPrice(basePrice, seatExtra, discountActive, showtime.DiscountPercent ?? 0), IsPaid = false, OrderId = orderId, PaymentMethod = "Pending" });
        }
        await _context.SaveChangesAsync();
        return Json(new { success = true, redirectUrl = Url.Action("SelectCombos", new { orderId }) });
    }

    public async Task<IActionResult> SelectCombos(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return NotFound("OrderId không hợp lệ!");
        var tickets = await _context.Tickets.Where(t => t.OrderId == orderId).ToListAsync();
        if (!tickets.Any()) return NotFound("Không tìm thấy vé!");
        return View(new ComboSelectionViewModel { OrderId = orderId, TicketTotal = tickets.Sum(t => t.Price), AvailableCombos = await _context.Combos.Where(c => c.IsAvailable).Select(c => new ComboItem { Id = c.ComboId, Name = c.Name, Description = c.Description, Price = c.Price, ImageUrl = c.ImageUrl }).ToListAsync(), ComboTotal = 0, GrandTotal = tickets.Sum(t => t.Price) });
    }

    [HttpPost]
    public IActionResult SaveCombos([FromBody] ComboRequest req)
    {
        if (string.IsNullOrEmpty(req.OrderId) || req.SelectedCombos == null) return Json(new { success = false, error = "Dữ liệu không hợp lệ" });
        HttpContext.Session.SetString($"Combos_{req.OrderId}", JsonConvert.SerializeObject(req.SelectedCombos));
        HttpContext.Session.SetString($"ComboTotal_{req.OrderId}", req.ComboTotal.ToString());
        return Json(new { success = true });
    }

    public async Task<IActionResult> Payment(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return NotFound("OrderId không hợp lệ!");
        var tickets = await _context.Tickets.Where(t => t.OrderId == orderId).ToListAsync();
        if (!tickets.Any()) return NotFound("Không tìm thấy vé!");
        decimal ticketTotal = tickets.Sum(t => t.Price); decimal comboTotal = 0;
        var comboTotalStr = HttpContext.Session.GetString($"ComboTotal_{orderId}");
        if (!string.IsNullOrEmpty(comboTotalStr)) decimal.TryParse(comboTotalStr, out comboTotal);
        decimal grandTotal = ticketTotal + comboTotal;
        HttpContext.Session.SetString("TotalAmount", ((long)grandTotal).ToString());
        return View(new PaymentViewModel { OrderId = orderId, TicketTotal = ticketTotal, ComboTotal = comboTotal, GrandTotal = grandTotal });
    }

    [HttpPost]
    public async Task<IActionResult> GetMomoQRCode([FromBody] PaymentRequest req)
    {
        if (req == null || req.Amount <= 0) return BadRequest(new { success = false, error = "Dữ liệu không hợp lệ!" });
        string qrUrl = await _momo.CreatePaymentRequestAsync(req.Amount, req.OrderId);
        var tickets = await _context.Tickets.Where(t => t.OrderId == req.OrderId).ToListAsync();
        foreach (var t in tickets) t.PaymentMethod = "Pending";
        await _context.SaveChangesAsync();
        return Ok(new { success = true, qrUrl });
    }
    [HttpGet]
    public async Task<IActionResult> MockMomoCheckout(string orderId, long amount)
    {
        if (string.IsNullOrEmpty(orderId)) return RedirectToAction("Index", "Movies");

        var ticketsToPay = await _context.Tickets
            .Where(t => t.OrderId == orderId && !t.IsPaid)
            .Include(t => t.Seat)
            .ToListAsync();

        foreach (var t in ticketsToPay)
        {
            t.IsPaid = true;
            t.PaymentMethod = "Momo (Sandbox)";
            if (t.Seat != null) t.Seat.IsBooked = true;
        }
        await _context.SaveChangesAsync();

        var tickets = await _context.Tickets
            .Where(t => t.OrderId == orderId && t.IsPaid)
            .Include(t => t.User)
            .Include(t => t.Seat).ThenInclude(s => s.Room).ThenInclude(r => r.Theater)
            .Include(t => t.Showtime).ThenInclude(s => s.Movie)
            .ToListAsync();

        if (tickets.Any() && !string.IsNullOrEmpty(tickets.First().User?.Email))
        {
            var htmlContent = BookingHelper.BuildTicketEmailHtml(orderId, tickets);
            try
            {
                await _email.SendTicketEmailAsync(tickets.First().User.Email, "Xác nhận đặt vé xem phim", htmlContent, orderId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Send email failed for OrderId {OrderId}", orderId);
            }
        }

        return RedirectToAction("PaymentSuccess", new { orderId });
    }

    [AllowAnonymous]
    [HttpPost("/api/momo/notify")]
    public async Task<IActionResult> MomoNotifyUrl()
    {
        using var reader = new StreamReader(Request.Body); var body = await reader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
        if (data == null || !data.ContainsKey("orderId")) return BadRequest();
        var orderId = data["orderId"];
        var result = _momo.ProcessNotify(data);
        if (result.IsSuccess)
        {
            var ticketsToPay = await _context.Tickets.Where(t => t.OrderId == orderId && !t.IsPaid).Include(t => t.Seat).ToListAsync();
            foreach (var t in ticketsToPay) { t.IsPaid = true; t.PaymentMethod = "Momo"; if (t.Seat != null) t.Seat.IsBooked = true; }
            await _context.SaveChangesAsync();
        }
        var tickets = await _context.Tickets.Where(t => t.OrderId == orderId && t.IsPaid).Include(t => t.User).Include(t => t.Seat).ThenInclude(s => s.Room).ThenInclude(r => r.Theater).Include(t => t.Showtime).ThenInclude(s => s.Movie).ToListAsync();
        if (!tickets.Any() || string.IsNullOrEmpty(tickets.First().User?.Email)) return Ok();
        var htmlContent = BookingHelper.BuildTicketEmailHtml(orderId, tickets);
        try { await _email.SendTicketEmailAsync(tickets.First().User.Email, "Xác nhận đặt vé xem phim", htmlContent, orderId); }
        catch (Exception ex) { _log.LogError(ex, "Send email failed for OrderId {OrderId}", orderId); }
        return Ok();
    }

    [AllowAnonymous]
    public async Task<IActionResult> PaymentSuccess(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return RedirectToAction("Index", "Movies");
        var tickets = await _context.Tickets.Include(t => t.User).Include(t => t.Seat).ThenInclude(s => s.Room).ThenInclude(r => r.Theater).Include(t => t.Showtime).ThenInclude(s => s.Movie).Where(t => t.OrderId == orderId).ToListAsync();
        if (!tickets.Any()) return RedirectToAction("Index", "Movies");
        if (tickets.Any(t => !t.IsPaid)) return View("PaymentPending", orderId);
        return View(new PaymentSuccessViewModel { Tickets = tickets, QrCodeBase64 = BookingHelper.GenerateQrCodeBase64($"OrderId={orderId}") });
    }

    [HttpGet]
    public async Task<IActionResult> GetPaidSeats(int showtimeId)
    {
        var paid = await _context.Tickets.Where(t => t.ShowtimeId == showtimeId && t.IsPaid).Select(t => new { t.SeatId, t.PaymentMethod }).ToListAsync();
        return Json(new { paidSeats = paid });
    }

    [Authorize(Roles = "Employee")]
    [HttpPost]
    public async Task<IActionResult> CashPayment([FromBody] CashPaymentRequest req)
    {
        if (string.IsNullOrEmpty(req.OrderId)) return Json(new { success = false, error = "OrderId không hợp lệ" });
        var tickets = await _context.Tickets.Where(t => t.OrderId == req.OrderId && !t.IsPaid).Include(t => t.Seat).ToListAsync();
        foreach (var t in tickets) { t.IsPaid = true; t.PaymentMethod = "Cash"; if (t.Seat != null) t.Seat.IsBooked = true; }
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}
}
