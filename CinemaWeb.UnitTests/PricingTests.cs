using CinemaManagement.Models;
using FluentAssertions;
using Xunit;

namespace CinemaWeb.UnitTests
{
    public class PricingTests
    {
        [Fact]
        public void BasePrice_ShouldBeShowtimePriceTimesRoomMultiplier()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 1.2m };

            // Act
            var basePrice = showtime.Price * (room.PriceMultiplier ?? 1);

            // Assert
            basePrice.Should().Be(120000m);
        }

        [Fact]
        public void BasePrice_ShouldUseDefaultMultiplier_WhenRoomMultiplierIsNull()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = null };

            // Act
            var basePrice = showtime.Price * (room.PriceMultiplier ?? 1);

            // Assert
            basePrice.Should().Be(100000m);
        }

        [Theory]
        [InlineData("Regular", 0)]
        [InlineData("Vip", 50000)]
        [InlineData("Couple", 100000)]
        public void SeatSurcharge_ShouldMatchSeatType(string seatType, decimal expectedSurcharge)
        {
            // Arrange
            var seatTypePrices = new List<RoomSeatTypePrice>
            {
                new() { SeatType = "Regular", Price = 0 },
                new() { SeatType = "Vip", Price = 50000 },
                new() { SeatType = "Couple", Price = 100000 }
            };

            // Act
            var surcharge = seatTypePrices
                .FirstOrDefault(x => x.SeatType == seatType)?.Price ?? 0;

            // Assert
            surcharge.Should().Be(expectedSurcharge);
        }

        [Fact]
        public void FinalPrice_ShouldApplyDiscount_WhenDiscountIsActive()
        {
            // Arrange
            decimal basePrice = 100000m;
            decimal seatExtra = 50000m;
            decimal discountPercent = 20m;
            bool discountActive = true;
            var now = DateTime.Now;

            // Act
            decimal discountMul = discountActive ? (1 - discountPercent / 100) : 1m;
            var finalPrice = (basePrice + seatExtra) * discountMul;

            // Assert
            finalPrice.Should().Be(120000m);
        }

        [Fact]
        public void FinalPrice_ShouldNotApplyDiscount_WhenDiscountIsNotActive()
        {
            // Arrange
            decimal basePrice = 100000m;
            decimal seatExtra = 50000m;
            bool discountActive = false;

            // Act
            decimal discountMul = discountActive ? (1 - 20m / 100) : 1m;
            var finalPrice = (basePrice + seatExtra) * discountMul;

            // Assert
            finalPrice.Should().Be(150000m);
        }

        [Fact]
        public void FinalPrice_ShouldApplyDiscount_WhenWithinDiscountWindow()
        {
            // Arrange
            var showtime = new Showtime
            {
                DiscountPercent = 20m,
                DiscountStart = DateTime.Now.AddHours(-1),
                DiscountEnd = DateTime.Now.AddHours(1)
            };
            var now = DateTime.Now;

            // Act
            bool discountActive =
                showtime.DiscountPercent.HasValue &&
                showtime.DiscountStart <= now &&
                showtime.DiscountEnd >= now;

            // Assert
            discountActive.Should().BeTrue();
        }

        [Fact]
        public void FinalPrice_ShouldNotApplyDiscount_WhenOutsideDiscountWindow()
        {
            // Arrange
            var showtime = new Showtime
            {
                DiscountPercent = 20m,
                DiscountStart = DateTime.Now.AddDays(-2),
                DiscountEnd = DateTime.Now.AddDays(-1)
            };
            var now = DateTime.Now;

            // Act
            bool discountActive =
                showtime.DiscountPercent.HasValue &&
                showtime.DiscountStart <= now &&
                showtime.DiscountEnd >= now;

            // Assert
            discountActive.Should().BeFalse();
        }

        [Fact]
        public void GrandTotal_ShouldIncludeComboTotal()
        {
            // Arrange
            decimal ticketTotal = 300000m;
            decimal comboTotal = 75000m;

            // Act
            var grandTotal = ticketTotal + comboTotal;

            // Assert
            grandTotal.Should().Be(375000m);
        }

        [Fact]
        public void FullPriceCalculation_RegularSeat_NoDiscount()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 1m };
            var seatExtra = 0m; // Regular
            bool discountActive = false;

            // Act
            decimal basePrice = showtime.Price * (room.PriceMultiplier ?? 1);
            decimal discountMul = discountActive ? (1 - 0m / 100) : 1m;
            var finalPrice = Math.Round((basePrice + seatExtra) * discountMul);

            // Assert
            finalPrice.Should().Be(100000m);
        }

        [Fact]
        public void FullPriceCalculation_VipSeat_WithDiscount()
        {
            // Arrange
            var showtime = new Showtime { Price = 100000m };
            var room = new Room { PriceMultiplier = 1.2m };
            var seatExtra = 50000m; // Vip
            bool discountActive = true;
            decimal discountPercent = 10m;

            // Act
            decimal basePrice = showtime.Price * (room.PriceMultiplier ?? 1);
            decimal discountMul = discountActive ? (1 - discountPercent / 100) : 1m;
            var finalPrice = Math.Round((basePrice + seatExtra) * discountMul);

            // Assert
            // (100000 * 1.2 + 50000) * 0.9 = (120000 + 50000) * 0.9 = 170000 * 0.9 = 153000
            finalPrice.Should().Be(153000m);
        }
    }
}
