using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CinemaManagement.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PricingService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public decimal CalculateBasePrice(Showtime showtime, Room room)
            => showtime.Price * (room.PriceMultiplier ?? 1);

        public async Task<SeatTypePriceRates> GetSeatTypeRatesAsync(int roomId)
        {
            var key = $"room-seat-rates:{roomId}";
            if (_cache.TryGetValue(key, out SeatTypePriceRates? cached) && cached != null)
                return cached;

            var rates = await _context.RoomSeatTypePrices
                .AsNoTracking()
                .Where(p => p.RoomId == roomId)
                .ToListAsync();

            var result = new SeatTypePriceRates(
                rates.FirstOrDefault(x => x.SeatType == "Regular")?.Price ?? 0,
                rates.FirstOrDefault(x => x.SeatType == "Vip")?.Price ?? 0,
                rates.FirstOrDefault(x => x.SeatType == "Couple")?.Price ?? 0
            );
            _cache.Set(key, result, TimeSpan.FromMinutes(2));
            return result;
        }

        public decimal CalculateTicketPrice(decimal basePrice, decimal seatExtra, bool discountActive, decimal discountPercent)
        {
            var discountMultiplier = discountActive ? 1m - discountPercent / 100m : 1m;
            return Math.Round((basePrice + seatExtra) * discountMultiplier);
        }
    }
}
