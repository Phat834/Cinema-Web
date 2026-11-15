namespace CinemaManagement.ViewModel
{
    public class QRPaymentViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string QrCodeImageUrl { get; set; } = string.Empty;
        public int CountdownSeconds { get; set; } = 300;
    }
}
