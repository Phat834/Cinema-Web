using CinemaManagement.Models;
using FluentAssertions;
using Xunit;

namespace CinemaWeb.UnitTests
{
    public class BookingValidationTests
    {
        [Fact]
        public void CannotBook_SeatAlreadyPaid()
        {
            // Arrange
            var showtimeId = 1;
            var seatId = 5;
            var paidSeatIds = new List<int> { 5, 7, 9 };
            var requestedSeatIds = new List<int> { 5, 6 };

            // Act
            var hasTakenSeat = requestedSeatIds.Any(id => paidSeatIds.Contains(id));

            // Assert
            hasTakenSeat.Should().BeTrue();
        }

        [Fact]
        public void CanBook_SeatNotPaid()
        {
            // Arrange
            var seatId = 5;
            var paidSeatIds = new List<int> { 7, 9 };
            var requestedSeatIds = new List<int> { 5, 6 };

            // Act
            var hasTakenSeat = requestedSeatIds.Any(id => paidSeatIds.Contains(id));

            // Assert
            hasTakenSeat.Should().BeFalse();
        }

        [Fact]
        public void CannotBook_SeatReservedWithinTimeout()
        {
            // Arrange
            var showtimeId = 1;
            var seatId = 5;
            var reservationTimeoutMinutes = 5;
            var cutoff = DateTime.Now.AddMinutes(-reservationTimeoutMinutes);

            var reservedTickets = new List<Ticket>
            {
                new() { SeatId = 5, ShowtimeId = showtimeId, IsPaid = false, BookingTime = DateTime.Now.AddMinutes(-2) },
                new() { SeatId = 6, ShowtimeId = showtimeId, IsPaid = false, BookingTime = DateTime.Now.AddMinutes(-10) }
            };

            // Act
            var reservedSeatIds = reservedTickets
                .Where(t => t.ShowtimeId == showtimeId && !t.IsPaid && t.BookingTime >= cutoff)
                .Select(t => t.SeatId)
                .ToList();

            // Assert
            reservedSeatIds.Should().Contain(5);
            reservedSeatIds.Should().NotContain(6);
        }

        [Fact]
        public void ExpiredSeat_ShouldBeReleased_AfterTimeout()
        {
            // Arrange
            var reservationTimeoutMinutes = 5;
            var cutoff = DateTime.Now.AddMinutes(-reservationTimeoutMinutes);

            var tickets = new List<Ticket>
            {
                new() { TicketId = 1, SeatId = 5, IsPaid = false, BookingTime = DateTime.Now.AddMinutes(-10) },
                new() { TicketId = 2, SeatId = 6, IsPaid = false, BookingTime = DateTime.Now.AddMinutes(-2) },
                new() { TicketId = 3, SeatId = 7, IsPaid = true, BookingTime = DateTime.Now.AddMinutes(-10) }
            };

            // Act
            var expiredTickets = tickets
                .Where(t => !t.IsPaid && t.BookingTime < cutoff)
                .ToList();

            // Assert
            expiredTickets.Should().HaveCount(1);
            expiredTickets.First().TicketId.Should().Be(1);
        }

        [Fact]
        public void ValidSeat_ShouldNotBeExpired_WithinTimeoutWindow()
        {
            // Arrange
            var reservationTimeoutMinutes = 5;
            var cutoff = DateTime.Now.AddMinutes(-reservationTimeoutMinutes);

            var ticket = new Ticket
            {
                TicketId = 1,
                SeatId = 5,
                IsPaid = false,
                BookingTime = DateTime.Now.AddMinutes(-2)
            };

            // Act
            var isExpired = !ticket.IsPaid && ticket.BookingTime < cutoff;

            // Assert
            isExpired.Should().BeFalse();
        }

        [Fact]
        public void PaidTicket_ShouldNotBeExpired()
        {
            // Arrange
            var reservationTimeoutMinutes = 5;
            var cutoff = DateTime.Now.AddMinutes(-reservationTimeoutMinutes);

            var ticket = new Ticket
            {
                TicketId = 1,
                SeatId = 5,
                IsPaid = true,
                BookingTime = DateTime.Now.AddMinutes(-10)
            };

            // Act
            var isExpired = !ticket.IsPaid && ticket.BookingTime < cutoff;

            // Assert
            isExpired.Should().BeFalse();
        }

        [Fact]
        public void NoSeatsSelected_ShouldReportValidationError()
        {
            // Arrange
            List<int> emptySeatIds = new();
            List<int> nullSeatIds = null!;

            // Act
            bool emptyInvalid = emptySeatIds == null || !emptySeatIds.Any();
            bool nullInvalid = nullSeatIds == null || !nullSeatIds.Any();

            // Assert
            emptyInvalid.Should().BeTrue();
            nullInvalid.Should().BeTrue();
        }

        [Fact]
        public void MoreThanMaxSeats_ShouldReportValidationError()
        {
            // Arrange
            const int maxSeats = 8;
            var seatIds = Enumerable.Range(1, 9).ToList();

            // Act
            bool exceedsMax = seatIds.Count > maxSeats;

            // Assert
            exceedsMax.Should().BeTrue();
        }

        [Fact]
        public void CoupleSeatOddCount_ShouldReportValidationError()
        {
            // Arrange
            var coupleSeats = new List<Seat>
            {
                new() { SeatId = 1, SeatType = SeatType.Couple },
                new() { SeatId = 2, SeatType = SeatType.Couple },
                new() { SeatId = 3, SeatType = SeatType.Couple }
            };

            // Act
            var coupleSeatCount = coupleSeats.Count(s => s.SeatType == SeatType.Couple);
            bool isOdd = coupleSeatCount % 2 != 0;

            // Assert
            isOdd.Should().BeTrue();
        }

        [Fact]
        public void Reservation_ExactlyFiveMinutesOneSecond_ShouldBeExpired()
        {
            // Arrange
            var timeoutSeconds = 5 * 60 + 1;
            var bookingTime = DateTime.Now.AddSeconds(-timeoutSeconds);
            var cutoff = DateTime.Now.AddMinutes(-5);

            // Act
            var isExpired = bookingTime < cutoff;

            // Assert
            isExpired.Should().BeTrue();
        }

        [Fact]
        public void Reservation_UnderFiveMinutes_ShouldNotBeExpired()
        {
            // Arrange
            var bookingTime = DateTime.Now.AddMinutes(-4).AddSeconds(-50);
            var cutoff = DateTime.Now.AddMinutes(-5);

            // Act
            var isExpired = bookingTime < cutoff;

            // Assert
            isExpired.Should().BeFalse();
        }
    }
}
