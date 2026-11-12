using CinemaManagement.Data;
using CinemaManagement.Models;

namespace CinemaManagement.Services
{
    public interface IPricingService
    {
        decimal CalculateBasePrice(Showtime showtime, Room room);
        Task<SeatTypePriceRates> GetSeatTypeRatesAsync(int roomId);
        decimal CalculateTicketPrice(decimal basePrice, decimal seatExtra, bool discountActive, decimal discountPercent);
    }

    public record SeatTypePriceRates(decimal Regular, decimal Vip, decimal Couple);
}
