using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;

        public PricingService(AppDbContext context)
        {
            _context = context;
        }

        public decimal CalculateBasePrice(Showtime showtime, Room room)
            => showtime.Price * (room.PriceMultiplier ?? 1);

        public async Task<SeatTypePriceRates> GetSeatTypeRatesAsync(int roomId)
        {
            var rates = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == roomId)
                .ToListAsync();

            return new SeatTypePriceRates(
                rates.FirstOrDefault(x => x.SeatType == "Regular")?.Price ?? 0,
                rates.FirstOrDefault(x => x.SeatType == "Vip")?.Price ?? 0,
                rates.FirstOrDefault(x => x.SeatType == "Couple")?.Price ?? 0
            );
        }

        public decimal CalculateTicketPrice(decimal basePrice, decimal seatExtra, bool discountActive, decimal discountPercent)
        {
            var discountMultiplier = discountActive ? 1m - discountPercent / 100m : 1m;
            return Math.Round((basePrice + seatExtra) * discountMultiplier);
        }
    }
}
