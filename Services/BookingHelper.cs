using CinemaManagement.Models;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using QRCoder;

namespace CinemaManagement.Services
{
    internal static class BookingHelper
    {
        public static string GenerateQrCodeBase64(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var ms = new MemoryStream();
            using var bitmap = qrCode.GetGraphic(20);
            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string BuildTicketEmailHtml(string orderId, List<Ticket> tickets)
        {
            var sb = new StringBuilder();
            sb.Append("<h2>🎉 Thanh toán thành công</h2>");
            sb.Append($"<p><b>Mã đơn:</b> {orderId}</p>");
            sb.Append("<hr/>");
            foreach (var t in tickets)
            {
                sb.Append("<div style='border:1px solid #ddd;padding:10px;margin-bottom:10px'>");
                sb.Append($"<p><b>Phim:</b> {t.Showtime?.Movie?.Title}</p>");
                sb.Append($"<p><b>Rạp:</b> {t.Seat?.Room?.Theater?.Name}</p>");
                sb.Append($"<p><b>Phòng:</b> {t.Seat?.Room?.Name}</p>");
                sb.Append($"<p><b>Ghế:</b> {t.Seat?.SeatNumber}</p>");
                sb.Append($"<p><b>Ngày:</b> {t.Showtime?.StartTime:dd/MM/yyyy}</p>");
                sb.Append($"<p><b>Giờ:</b> {t.Showtime?.StartTime:HH:mm}</p>");
                sb.Append($"<p><b>Giá:</b> {t.Price:#,##0} VND</p>");
                sb.Append("</div>");
            }
            return sb.ToString();
        }
    }
}
