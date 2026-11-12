using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;

public class SeatResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BookingSettings _bookingSettings;

    public SeatResetService(IServiceProvider serviceProvider, IOptions<BookingSettings> bookingSettings)
    {
        _serviceProvider = serviceProvider;
        _bookingSettings = bookingSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;
                var timeout = _bookingSettings.ReservationTimeoutMinutes;

                // Bọc toàn bộ quá trình cập nhật trong transaction để đảm bảo tính toàn vẹn dữ liệu
                await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

                try
                {
                    // 1) Hủy vé chưa thanh toán quá hạn:
                    //    (BookingTime + Timeout < now) AND (IsPaid == false)
                    //    → trả lại ghế, xóa vé
                    var expiredTickets = await context.Tickets
                        .Where(t => !t.IsPaid && t.BookingTime < now.AddMinutes(-timeout))
                        .Include(t => t.Seat)
                        .OrderBy(t => t.BookingTime)
                        .ToListAsync(stoppingToken);

                    foreach (var ticket in expiredTickets)
                    {
                        if (ticket.Seat != null)
                            ticket.Seat.IsBooked = false;

                        context.Tickets.Remove(ticket);
                    }

                    // 2) Reset ghế của các suất chiếu đã kết thúc
                    var endedShowtimes = await context.Showtimes
                        .Include(s => s.Movie)
                        .Where(s => s.StartTime.AddMinutes(s.Movie.Duration) < now)
                        .Select(s => s.ShowtimeId)
                        .ToListAsync(stoppingToken);

                    foreach (var showtimeId in endedShowtimes)
                    {
                        var paidTickets = await context.Tickets
                            .Where(t => t.ShowtimeId == showtimeId && t.IsPaid)
                            .Include(t => t.Seat)
                            .ToListAsync(stoppingToken);

                        foreach (var ticket in paidTickets)
                        {
                            if (ticket.Seat != null)
                                ticket.Seat.IsBooked = false;
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(stoppingToken);
                    Console.WriteLine($"SeatResetService transaction error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SeatResetService error: {ex.Message}");
            }

            // Chạy lại sau mỗi 1 phút
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}