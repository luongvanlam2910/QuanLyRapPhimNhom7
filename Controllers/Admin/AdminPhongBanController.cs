using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Admin;

// Chỉ Admin toàn hệ thống mới được định nghĩa cơ cấu phòng ban.
[Authorize(Policy = "AdminOnly")]
[Route("Admin/PhongBan")]
public class AdminPhongBanController : Controller
{
    private readonly IPhongBanService _phongBanService;

    public AdminPhongBanController(IPhongBanService phongBanService)
    {
        _phongBanService = phongBanService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.DanhSachPhongBan = await _phongBanService.FindAllAsync();
        ViewBag.PageTitle = "Quản lý phòng ban";
        return View();
    }

    [HttpPost("Them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(string tenPb, string? moTa)
    {
        try
        {
            await _phongBanService.SaveAsync(new PhongBan { TenPb = tenPb, MoTa = moTa });
            TempData["SuccessMessage"] = "Thêm phòng ban thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Sua")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, string tenPb, string? moTa)
    {
        try
        {
            var pb = await _phongBanService.FindByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy phòng ban.");
            pb.TenPb = tenPb;
            pb.MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
            await _phongBanService.SaveAsync(pb);
            TempData["SuccessMessage"] = "Cập nhật phòng ban thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            await _phongBanService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa phòng ban thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }
        return RedirectToAction(nameof(Index));
    }
}
