using Microsoft.AspNetCore.SignalR;

namespace CinemaManagement.Hubs
{
    public class CinemaHub : Hub
    {
        public async Task JoinShowtimeGroup(int showtimeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Showtime_{showtimeId}");
        }

        public async Task LeaveShowtimeGroup(int showtimeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Showtime_{showtimeId}");
        }

        public async Task HoldSeat(int showtimeId, int seatId, string seatNumber)
        {
            await Clients.OthersInGroup($"Showtime_{showtimeId}").SendAsync("SeatHeld", seatId, seatNumber);
        }

        public async Task ReleaseSeat(int showtimeId, int seatId)
        {
            await Clients.OthersInGroup($"Showtime_{showtimeId}").SendAsync("SeatReleased", seatId);
        }
    }
}
