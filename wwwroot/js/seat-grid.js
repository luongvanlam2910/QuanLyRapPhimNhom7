/**
 * Sơ đồ ghế dùng chung cho khách (DatVe/ChonGhe) và quầy (Admin/BanVe) — cùng một giao diện, cùng cách tô màu trạng thái.
 *   SeatGrid.render(container, seats)        dựng các hàng ghế từ [{gheId, hangGhe, soGhe, loaiGhe, trangThai}]
 *   SeatGrid.applyState(list, onFree)        cập nhật trạng thái từ [{gheId, trangThai}] (làm mới định kỳ);
 *                                            onFree(btn) được gọi khi 1 ghế vừa được nhả và chưa gắn sự kiện click
 * Trạng thái: TRONG | DA_DAT | GIU_TAM | BAO_TRI
 */
window.SeatGrid = (function () {
  'use strict';

  function loaiClass(loai) { return loai === 'VIP' ? 'seat-vip' : loai === 'Đôi' ? 'seat-doi' : 'seat-thuong'; }

  function render(container, seats) {
    if (!container) return;
    container.innerHTML = '';
    if (!seats || seats.length === 0) {
      container.innerHTML = '<p style="color:#666;font-size:0.85rem;text-align:center">Không tìm thấy ghế cho phòng chiếu này</p>';
      return;
    }
    const rows = {};
    seats.forEach(function (s) { (rows[s.hangGhe] = rows[s.hangGhe] || []).push(s); });

    Object.keys(rows).sort().forEach(function (hang) {
      const rowDiv = document.createElement('div');
      rowDiv.className = 'seat-row';
      rowDiv.style.marginTop = '8px';

      const label = document.createElement('div');
      label.className = 'seat-row-label';
      label.textContent = hang;
      rowDiv.appendChild(label);

      rows[hang].sort(function (a, b) { return a.soGhe - b.soGhe; }).forEach(function (seat) {
        const status = seat.trangThai || 'TRONG';
        const loai = seat.loaiGhe || 'Thường';
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.dataset.maGhe = seat.gheId;
        btn.dataset.hang = hang;
        btn.dataset.so = seat.soGhe;
        btn.dataset.loaiGhe = loai;
        btn.title = hang + seat.soGhe + ' - ' + loai;
        btn.textContent = seat.soGhe;

        let cls = 'seat ';
        if (status === 'DA_DAT') { cls += 'seat-booked'; btn.disabled = true; btn.title += ' (Đã đặt)'; }
        else if (status === 'GIU_TAM') { cls += 'seat-hold'; btn.disabled = true; btn.title += ' (Đang giữ)'; }
        else if (status === 'BAO_TRI') { cls += 'seat-maintenance'; btn.disabled = true; btn.title += ' (Bảo trì)'; }
        else cls += loaiClass(loai);
        btn.className = cls;
        rowDiv.appendChild(btn);
      });

      rowDiv.appendChild(label.cloneNode(true));
      container.appendChild(rowDiv);
    });
  }

  function applyState(list, onFree) {
    list.forEach(function (s) {
      const btn = document.querySelector('.seat[data-ma-ghe="' + s.gheId + '"]');
      if (!btn || btn.classList.contains('seat-selected') || btn.classList.contains('seat-maintenance')) return;
      btn.classList.remove('seat-booked', 'seat-hold', 'seat-thuong', 'seat-vip', 'seat-doi');
      if (s.trangThai === 'DA_DAT') { btn.classList.add('seat-booked'); btn.disabled = true; }
      else if (s.trangThai === 'GIU_TAM') { btn.classList.add('seat-hold'); btn.disabled = true; }
      else {
        btn.classList.add(loaiClass(btn.dataset.loaiGhe));
        btn.disabled = false;
        if (onFree) onFree(btn);
      }
    });
  }

  return { render: render, applyState: applyState, loaiClass: loaiClass };
})();
