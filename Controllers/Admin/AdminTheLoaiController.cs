using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "QuanLyOnly")]
[Route("Admin/TheLoai")]
public class AdminTheLoaiController : Controller
{
    private readonly IPhimService _phimService;

    public AdminTheLoaiController(IPhimService phimService)
    {
        _phimService = phimService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync();
        ViewBag.PageTitle = "Quản lý thể loại";
        return View();
    }

    [HttpPost("Them")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(string tenTheLoai)
    {
        try
        {
            await _phimService.SaveTheLoaiAsync(new TheLoai { TenTheLoai = tenTheLoai });
            TempData["SuccessMessage"] = "Thêm thể loại thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Sua")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, string tenTheLoai)
    {
        try
        {
            var tl = await _phimService.FindTheLoaiByIdAsync(id)
                ?? throw new BusinessException("Không tìm thấy thể loại.");
            tl.TenTheLoai = tenTheLoai;
            await _phimService.SaveTheLoaiAsync(tl);
            TempData["SuccessMessage"] = "Cập nhật thể loại thành công.";
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
            await _phimService.DeleteTheLoaiAsync(id);
            TempData["SuccessMessage"] = "Xóa thể loại thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }
        return RedirectToAction(nameof(Index));
    }
}
