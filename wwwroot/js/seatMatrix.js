// Render/điều khiển ma trận ghế: click chọn, đổi màu, tính tiền, sync hub, xác nhận.
(function (global) {
  function colorByType(type) {
    if (type === 'Vip') return { bg: '#f1c40f', fg: '#000' };
    if (type === 'Couple') return { bg: '#d63384', fg: '#fff' };
    return { bg: '#6c757d', fg: '#fff' };
  }

  function initSeatMatrix(cfg) {
    const selected = new Set();
    const totalEl = document.querySelector(cfg.totalSelector || '#totalPrice');
    const confirmBtn = document.querySelector(cfg.confirmSelector || '#confirmBooking');
    const qSeat = (id) => document.querySelector(`.seat[data-id='${id}']`);

    const updateTotal = () => {
      let total = 0;
      selected.forEach((id) => {
        const s = qSeat(id); if (!s) return;
        total += (Number(cfg.basePrice) + Number((cfg.extra || {})[s.dataset.type] || 0)) * Number(cfg.discountMul || 1);
      });
      if (totalEl) totalEl.textContent = Math.round(total).toLocaleString();
      if (confirmBtn) confirmBtn.disabled = selected.size === 0;
    };

    const markHeld = (seatId, seatNumber) => {
      const s = qSeat(seatId); if (!s || s.classList.contains('seat-booked')) return;
      selected.delete(String(seatId)); s.classList.remove('seat-selected');
      s.disabled = true; s.style.background = '#ffa500'; s.style.opacity = '0.7';
      s.title = `Đang có người chọn: ${seatNumber || ''}`; updateTotal();
    };
    const markReleased = (seatId) => {
      const s = qSeat(seatId); if (!s) return;
      s.disabled = false; s.style.opacity = ''; s.title = '';
      const c = colorByType(s.dataset.type); s.style.background = c.bg; s.style.color = c.fg;
    };

    document.querySelectorAll('.seat:not(.seat-booked)').forEach((s) => {
      s.addEventListener('click', () => {
        const id = String(s.dataset.id);
        if (s.classList.contains('seat-selected')) {
          s.classList.remove('seat-selected'); selected.delete(id); cfg.hub.releaseSeat(id);
        } else {
          s.classList.add('seat-selected'); selected.add(id); cfg.hub.holdSeat(id, s.textContent.trim());
        }
        updateTotal();
      });
    });

    document.addEventListener('seat:held', (e) => markHeld(String(e.detail.seatId), e.detail.seatNumber));
    document.addEventListener('seat:released', (e) => markReleased(String(e.detail.seatId)));

    if (confirmBtn) confirmBtn.addEventListener('click', async () => {
      const ids = Array.from(selected);
      if (cfg.hasEdgeSeatLeft && cfg.hasEdgeSeatLeft(ids)) { alert('❌ Không để trống duy nhất một ghế ở đầu hàng.'); return; }
      const res = await fetch(cfg.confirmUrl, { method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ showtimeId: Number(cfg.showtimeId), seatIds: ids.map(Number) }) });
      const data = await res.json();
      if (!data.success) alert(data.error); else window.location.href = data.redirectUrl;
    });

    updateTotal();
    return { getSelected: () => Array.from(selected) };
  }

  global.SeatMatrix = { init: initSeatMatrix };
})(window);
