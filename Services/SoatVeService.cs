using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class SoatVeService : ISoatVeService
{
    private readonly ApplicationDbContext _db;

    public SoatVeService(ApplicationDbContext db)
    {
        _db = db;
    }

    private IQueryable<Models.Entities.Ve> QueryCoDayDu()
        => _db.Ves
            .Include(v => v.Ghe)
            .Include(v => v.DatVe)
            .Include(v => v.LichChieu)!.ThenInclude(lc => lc!.Phim)
            .Include(v => v.LichChieu)!.ThenInclude(lc => lc!.PhongChieu)!.ThenInclude(pc => pc!.RapChieu);

    public async Task<SoatVeResultDto> TraCuuAsync(string maVe, int? rapChieuIdGioiHan)
        => await KiemTraVeAsync(maVe, rapChieuIdGioiHan);

    public async Task<SoatVeResultDto> XacNhanSoatAsync(string maVe, int nhanVienId, int? rapChieuIdGioiHan)
    {
        var ve = await _db.Ves
            .Include(v => v.Ghe)
            .Include(v => v.DatVe)
            .Include(v => v.LichChieu)!.ThenInclude(lc => lc!.Phim)
            .Include(v => v.LichChieu)!.ThenInclude(lc => lc!.PhongChieu)!.ThenInclude(pc => pc!.RapChieu)
            .FirstOrDefaultAsync(v => v.MaVe == maVe.Trim());

        var ketQua = await KiemTraVeAsync(maVe, rapChieuIdGioiHan, ve);
        if (!ketQua.HopLe || ve == null)
        {
            return ketQua;
        }

        ve.TrangThai = TrangThaiVe.DaSuDung;
        ve.ThoiGianSoatVe = DateTime.Now;
        ve.NhanVienSoatVeId = nhanVienId;
        await _db.SaveChangesAsync();

        ketQua.ThongBao = "Soát vé thành công. Mời khách vào rạp.";
        ketQua.TrangThaiVe = ve.TrangThai;
        ketQua.ThoiGianSoatVe = ve.ThoiGianSoatVe;
        return ketQua;
    }

    private async Task<SoatVeResultDto> KiemTraVeAsync(string maVe, int? rapChieuIdGioiHan, Models.Entities.Ve? veDaTai = null)
    {
        var ve = veDaTai ?? await QueryCoDayDu().FirstOrDefaultAsync(v => v.MaVe == maVe.Trim());

        if (ve == null)
        {
            return new SoatVeResultDto { HopLe = false, ThongBao = "Không tìm thấy vé với mã này." };
        }

        var lichChieu = ve.LichChieu!;
        var rap = lichChieu.PhongChieu!.RapChieu!;

        var ketQua = new SoatVeResultDto
        {
            MaVe = ve.MaVe,
            TenPhim = lichChieu.Phim?.TenPhim,
            TenRap = rap.TenRap,
            TenPhong = lichChieu.PhongChieu.TenPhong,
            HangGhe = ve.Ghe?.HangGhe,
            SoGhe = ve.Ghe?.SoGhe,
            LoaiGhe = ve.Ghe?.LoaiGhe,
            DoTuoiPhim = lichChieu.Phim?.DoTuoi,
            ThoiGianChieu = lichChieu.ThoiGianBatDau,
            TrangThaiVe = ve.TrangThai,
            ThoiGianSoatVe = ve.ThoiGianSoatVe,
        };

        if (rapChieuIdGioiHan.HasValue && rap.Id != rapChieuIdGioiHan.Value)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = "Vé này thuộc suất chiếu của rạp khác.";
            return ketQua;
        }

        if (ve.DatVe == null || ve.DatVe.TrangThai != TrangThaiDatVe.DaThanhToan)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = "Đơn đặt vé chưa thanh toán hoặc đã hủy.";
            return ketQua;
        }

        if (ve.TrangThai == TrangThaiVe.DaHuy)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = "Vé này đã bị hủy.";
            return ketQua;
        }

        if (ve.TrangThai == TrangThaiVe.DaSuDung)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = $"Vé đã được soát lúc {ve.ThoiGianSoatVe:HH:mm dd/MM/yyyy}. Không thể vào thêm lần nữa.";
            return ketQua;
        }

        // Chỉ cho soát trong khoảng từ 30 phút trước giờ chiếu đến hết thời lượng chiếu,
        // tránh khách vào xem ở suất không đúng hoặc dùng vé để vào lại sau khi suất đã kết thúc từ lâu.
        var now = DateTime.Now;
        var gioMoCua = lichChieu.ThoiGianBatDau.AddMinutes(-30);
        if (now < gioMoCua)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = $"Chưa đến giờ soát vé. Suất chiếu bắt đầu lúc {lichChieu.ThoiGianBatDau:HH:mm dd/MM/yyyy}.";
            return ketQua;
        }
        if (now > lichChieu.ThoiGianKetThuc)
        {
            ketQua.HopLe = false;
            ketQua.ThongBao = "Suất chiếu đã kết thúc, vé không còn hiệu lực.";
            return ketQua;
        }

        ketQua.HopLe = true;
        ketQua.ThongBao = "Vé hợp lệ.";
        return ketQua;
    }
}
