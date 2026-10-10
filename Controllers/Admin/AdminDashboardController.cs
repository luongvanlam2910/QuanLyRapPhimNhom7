using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "NhanVienHoacQuanLy")]
[Route("Admin")]
public class AdminDashboardController : Controller
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly ICurrentStaffService _currentStaffService;
    private readonly ApplicationDbContext _db;

    public AdminDashboardController(IBaoCaoService baoCaoService, ICurrentStaffService currentStaffService, ApplicationDbContext db)
    {
        _baoCaoService = baoCaoService;
        _currentStaffService = currentStaffService;
        _db = db;
    }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Index()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();

        if (!_currentStaffService.LaQuanLyTroLen)
        {
            return await DashboardNhanVienAsync(rapChieuId);
        }

        ViewBag.Stats = await _baoCaoService.GetDashboardStatsAsync(rapChieuId);
        ViewBag.DoanhThuPhim = await _baoCaoService.GetDoanhThuTheoPhimAsync(rapChieuId);
        ViewBag.DoanhThuPhuongThuc = await _baoCaoService.GetDoanhThuTheoPhuongThucAsync(rapChieuId);
        ViewBag.PageTitle = "Dashboard";
        return View();
    }

    /// <summary>Nhân viên thường chỉ thấy việc của chính mình (số đơn/vé đã bán, vé đã soát) và suất chiếu hôm nay — không có doanh thu.</summary>
    private async Task<IActionResult> DashboardNhanVienAsync(int? rapChieuId)
    {
        var nvId = await _currentStaffService.GetNhanVienIdAsync();
        var homNay = DateTime.Today;
        var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

        var donCuaToi = _db.DatVes.Where(d => d.NhanVienId == nvId && nvId != null && d.TrangThai != TrangThaiDatVe.DaHuy);
        var veCuaToi = _db.Ves.Where(v => v.DatVe!.NhanVienId == nvId && nvId != null && v.TrangThai != TrangThaiVe.DaHuy);
        var veToiSoat = _db.Ves.Where(v => v.NhanVienSoatVeId == nvId && nvId != null && v.ThoiGianSoatVe != null);

        ViewBag.DonHomNay = await donCuaToi.CountAsync(d => d.NgayDat >= homNay);
        ViewBag.VeHomNay = await veCuaToi.CountAsync(v => v.DatVe!.NgayDat >= homNay);
        ViewBag.DonThangNay = await donCuaToi.CountAsync(d => d.NgayDat >= dauThang);
        ViewBag.VeThangNay = await veCuaToi.CountAsync(v => v.DatVe!.NgayDat >= dauThang);
        ViewBag.SoatHomNay = await veToiSoat.CountAsync(v => v.ThoiGianSoatVe >= homNay);
        ViewBag.SoatThangNay = await veToiSoat.CountAsync(v => v.ThoiGianSoatVe >= dauThang);

        var now = DateTime.Now;
        var suat = _db.LichChieus.Include(l => l.Phim).Include(l => l.PhongChieu)
            .Where(l => l.TrangThai == TrangThaiLichChieu.MoBan && l.ThoiGianBatDau >= now && l.ThoiGianBatDau < homNay.AddDays(1));
        if (rapChieuId.HasValue) suat = suat.Where(l => l.PhongChieu!.RapChieuId == rapChieuId.Value);
        ViewBag.SuatHomNay = await suat.OrderBy(l => l.ThoiGianBatDau).Take(12).ToListAsync();

        ViewBag.DonGanDay = await _db.DatVes.Include(d => d.Ves)
            .Where(d => d.NhanVienId == nvId && nvId != null)
            .OrderByDescending(d => d.NgayDat).Take(8).ToListAsync();

        ViewBag.PageTitle = "Công việc của tôi";
        return View("NhanVien");
    }
}
