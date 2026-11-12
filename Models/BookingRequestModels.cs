namespace CinemaManagement.Models
{
    public class BookingRequest { public int ShowtimeId { get; set; } public List<int> SeatIds { get; set; } = new(); }
    public class PaymentRequest { public string OrderId { get; set; } = string.Empty; public long Amount { get; set; } }
    public class ComboRequest { public string OrderId { get; set; } = string.Empty; public Dictionary<int, int> SelectedCombos { get; set; } = new(); public decimal ComboTotal { get; set; } }
    public class CashPaymentRequest { public string OrderId { get; set; } }
}
