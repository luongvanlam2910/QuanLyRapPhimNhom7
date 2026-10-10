using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class BaoCaoService : IBaoCaoService
{
    private readonly ApplicationDbContext _db;

    public BaoCaoService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int? rapChieuId = null)
    {
        var veQuery = _db.Ves.Where(v => v.TrangThai != TrangThaiVe.DaHuy);
        var datVeQuery = _db.DatVes.AsQueryable();
        var lichChieuQuery = _db.LichChieus.AsQueryable();

        if (rapChieuId.HasValue)
        {
            veQuery = veQuery.Where(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value);
            datVeQuery = datVeQuery.Where(d => d.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value));
            lichChieuQuery = lichChieuQuery.Where(l => l.PhongChieu!.RapChieuId == rapChieuId.Value);
        }

        var tongVeBan = await veQuery.CountAsync();
        var tongDoanhThu = await datVeQuery
            .Where(d => d.TrangThai == TrangThaiDatVe.DaThanhToan)
            .SumAsync(d => (decimal?)d.TongTien) ?? 0m;

        return new DashboardStatsDto
        {
            TongVeBan = tongVeBan,
            TongDoanhThu = tongDoanhThu,
            SoSuatMoBan = await lichChieuQuery.LongCountAsync(l => l.TrangThai == TrangThaiLichChieu.MoBan),
            DaDatVe = await datVeQuery.LongCountAsync(d => d.TrangThai == TrangThaiDatVe.DaThanhToan),
            ChoThanhToan = await datVeQuery.LongCountAsync(d => d.TrangThai == TrangThaiDatVe.ChoThanhToan),
            TongPhim = await _db.Phims.LongCountAsync(),
            TongKhachHang = await _db.KhachHangs.LongCountAsync(),
        };
    }

    public async Task<List<DoanhThuPhimDto>> GetDoanhThuTheoPhimAsync(int? rapChieuId = null)
    {
        var veQuery = _db.Ves
            .Where(v => v.TrangThai != TrangThaiVe.DaHuy && v.DatVe!.TrangThai == TrangThaiDatVe.DaThanhToan);

        if (rapChieuId.HasValue)
        {
            veQuery = veQuery.Where(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value);
        }

        var query =
            from v in veQuery
            group v by new { v.LichChieu!.PhimId, v.LichChieu.Phim!.TenPhim } into g
            select new DoanhThuPhimDto
            {
                PhimId = g.Key.PhimId,
                TenPhim = g.Key.TenPhim,
                TongDoanhThu = g.Sum(x => x.GiaVe),
                SoVeBan = g.Count()
            };

        return await query.OrderByDescending(x => x.TongDoanhThu).ToListAsync();
    }

    public async Task<List<DoanhThuRapDto>> GetDoanhThuTheoRapAsync(int? rapChieuId = null)
    {
        var veQuery = _db.Ves
            .Where(v => v.TrangThai != TrangThaiVe.DaHuy && v.DatVe!.TrangThai == TrangThaiDatVe.DaThanhToan);

        if (rapChieuId.HasValue)
        {
            veQuery = veQuery.Where(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value);
        }

        var query =
            from v in veQuery
            group v by new
            {
                v.LichChieu!.PhongChieu!.RapChieuId,
                v.LichChieu.PhongChieu.RapChieu!.TenRap
            } into g
            select new DoanhThuRapDto
            {
                RapChieuId = g.Key.RapChieuId,
                TenRap = g.Key.TenRap,
                TongDoanhThu = g.Sum(x => x.GiaVe),
                SoVeBan = g.Count()
            };

        return await query.OrderByDescending(x => x.TongDoanhThu).ToListAsync();
    }

    public async Task<List<DoanhThuPhuongThucDto>> GetDoanhThuTheoPhuongThucAsync(int? rapChieuId = null)
    {
        var ttQuery = _db.ThanhToans.Where(t => t.TrangThai == TrangThaiThanhToan.ThanhCong);

        if (rapChieuId.HasValue)
        {
            ttQuery = ttQuery.Where(t => t.DatVe!.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value));
        }

        var query =
            from t in ttQuery
            group t by t.PhuongThuc into g
            select new DoanhThuPhuongThucDto
            {
                PhuongThuc = g.Key ?? "Khác",
                TongTien = g.Sum(x => x.SoTien),
                SoGiaoDich = g.Count()
            };

        return await query.OrderByDescending(x => x.TongTien).ToListAsync();
    }

    public async Task<decimal> GetDoanhThuSuatAsync(int lichChieuId)
    {
        return await _db.Ves
            .Where(v => v.LichChieuId == lichChieuId && v.TrangThai != TrangThaiVe.DaHuy)
            .SumAsync(v => (decimal?)v.GiaVe) ?? 0m;
    }
}
