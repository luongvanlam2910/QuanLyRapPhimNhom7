using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers.Admin;

// Mọi nhân viên (bán vé/quản lý/admin) đều được soát vé tại cửa rạp.
[Authorize(Policy = "NhanVienHoacQuanLy")]
[Route("Admin/SoatVe")]
public class AdminSoatVeController : Controller
{
    private readonly ISoatVeService _soatVeService;
    private readonly ICurrentStaffService _currentStaffService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AdminSoatVeController(
        ISoatVeService soatVeService,
        ICurrentStaffService currentStaffService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _soatVeService = soatVeService;
        _currentStaffService = currentStaffService;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Soát vé";
        return View();
    }

    [HttpPost("TraCuu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TraCuu([FromForm] string maVe)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var ketQua = await _soatVeService.TraCuuAsync(maVe ?? "", rapChieuId);
        return Json(ketQua);
    }

    [HttpPost("XacNhan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan([FromForm] string maVe)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();

        var userId = _userManager.GetUserId(User);
        var nhanVien = await _db.NhanViens.FirstOrDefaultAsync(n => n.UserId == userId);
        if (nhanVien == null)
        {
            return Json(new { hopLe = false, thongBao = "Tài khoản của bạn chưa được gắn với hồ sơ nhân viên nên không thể soát vé." });
        }

        var ketQua = await _soatVeService.XacNhanSoatAsync(maVe ?? "", nhanVien.Id, rapChieuId);
        return Json(ketQua);
    }
}
