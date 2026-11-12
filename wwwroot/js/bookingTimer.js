// Quản lý đếm ngược giữ ghế từ cấu hình; hết giờ thì popup/redirect.
(function (global) {
  function pad(n) { return String(n).padStart(2, '0'); }

  function initBookingTimer(options) {
    const opts = options || {};
    const minutes = Number(opts.minutes || 5);
    const displayEl = document.querySelector(opts.selector || '#bookingTimer');
    const onExpire = opts.onExpire || function () {
      alert('Hết thời gian giữ ghế. Vui lòng chọn lại.');
      window.location.href = opts.redirectUrl || '/';
    };

    let remaining = Math.max(0, Math.floor(minutes * 60));
    const render = () => {
      if (!displayEl) return;
      displayEl.textContent = `${pad(Math.floor(remaining / 60))}:${pad(remaining % 60)}`;
    };

    render();
    const timerId = window.setInterval(() => {
      remaining -= 1;
      if (remaining <= 0) {
        window.clearInterval(timerId);
        if (displayEl) displayEl.textContent = '00:00';
        onExpire();
        return;
      }
      render();
    }, 1000);

    return { stop: () => window.clearInterval(timerId), getRemaining: () => remaining };
  }

  global.BookingTimer = { init: initBookingTimer };
})(window);
