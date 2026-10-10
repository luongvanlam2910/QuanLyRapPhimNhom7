using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "QuanLyOnly")]
[Route("Admin/Phim")]
public class AdminPhimController : Controller
{
    private readonly IPhimService _phimService;

    public AdminPhimController(IPhimService phimService)
    {
        _phimService = phimService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.DanhSachPhim = await _phimService.FindAllAsync();
        ViewBag.PageTitle = "Quản lý phim";
        return View();
    }

    [HttpGet("Them")]
    public async Task<IActionResult> Them()
    {
        ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
        ViewBag.IsEdit = false;
        ViewBag.PageTitle = "Thêm phim";
        return View(new PhimFormDTO());
    }

    [HttpPost("Them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(PhimFormDTO phimForm, IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Thêm phim";
            return View(phimForm);
        }

        try
        {
            var phim = BuildPhimFromDto(phimForm);
            var theLoaiIds = phimForm.MaTheLoais.Select(int.Parse).ToList();
            await _phimService.SaveAsync(phim, theLoaiIds, posterFile);
            TempData["SuccessMessage"] = "Thêm phim thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
            ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Thêm phim";
            return View(phimForm);
        }
    }

    [HttpGet("Sua/{maPhim:int}")]
    public async Task<IActionResult> Sua(int maPhim)
    {
        var phim = await _phimService.FindByIdAsync(maPhim);
        if (phim == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy phim.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new PhimFormDTO
        {
            MaPhim = phim.Id,
            TenPhim = phim.TenPhim,
            ThoiLuong = phim.ThoiLuong,
            DaoDien = phim.DaoDien,
            DoTuoi = phim.DoTuoi,
            NgayKhoiChieu = phim.NgayKhoiChieu?.ToString("yyyy-MM-dd"),
            TrangThai = MapTrangThaiToCode(phim.TrangThai),
            AnhPoster = phim.AnhPoster,
            MoTa = phim.MoTa,
            MaTheLoais = phim.TheLoais.Select(t => t.Id.ToString()).ToList()
        };

        ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
        ViewBag.IsEdit = true;
        ViewBag.PageTitle = "Sửa phim";
        return View("Them", dto);
    }

    [HttpPost("Sua/{maPhim:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int maPhim, PhimFormDTO phimForm, IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
            ViewBag.IsEdit = true;
            ViewBag.PageTitle = "Sửa phim";
            return View("Them", phimForm);
        }

        try
        {
            var phim = await _phimService.GetByIdAsync(maPhim);
            var updated = BuildPhimFromDto(phimForm);
            updated.Id = maPhim;
            var theLoaiIds = phimForm.MaTheLoais.Select(int.Parse).ToList();
            await _phimService.SaveAsync(updated, theLoaiIds, posterFile);
            TempData["SuccessMessage"] = "Cập nhật phim thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
            ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
            ViewBag.IsEdit = true;
            ViewBag.PageTitle = "Sửa phim";
            return View("Them", phimForm);
        }
    }

    [HttpPost("Xoa/{maPhim:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int maPhim)
    {
        try
        {
            await _phimService.DeleteAsync(maPhim);
            TempData["SuccessMessage"] = "Xóa phim thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private static Phim BuildPhimFromDto(PhimFormDTO dto)
    {
        DateOnly? ngayKhoiChieu = null;
        if (!string.IsNullOrWhiteSpace(dto.NgayKhoiChieu) && DateOnly.TryParse(dto.NgayKhoiChieu, out var parsed))
        {
            ngayKhoiChieu = parsed;
        }

        // Chặn phân loại độ tuổi tùy ý ngoài 5 mức quy định (P/K/T13/T16/T18) — nếu không, kiểm tra
        // độ tuổi khi đặt vé (KiemTraDoTuoiService) sẽ bỏ qua nhãn lạ, coi như không giới hạn tuổi.
        if (!string.IsNullOrWhiteSpace(dto.DoTuoi) && !DoTuoiPhim.All.Contains(dto.DoTuoi))
        {
            throw new BusinessException($"Phân loại độ tuổi không hợp lệ. Chỉ chấp nhận: {string.Join(", ", DoTuoiPhim.All)}.");
        }

        return new Phim
        {
            TenPhim = dto.TenPhim,
            ThoiLuong = dto.ThoiLuong,
            DaoDien = dto.DaoDien,
            DoTuoi = string.IsNullOrWhiteSpace(dto.DoTuoi) ? null : dto.DoTuoi,
            NgayKhoiChieu = ngayKhoiChieu,
            TrangThai = MapTrangThai(dto.TrangThai),
            AnhPoster = dto.AnhPoster,
            MoTa = dto.MoTa
        };
    }

    private static string MapTrangThai(string? code)
    {
        return code switch
        {
            "dang-chieu" => "Đang chiếu",
            "sap-chieu" => "Sắp chiếu",
            "ngung-chieu" => "Ngừng chiếu",
            _ => "Đang chiếu"
        };
    }

    private static string MapTrangThaiToCode(string? trangThai)
    {
        if (string.IsNullOrWhiteSpace(trangThai)) return "dang-chieu";
        if (trangThai.Contains("ang", StringComparison.OrdinalIgnoreCase)) return "dang-chieu";
        if (trangThai.Contains("p chi", StringComparison.OrdinalIgnoreCase)) return "sap-chieu";
        return "ngung-chieu";
    }
}
