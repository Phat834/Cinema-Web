namespace CinemaManagement.Models
{
    /// <summary>
    /// Cấu hình thời gian giữ chỗ (seat reservation timeout).
    /// Đọc từ appsettings.json under "BookingSettings".
    /// </summary>
    public class BookingSettings
    {
        public int ReservationTimeoutMinutes { get; set; } = 5;
    }
}