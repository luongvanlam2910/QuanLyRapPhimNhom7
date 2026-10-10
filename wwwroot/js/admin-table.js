/**
 * Công cụ danh sách cho khu quản trị (jQuery): tìm nhanh (không phân biệt dấu), lọc theo cột,
 * sắp xếp theo tiêu đề, phân trang — chạy phía trình duyệt.
 *
 *  - Mọi bảng:  .admin-content table.table   (tắt bằng thuộc tính data-no-paging)
 *  - Danh sách không phải bảng: đặt data-paged-list trên thẻ chứa;
 *      data-item="<selector>"   các phần tử con là 1 dòng (mặc định: con trực tiếp)
 *      data-page-size="8"       số dòng/trang mặc định
 *      data-group-label="Phòng" + thuộc tính data-group="..." trên từng dòng => ô lọc
 */
(function ($) {
  'use strict';

  function fold(s) {
    return (s || '').toString().toLowerCase().normalize('NFD')
      .replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/\s+/g, ' ').trim();
  }

  /** Giá trị 1 ô: ưu tiên data-val (ô chứa <select>/nút), mặc định là văn bản đã gọn khoảng trắng. */
  function cellVal($c) {
    var v = $c.attr('data-val');
    return v !== undefined ? v : $.trim($c.text()).replace(/\s+/g, ' ');
  }

  /** Văn bản dùng để tìm kiếm của 1 dòng: select chỉ tính giá trị đang chọn; có cache. */
  function rowText($it) {
    var t = $it.data('ft');
    if (t === undefined) {
      var $c = $it.clone();
      $c.find('select').each(function () { $(this).replaceWith(document.createTextNode(' ' + $(this).find('option:selected').text() + ' ')); });
      t = fold($c.text());
      $it.data('ft', t);
    }
    return t;
  }

  /** Dựng thanh công cụ + phân trang cho 1 tập "dòng". */
  function attach(cfg) {
    var state = { page: 1, size: cfg.pageSize || 10, q: '', f: {} };
    if (cfg.startIndex) { var si = cfg.startIndex(); if (si > 0) state.page = Math.floor(si / state.size) + 1; }

    var $top = $('<div class="tbl-top"></div>');
    var $search = $('<div class="tbl-search"><i class="fas fa-magnifying-glass"></i>' +
      '<input type="search" class="form-control form-control-sm" placeholder="Tìm kiếm..."></div>');
    var $filters = $('<div class="tbl-filters"></div>');
    var $reset = $('<button type="button" class="btn btn-sm btn-outline-secondary"><i class="fas fa-rotate-left"></i> Xóa lọc</button>');
    var $size = $('<div class="tbl-size"><span>Hiển thị</span><select class="form-select form-select-sm">' +
      '<option>5</option><option>8</option><option>10</option><option>25</option><option>50</option><option>100</option></select><span>dòng</span></div>');
    $size.find('select').val(String(state.size));
    if ($size.find('select').val() === null) $size.find('select').append('<option>' + state.size + '</option>').val(String(state.size));

    $top.append($search, $filters, $reset, $size);
    var $bottom = $('<div class="tbl-bottom"><div class="tbl-info"></div><ul class="pagination pagination-sm mb-0"></ul></div>');
    cfg.before.before($top);
    cfg.after.after($bottom);

    (cfg.filters || []).forEach(function (f, idx) {
      var $s = $('<select class="form-select form-select-sm" data-f="' + idx + '"></select>');
      $s.append($('<option value="">' + f.label + ': tất cả</option>'));
      f.values.forEach(function (v) { $s.append($('<option></option>').attr('value', v).text(v)); });
      $s.on('change', function () { state.f[idx] = this.value; state.page = 1; render(); });
      $filters.append($s);
    });

    function render() {
      var q = fold(state.q);
      var $items = cfg.items();
      var $match = $items.filter(function () {
        var $it = $(this);
        if (q !== '' && rowText($it).indexOf(q) === -1) return false;
        for (var i = 0; i < (cfg.filters || []).length; i++) {
          var v = state.f[i];
          if (v && cfg.filters[i].valueOf($it) !== v) return false;
        }
        return true;
      });
      var total = $match.length, pages = Math.max(1, Math.ceil(total / state.size));
      if (state.page > pages) state.page = pages;
      var from = (state.page - 1) * state.size;
      $items.hide();
      $match.slice(from, from + state.size).show();
      if (cfg.onRender) cfg.onRender($match, total);

      $bottom.find('.tbl-info').text(total === 0 ? 'Không có dữ liệu phù hợp'
        : 'Hiển thị ' + (from + 1) + '–' + Math.min(from + state.size, total) + ' / ' + total + ' dòng');

      var $ul = $bottom.find('ul').empty();
      function item(label, page, o) {
        o = o || {};
        var $li = $('<li class="page-item"></li>').toggleClass('active', !!o.active).toggleClass('disabled', !!o.disabled);
        $('<a class="page-link" href="#"></a>').html(label).appendTo($li).on('click', function (e) {
          e.preventDefault();
          if (o.disabled || o.active) return;
          state.page = page; render();
        });
        $ul.append($li);
      }
      item('<i class="fas fa-angles-left"></i>', 1, { disabled: state.page === 1 });
      item('<i class="fas fa-angle-left"></i>', state.page - 1, { disabled: state.page === 1 });
      var s = Math.max(1, state.page - 2), e = Math.min(pages, state.page + 2);
      if (s > 1) item('…', s - 1);
      for (var p = s; p <= e; p++) item(p, p, { active: p === state.page });
      if (e < pages) item('…', e + 1);
      item('<i class="fas fa-angle-right"></i>', state.page + 1, { disabled: state.page === pages });
      item('<i class="fas fa-angles-right"></i>', pages, { disabled: state.page === pages });
    }

    $search.find('input').on('input', function () { state.q = this.value; state.page = 1; render(); });
    $size.find('select').on('change', function () { state.size = parseInt(this.value, 10); state.page = 1; render(); });
    $reset.on('click', function () {
      state.q = ''; state.f = {}; state.page = 1;
      $search.find('input').val(''); $filters.find('select').val(''); render();
    });
    render();
    return { render: render };
  }

  // ---------- Bảng ----------
  function sortKey(txt) {
    var t = $.trim(txt);
    var m = t.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})(?:\s+(\d{1,2}):(\d{2}))?/); // dd/MM/yyyy [HH:mm]
    if (m) return Number(m[3] + ('0' + m[2]).slice(-2) + ('0' + m[1]).slice(-2) + ('0' + (m[4] || 0)).slice(-2) + (m[5] || '00'));
    var m2 = t.match(/^(\d{1,2}):(\d{2})/);                                      // HH:mm
    if (m2) return Number(m2[1]) * 60 + Number(m2[2]);
    if (/^[\d.,\s₫đ%+\-]+$/.test(t) && /\d/.test(t)) return Number(t.replace(/[^\d\-]/g, ''));
    return fold(t);
  }

  function buildTable($table) {
    var $tbody = $table.children('tbody');
    var rows = function () { return $tbody.children('tr'); };
    var $r = rows();
    if ($r.length === 0) return;

    var $heads = $table.find('> thead > tr:first > th');
    var filters = [];
    if ($r.length > 5) {
      $heads.each(function (i) {
        var label = $.trim($(this).text());
        if (!label || /thao tác|hành động|^#$|^stt$|^mã/i.test(label)) return;
        var vals = {}, n = 0, ok = true;
        $r.each(function () {
          var $c = $(this).children('td').eq(i);
          if (!$c.attr('data-val') && $c.find('form,button,a.btn,input').length) { ok = false; return false; }
          var v = cellVal($c);
          if (v.length > 40) { ok = false; return false; }
          if (!vals[v]) { vals[v] = 1; n++; }
        });
        if (ok && n >= 2 && n <= 30 && n < $r.length * 0.6) {
          filters.push({
            label: label,
            values: Object.keys(vals).sort(function (a, b) { return a.localeCompare(b, 'vi'); }),
            valueOf: function ($row) { return cellVal($row.children('td').eq(i)); }
          });
        }
      });
    }

    var $anchor = $table.closest('.table-responsive');
    if ($anchor.length === 0) $anchor = $table;
    var api = attach({ items: rows, before: $anchor, after: $anchor, filters: filters, pageSize: 10 });

    // Sắp xếp khi bấm tiêu đề cột
    $heads.each(function (i) {
      var $th = $(this), label = $.trim($th.text());
      if (!label || /thao tác|hành động/i.test(label)) return;
      var $sample = rows().first().children('td').eq(i);
      if ($sample.find('form,button,a.btn').length) return;
      var dir = 0;
      $th.addClass('th-sort').attr('title', 'Bấm để sắp xếp').append(' <i class="fas fa-sort sort-ico"></i>');
      $th.on('click', function () {
        dir = dir === 1 ? -1 : 1;
        $heads.find('.sort-ico').attr('class', 'fas fa-sort sort-ico');
        $th.find('.sort-ico').attr('class', 'fas sort-ico ' + (dir === 1 ? 'fa-sort-up' : 'fa-sort-down'));
        var sorted = rows().get().sort(function (a, b) {
          var ka = sortKey($(a).children('td').eq(i).text()), kb = sortKey($(b).children('td').eq(i).text());
          if (typeof ka === 'number' && typeof kb === 'number') return (ka - kb) * dir;
          return String(ka).localeCompare(String(kb), 'vi', { numeric: true }) * dir;
        });
        $tbody.append(sorted);
        api.render();
      });
    });
  }

  // ---------- Danh sách không phải bảng ----------
  function buildList($box) {
    var sel = $box.attr('data-item');
    var items = function () { return sel ? $box.find(sel) : $box.children(); };
    if (items().length === 0) return;

    var filters = [];
    var gl = $box.attr('data-group-label');
    if (gl) {
      var vals = {};
      items().each(function () { var g = $(this).attr('data-group'); if (g) vals[g] = 1; });
      var keys = Object.keys(vals).sort(function (a, b) { return a.localeCompare(b, 'vi', { numeric: true }); });
      if (keys.length > 1) filters.push({ label: gl, values: keys, valueOf: function ($it) { return $it.attr('data-group'); } });
    }
    attach({
      items: items, filters: filters,
      before: $box.attr('data-toolbar') === 'inside' ? items().first() : $box,
      after: $box.attr('data-toolbar') === 'inside' ? items().last() : $box,
      pageSize: parseInt($box.attr('data-page-size') || '8', 10),
      startIndex: function () { return items().index(items().filter('.active').first()); }
    });
  }

  $(function () {
    $('.admin-content table.table').not('[data-no-paging]').each(function () { buildTable($(this)); });
    $('.admin-content [data-paged-list]').each(function () { buildList($(this)); });
  });
})(jQuery);
