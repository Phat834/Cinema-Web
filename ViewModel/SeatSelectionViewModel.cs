namespace CinemaManagement.ViewModel
{
    public class SeatSelectionViewModel
    {
        public Models.Showtime Showtime { get; set; }
        public Models.Movie Movie { get; set; }
        public Models.Room Room { get; set; }

        public System.Collections.Generic.List<Models.Seat> AvailableSeats { get; set; }

        public int ShowtimeId => Showtime?.ShowtimeId ?? 0;
        public int RoomId => Room?.RoomId ?? 0;

        // GIÁ
        public decimal BasePrice { get; set; }        // Giá gốc suất chiếu (đã nhân multiplier)
        public decimal PriceRegular { get; set; }     // Giá phụ loại ghế
        public decimal PriceVip { get; set; }
        public decimal PriceCouple { get; set; }

        // GIẢM GIÁ
        public decimal? DiscountPercent { get; set; }
        public System.DateTime? DiscountStart { get; set; }
        public System.DateTime? DiscountEnd { get; set; }

        // GHẾ
        public System.Collections.Generic.List<int> SelectedSeats { get; set; } = new();
        public System.Collections.Generic.List<int> PaidSeatIds { get; set; } = new();
        public System.Collections.Generic.List<int> ReservedSeatIds { get; set; } = new();

        // Thời gian giữ ghế (phút), lấy từ BookingSettings
        public int ReservationTimeoutMinutes { get; set; } = 5;
    }
}
