// Kết nối SignalR cho chọn ghế: join group, hold/release và bắn sự kiện DOM.
(function (global) {
  function initSeatHubClient(options) {
    const { hubUrl, showtimeId } = options || {};
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl || '/cinemaHub')
      .withAutomaticReconnect()
      .build();

    // Phát sự kiện ra DOM để module UI xử lý.
    connection.on('SeatHeld', (seatId, seatNumber) => {
      document.dispatchEvent(new CustomEvent('seat:held', { detail: { seatId, seatNumber } }));
    });
    connection.on('SeatReleased', (seatId) => {
      document.dispatchEvent(new CustomEvent('seat:released', { detail: { seatId } }));
    });

    const ready = connection.start()
      .then(() => connection.invoke('JoinShowtimeGroup', Number(showtimeId)))
      .catch((err) => { console.error('SignalR connect error:', err); throw err; });

    return {
      connection,
      ready,
      holdSeat: (seatId, seatNumber) => ready.then(() =>
        connection.invoke('HoldSeat', Number(showtimeId), Number(seatId), String(seatNumber || ''))),
      releaseSeat: (seatId) => ready.then(() =>
        connection.invoke('ReleaseSeat', Number(showtimeId), Number(seatId)))
    };
  }

  global.SeatHubClient = { init: initSeatHubClient };
})(window);
