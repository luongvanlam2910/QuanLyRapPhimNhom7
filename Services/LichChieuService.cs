using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class LichChieuService : ILichChieuService
{
    private readonly ApplicationDbContext _db;
    private readonly IDatVeService _datVeService;

    public LichChieuService(ApplicationDbContext db, IDatVeService datVeService)
    {
        _db = db;
        _datVeService = datVeService;
    }

    public async Task<List<LichChieu>> FindAllAsync()
        => await _db.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
            .OrderByDescending(l => l.ThoiGianBatDau)
            .ToListAsync();

    public async Task<LichChieu?> FindByIdAsync(int id)
        => await _db.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<LichChieu> GetByIdAsync(int id)
        => await FindByIdAsync(id) ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", id);

    public async Task<List<LichChieu>> FindByPhimRapNgayAsync(int phimId, int rapChieuId, DateTime ngay)
    {
        var startOfDay = ngay.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _db.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
            .Where(l => l.PhimId == phimId
                && l.PhongChieu!.RapChieuId == rapChieuId
                && l.TrangThai == TrangThaiLichChieu.MoBan
                && l.ThoiGianBatDau >= startOfDay
                && l.ThoiGianBatDau < endOfDay)
            .OrderBy(l => l.ThoiGianBatDau)
            .ToListAsync();
    }

    public async Task<List<LichChieu>> FindByPhimAsync(int phimId)
        => await _db.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
            .Where(l => l.PhimId == phimId && l.TrangThai == TrangThaiLichChieu.MoBan)
            .OrderBy(l => l.ThoiGianBatDau)
            .ToListAsync();

    public async Task<LichChieu> SaveAsync(LichChieu lichChieu)
    {
        if (lichChieu.ThoiGianKetThuc <= lichChieu.ThoiGianBatDau)
        {
            throw new BusinessException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");
        }

        if (lichChieu.GiaVe < 0)
        {
            throw new BusinessException("Giá vé phải lớn hơn hoặc bằng 0");
        }

        var phim = await _db.Phims.FindAsync(lichChieu.PhimId)
            ?? throw new ResourceNotFoundException("Phim", "Id", lichChieu.PhimId);

        if (phim.TrangThai == TrangThaiPhim.NgungChieu)
        {
            throw new BusinessException($"Phim '{phim.TenPhim}' đã ngừng chiếu, không thể xếp lịch.");
        }

        // Tính cả thời gian dọn phòng sau khi chiếu xong để tránh 2 suất nối sát nhau không kịp dọn.
        const int phutDonPhong = 15;
        var batDauCanKiemTra = lichChieu.ThoiGianBatDau.AddMinutes(-phutDonPhong);
        var ketThucCanKiemTra = lichChieu.ThoiGianKetThuc.AddMinutes(phutDonPhong);

        // Suất đã hủy không tính là xung đột.
        var conflicts = await _db.LichChieus
            .Where(l => l.PhongChieuId == lichChieu.PhongChieuId
                && l.Id != lichChieu.Id
                && l.TrangThai != TrangThaiLichChieu.DaHuy
                && l.ThoiGianBatDau < ketThucCanKiemTra
                && l.ThoiGianKetThuc > batDauCanKiemTra)
            .AnyAsync();

        if (conflicts)
        {
            throw new BusinessException("Phòng chiếu đã có suất chiếu khác trong khoảng thời gian này (đã tính thời gian dọn phòng).");
        }

        if (lichChieu.Id == 0)
        {
            if (lichChieu.ThoiGianBatDau <= DateTime.Now)
            {
                throw new BusinessException("Thời gian bắt đầu phải ở tương lai.");
            }

            _db.LichChieus.Add(lichChieu);
            await _db.SaveChangesAsync();
            lichChieu.MaLich = $"LC{lichChieu.Id:D5}";
        }
        else if (_db.Entry(lichChieu).State == EntityState.Detached)
        {
            _db.LichChieus.Update(lichChieu);
        }

        await _db.SaveChangesAsync();
        return lichChieu;
    }

    public async Task DeleteAsync(int id)
    {
        var lc = await _db.LichChieus
            .Include(l => l.Ves)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", id);

        var coVeChuaHuy = lc.Ves.Any(v => v.TrangThai != TrangThaiVe.DaHuy);
        if (coVeChuaHuy)
        {
            // Đã có khách đặt vé: không thể xóa cứng (vi phạm FK + mất dữ liệu đã bán).
            // Chuyển sang hủy suất chiếu, kèm hủy toàn bộ vé liên quan và hoàn tiền.
            foreach (var ve in lc.Ves.Where(v => v.TrangThai != TrangThaiVe.DaHuy))
            {
                ve.TrangThai = TrangThaiVe.DaHuy;
            }

            var datVeIds = await _db.Ves
                .Where(v => v.LichChieuId == id)
                .Select(v => v.DatVeId)
                .Distinct()
                .ToListAsync();

            foreach (var datVeId in datVeIds)
            {
                await _datVeService.HuyDatVeByStaffAsync(datVeId);
            }

            lc.TrangThai = TrangThaiLichChieu.DaHuy;
            await _db.SaveChangesAsync();
            throw new BusinessException($"Suất chiếu đã có vé bán ra nên không thể xóa — đã chuyển sang trạng thái \"{TrangThaiLichChieu.DaHuy}\" và hoàn vé cho khách.");
        }

        _db.LichChieus.Remove(lc);
        await _db.SaveChangesAsync();
    }

    private static readonly string[] TrangThaiHopLe =
        { TrangThaiLichChieu.MoBan, "Tạm dừng", TrangThaiLichChieu.DaHuy, TrangThaiLichChieu.DaKetThuc };

    /// <summary>
    /// Đổi trạng thái suất chiếu có kiểm soát:
    ///   • → "Đã hủy": hủy toàn bộ đơn/vé đã bán và hoàn tiền (trước đây chỉ đổi chữ → ghế bị khóa vĩnh viễn, khách không được hoàn).
    ///   • "Đã hủy" → trạng thái khác: không cho mở lại (vé đã hoàn, ghế có thể đã bán cho người khác).
    ///   • Mở bán lại suất đã bắt đầu: không cho phép.
    /// </summary>
    public async Task UpdateTrangThaiAsync(int id, string trangThai)
    {
        if (!TrangThaiHopLe.Contains(trangThai))
        {
            throw new BusinessException("Trạng thái suất chiếu không hợp lệ.");
        }

        var lc = await _db.LichChieus.Include(l => l.Ves).FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", id);

        if (lc.TrangThai == trangThai)
        {
            return;
        }

        if (lc.TrangThai == TrangThaiLichChieu.DaHuy)
        {
            throw new BusinessException("Suất chiếu đã hủy không thể mở lại (vé đã được hoàn, ghế có thể đã được bán). Hãy tạo suất chiếu mới.");
        }

        if (trangThai == TrangThaiLichChieu.DaHuy)
        {
            var datVeIds = lc.Ves.Where(v => v.TrangThai != TrangThaiVe.DaHuy).Select(v => v.DatVeId).Distinct().ToList();
            foreach (var datVeId in datVeIds)
            {
                try
                {
                    await _datVeService.HuyDatVeByStaffAsync(datVeId);
                }
                catch (BusinessException)
                {
                    // đơn đã hủy trước đó: bỏ qua
                }
            }
        }
        else if (trangThai == TrangThaiLichChieu.MoBan && lc.ThoiGianBatDau <= DateTime.Now)
        {
            throw new BusinessException("Suất chiếu đã bắt đầu hoặc đã kết thúc, không thể mở bán lại.");
        }

        lc.TrangThai = trangThai;
        await _db.SaveChangesAsync();
    }

    /// <summary>Sửa giờ chiếu / giá vé. Suất đã có vé bán ra chỉ được đổi giá vé (không đổi giờ).</summary>
    public async Task<LichChieu> SuaAsync(int id, DateTime batDau, DateTime ketThuc, decimal giaVe)
    {
        var lc = await _db.LichChieus.Include(l => l.Ves).FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", id);

        if (lc.TrangThai == TrangThaiLichChieu.DaHuy)
        {
            throw new BusinessException("Suất chiếu đã hủy không thể sửa.");
        }
        if (lc.ThoiGianBatDau <= DateTime.Now)
        {
            throw new BusinessException("Suất chiếu đã bắt đầu hoặc đã kết thúc, không thể sửa.");
        }

        var doiGio = lc.ThoiGianBatDau != batDau || lc.ThoiGianKetThuc != ketThuc;
        if (doiGio)
        {
            if (lc.Ves.Any(v => v.TrangThai != TrangThaiVe.DaHuy))
            {
                throw new BusinessException("Suất chiếu đã có vé bán ra nên không thể đổi giờ chiếu. Bạn chỉ có thể đổi giá vé (áp dụng cho vé bán sau).");
            }
            if (batDau <= DateTime.Now)
            {
                throw new BusinessException("Thời gian bắt đầu phải ở tương lai.");
            }
        }

        lc.ThoiGianBatDau = batDau;
        lc.ThoiGianKetThuc = ketThuc;
        lc.GiaVe = giaVe;
        return await SaveAsync(lc); // dùng lại kiểm tra: giờ hợp lệ, giá >= 0, không trùng suất khác trong phòng
    }
}
