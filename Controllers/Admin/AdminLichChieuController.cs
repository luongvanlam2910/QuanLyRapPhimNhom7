using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "QuanLyOnly")]
[Route("Admin/LichChieu")]
public class AdminLichChieuController : Controller
{
    private readonly ILichChieuService _lichChieuService;
    private readonly IPhimService _phimService;
    private readonly IRapChieuService _rapChieuService;
    private readonly ICurrentStaffService _currentStaffService;
    private readonly ApplicationDbContext _db;

    public AdminLichChieuController(
        ILichChieuService lichChieuService,
        IPhimService phimService,
        IRapChieuService rapChieuService,
        ICurrentStaffService currentStaffService,
        ApplicationDbContext db)
    {
        _lichChieuService = lichChieuService;
        _phimService = phimService;
        _rapChieuService = rapChieuService;
        _currentStaffService = currentStaffService;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var danhSach = await _lichChieuService.FindAllAsync();
        if (rapChieuId.HasValue)
        {
            danhSach = danhSach.Where(l => l.PhongChieu?.RapChieuId == rapChieuId.Value).ToList();
        }
        ViewBag.DanhSachLichChieu = danhSach;
        ViewBag.PageTitle = "Quản lý lịch chiếu";
        return View();
    }

    [HttpGet("Them")]
    public async Task<IActionResult> Them()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        ViewBag.DanhSachPhim = await _phimService.FindAllAsync();
        ViewBag.DanhSachRap = rapChieuId.HasValue
            ? (await _rapChieuService.FindAllAsync()).Where(r => r.Id == rapChieuId.Value).ToList()
            : await _rapChieuService.FindAllAsync();
        ViewBag.IsEdit = false;
        ViewBag.PageTitle = "Thêm lịch chiếu";
        return View();
    }

    [HttpPost("Them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(int maPhim, int maPhong, DateTime thoiGianBatDau, DateTime thoiGianKetThuc, decimal giaVe, string trangThai = "Mở bán")
    {
        try
        {
            var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuId.HasValue)
            {
                var phong = await _db.PhongChieus.FindAsync(maPhong)
                    ?? throw new ResourceNotFoundException("Phòng chiếu", "Id", maPhong);
                if (phong.RapChieuId != rapChieuId.Value)
                {
                    throw new BusinessException("Bạn chỉ có thể xếp lịch chiếu cho phòng của rạp mình phụ trách.");
                }
            }

            var lichChieu = new LichChieu
            {
                PhimId = maPhim,
                PhongChieuId = maPhong,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                GiaVe = giaVe,
                TrangThai = trangThai
            };
            await _lichChieuService.SaveAsync(lichChieu);
            TempData["SuccessMessage"] = "Thêm lịch chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task KiemTraThuocRapCuaMinhAsync(int maLich)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        if (!rapChieuId.HasValue) return; // Admin: không giới hạn

        var lc = await _lichChieuService.FindByIdAsync(maLich);
        if (lc?.PhongChieu?.RapChieuId != rapChieuId.Value)
        {
            throw new BusinessException("Bạn không có quyền thao tác trên lịch chiếu của rạp khác.");
        }
    }

    [HttpPost("Xoa/{maLich:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int maLich)
    {
        try
        {
            await KiemTraThuocRapCuaMinhAsync(maLich);
            await _lichChieuService.DeleteAsync(maLich);
            TempData["SuccessMessage"] = "Xóa lịch chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Sua")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int maLich, DateTime thoiGianBatDau, DateTime thoiGianKetThuc, decimal giaVe)
    {
        try
        {
            await KiemTraThuocRapCuaMinhAsync(maLich);
            await _lichChieuService.SuaAsync(maLich, thoiGianBatDau, thoiGianKetThuc, giaVe);
            TempData["SuccessMessage"] = "Cập nhật lịch chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("TrangThai/{maLich:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatTrangThai(int maLich, string trangThai)
    {
        try
        {
            await KiemTraThuocRapCuaMinhAsync(maLich);
            await _lichChieuService.UpdateTrangThaiAsync(maLich, trangThai);
            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }
}
