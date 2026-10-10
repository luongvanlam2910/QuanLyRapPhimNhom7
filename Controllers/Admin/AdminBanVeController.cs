using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers.Admin;

// Bán vé tại quầy cho khách vãng lai hoặc khách có tài khoản, thu tiền ngay (không qua cổng thanh toán).
[Authorize(Policy = "NhanVienHoacQuanLy")]
[Route("Admin/BanVe")]
public class AdminBanVeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IGheService _gheService;
    private readonly IDatVeService _datVeService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly ICurrentStaffService _currentStaffService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminBanVeController(
        ApplicationDbContext db,
        IGheService gheService,
        IDatVeService datVeService,
        IKhuyenMaiService khuyenMaiService,
        ICurrentStaffService currentStaffService,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _gheService = gheService;
        _datVeService = datVeService;
        _khuyenMaiService = khuyenMaiService;
        _currentStaffService = currentStaffService;
        _userManager = userManager;
    }

    private async Task<DatVeXemPhim.Models.Entities.NhanVien?> GetCurrentNhanVienAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _db.NhanViens.FirstOrDefaultAsync(n => n.UserId == userId);
    }

    /// <summary>Khóa giữ ghế riêng cho từng nhân viên quầy (không dùng chung, không trùng khóa của khách online).</summary>
    private string PosKey() => "POS:" + _userManager.GetUserId(User);

    /// <summary>Nhân viên chỉ thao tác trên suất chiếu thuộc rạp mình phụ trách.</summary>
    private async Task<bool> ThuocRapCuaToiAsync(int maLich)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        if (!rapChieuId.HasValue) return true;
        return await _db.LichChieus.AnyAsync(l => l.Id == maLich && l.PhongChieu!.RapChieuId == rapChieuId.Value);
    }

    /// <summary>Giữ tạm ghế ngay khi nhân viên chọn, để quầy khác và khách online thấy ghế đã bị khóa.</summary>
    [HttpPost("giu-ghe")]
    public async Task<IActionResult> GiuGhe([FromForm] int maLich, [FromForm] List<int> maGhes)
    {
        try
        {
            if (!await ThuocRapCuaToiAsync(maLich))
            {
                return Json(new { success = false, message = "Suất chiếu không thuộc rạp bạn phụ trách." });
            }
            await _gheService.GiuGheTamAsync(maLich, maGhes, PosKey());
            return Json(new { success = true });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = DatVeXemPhim.Infrastructure.ErrorMessages.From(e) });
        }
    }

    [HttpPost("nha-ghe")]
    public async Task<IActionResult> NhaGhe([FromForm] int maLich, [FromForm] List<int>? maGhes)
    {
        if (maGhes == null || maGhes.Count == 0)
        {
            await _gheService.GiaiPhongGiuTamAsync(maLich, PosKey());
        }
        else
        {
            await _gheService.GiaiPhongGheAsync(maLich, maGhes, PosKey());
        }
        return Json(new { success = true });
    }

    [HttpGet("trang-thai-ghe/{maLich:int}")]
    public async Task<IActionResult> TrangThaiGhe(int maLich)
    {
        if (!await ThuocRapCuaToiAsync(maLich)) return Forbid();
        var seats = await _gheService.GetTrangThaiGheForLichAsync(maLich, PosKey());
        Response.Headers.CacheControl = "no-store";
        return Json(seats.Select(s => new { gheId = s.GheId, trangThai = s.TrangThai }));
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? maLich)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var now = DateTime.Now;
        var endOfDay = now.Date.AddDays(1);

        var query = _db.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
            .Where(l => l.TrangThai == TrangThaiLichChieu.MoBan && l.ThoiGianBatDau >= now && l.ThoiGianBatDau < endOfDay)
            .AsQueryable();

        if (rapChieuId.HasValue)
        {
            query = query.Where(l => l.PhongChieu!.RapChieuId == rapChieuId.Value);
        }

        ViewBag.DanhSachSuatHomNay = await query.OrderBy(l => l.ThoiGianBatDau).ToListAsync();

        if (maLich.HasValue)
        {
            var lichChieu = await _db.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
                .FirstOrDefaultAsync(l => l.Id == maLich.Value);

            if (lichChieu != null && (!rapChieuId.HasValue || lichChieu.PhongChieu!.RapChieuId == rapChieuId.Value))
            {
                ViewBag.LichChieuChon = lichChieu;
                ViewBag.DanhSachGhe = await _gheService.GetTrangThaiGheForLichAsync(maLich.Value, PosKey());
                ViewBag.KhuyenMais = await _khuyenMaiService.FindHieuLucAsync();
            }
        }

        ViewBag.PageTitle = "Bán vé tại quầy";
        return View();
    }

    [HttpPost("ban")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(int maLich, List<int> maGhes, int? maKm, string? tenKhach, string? sdtKhach, string phuongThucThanhToan = "Tiền mặt")
    {
        try
        {
            if (!await ThuocRapCuaToiAsync(maLich))
            {
                throw new Exception("Suất chiếu không thuộc rạp bạn phụ trách.");
            }

            var nhanVien = await GetCurrentNhanVienAsync()
                ?? throw new Exception("Tài khoản của bạn chưa được gắn với hồ sơ nhân viên nên không thể bán vé.");

            var request = new DatVeRequest
            {
                LichChieuId = maLich,
                GheIds = maGhes,
                KhuyenMaiId = maKm,
                PhuongThucThanhToan = phuongThucThanhToan,
                TenKhachVangLai = string.IsNullOrWhiteSpace(tenKhach) ? "Khách vãng lai" : tenKhach,
                SdtVangLai = sdtKhach,
            };

            // xacNhanThanhToanNgay mặc định true: bán tại quầy thu tiền mặt/quẹt thẻ ngay lúc đó.
            var response = await _datVeService.DatVeAsync(request, khachHangId: null, nhanVienId: nhanVien.Id, userKey: PosKey());
            TempData["SuccessMessage"] = $"Bán vé thành công. Mã đặt: {response.MaDat}";
            return RedirectToAction("KetQua", "DatVe", new { maDat = response.MaDat });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
            return RedirectToAction(nameof(Index), new { maLich });
        }
    }
}
