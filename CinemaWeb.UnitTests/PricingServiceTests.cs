using CinemaManagement.Models;
using CinemaManagement.Services;
using FluentAssertions;
using Xunit;

namespace CinemaWeb.UnitTests
{
    public class PricingServiceTests
    {
        private readonly PricingService _pricing;

        public PricingServiceTests()
        {
            _pricing = new PricingService(null!);
        }

        [Fact]
        public void CalculateBasePrice_StandardRoom_PriceMultiplier1_5()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 1.5m };

            // Act
            var basePrice = _pricing.CalculateBasePrice(showtime, room);

            // Assert
            basePrice.Should().Be(150000m);
        }

        [Fact]
        public void CalculateBasePrice_NullPriceMultiplier_DefaultsTo1_0()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = null };

            // Act
            var basePrice = _pricing.CalculateBasePrice(showtime, room);

            // Assert
            basePrice.Should().Be(100000m);
        }

        [Fact]
        public void CalculateBasePrice_HolidayMultiplier1_3()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 1.3m };

            // Act
            var basePrice = _pricing.CalculateBasePrice(showtime, room);

            // Assert
            basePrice.Should().Be(130000m);
        }

        [Fact]
        public void CalculateBasePrice_WeekendMultiplier2_0()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 2.0m };

            // Act
            var basePrice = _pricing.CalculateBasePrice(showtime, room);

            // Assert
            basePrice.Should().Be(200000m);
        }

        [Fact]
        public void CalculateTicketPrice_WithDiscount_AppliesReduction()
        {
            // Arrange
            decimal basePrice = 100000m;
            decimal seatExtra = 50000m;
            bool discountActive = true;
            decimal discountPercent = 20m;

            // Act
            var ticketPrice = _pricing.CalculateTicketPrice(basePrice, seatExtra, discountActive, discountPercent);

            // Assert
            ticketPrice.Should().Be(Math.Round((basePrice + seatExtra) * (1m - discountPercent / 100m)));
        }

        [Fact]
        public void CalculateTicketPrice_WithoutDiscount_ReturnsBasePlusExtra()
        {
            // Arrange
            decimal basePrice = 100000m;
            decimal seatExtra = 50000m;
            bool discountActive = false;
            decimal discountPercent = 20m;

            // Act
            var ticketPrice = _pricing.CalculateTicketPrice(basePrice, seatExtra, discountActive, discountPercent);

            // Assert
            ticketPrice.Should().Be(150000m);
        }

        [Fact]
        public void CalculateTicketPrice_RoundsDecimalResult()
        {
            // Arrange
            decimal basePrice = 100001m;
            decimal seatExtra = 0m;
            bool discountActive = false;
            decimal discountPercent = 0m;

            // Act
            var ticketPrice = _pricing.CalculateTicketPrice(basePrice, seatExtra, discountActive, discountPercent);

            // Assert
            ticketPrice.Should().Be(Math.Round(basePrice));
            ticketPrice.Should().Be(100001m);
        }

        [Fact]
        public void CalculateTicketPrice_RoundsDiscountedResult()
        {
            // Arrange
            decimal basePrice = 100000m;
            decimal seatExtra = 0m;
            bool discountActive = true;
            decimal discountPercent = 33m;

            // Act
            var ticketPrice = _pricing.CalculateTicketPrice(basePrice, seatExtra, discountActive, discountPercent);

            // Assert
            ticketPrice.Should().Be(Math.Round((basePrice + seatExtra) * (1m - discountPercent / 100m)));
        }
    }
}
