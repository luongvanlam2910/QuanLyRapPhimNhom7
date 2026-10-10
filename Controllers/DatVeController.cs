using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers;

[Authorize]
[Route("dat-ve")]
public class DatVeController : Controller
{
    private readonly IDatVeService _datVeService;
    private readonly ILichChieuService _lichChieuService;
    private readonly IGheService _gheService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DatVeController(
        IDatVeService datVeService,
        ILichChieuService lichChieuService,
        IGheService gheService,
        IKhuyenMaiService khuyenMaiService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _datVeService = datVeService;
        _lichChieuService = lichChieuService;
        _gheService = gheService;
        _khuyenMaiService = khuyenMaiService;
        _userManager = userManager;
        _db = db;
    }

    private async Task<int?> GetCurrentKhachHangIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return null;
        var kh = await _db.KhachHangs.FirstOrDefaultAsync(k => k.UserId == userId);
        return kh?.Id;
    }

    private string GetCurrentUserKey() => _userManager.GetUserId(User) ?? HttpContext.Connection.Id;

    private async Task<DatVe?> LoadDatVeFullAsync(string maDat)
    {
        return await _db.DatVes
            .Include(d => d.Ves).ThenInclude(v => v.Ghe)
            .Include(d => d.Ves).ThenInclude(v => v.LichChieu!).ThenInclude(lc => lc.Phim)
            .Include(d => d.Ves).ThenInclude(v => v.LichChieu!).ThenInclude(lc => lc.PhongChieu!).ThenInclude(pc => pc.RapChieu)
            .Include(d => d.ThanhToans)
            .Include(d => d.KhachHang)
            .FirstOrDefaultAsync(d => d.MaDat == maDat);
    }

    [HttpGet("chon-ghe")]
    public IActionResult ChonGheFallback()
    {
        return RedirectToAction("TimKiem", "Home");
    }

    [HttpGet("chon-ghe/{maLich:int}")]
    public async Task<IActionResult> ChonGhe(int maLich)
    {
        var lichChieu = await _lichChieuService.FindByIdAsync(maLich);
        if (lichChieu == null)
        {
            return NotFound();
        }

        var userKey = GetCurrentUserKey();
        var seats = await _gheService.GetTrangThaiGheForLichAsync(maLich, userKey);
        var khuyenMais = await _khuyenMaiService.FindHieuLucAsync();

        var vm = new ChonGheViewModel
        {
            LichChieu = lichChieu,
            DanhSachGhe = seats,
            KhuyenMais = khuyenMais,
            SeatHoldMinutes = 10,
        };
        ViewData["Title"] = "Chọn ghế - " + (lichChieu.Phim?.TenPhim ?? "");
        return View(vm);
    }

    [HttpPost("giu-ghe")]
    public async Task<IActionResult> GiuGhe([FromForm] int maLich, [FromForm] List<int> maGhes)
    {
        try
        {
            await _gheService.GiuGheTamAsync(maLich, maGhes, GetCurrentUserKey());
            return Json(new { success = true, message = "Ghế đã được giữ tạm thời" });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = DatVeXemPhim.Infrastructure.ErrorMessages.From(e) });
        }
    }

    /// <summary>Nhả ghế đang giữ tạm (bỏ chọn ghế / rời trang). Không truyền maGhes = nhả tất cả ghế của người này ở suất đó.</summary>
    [HttpPost("nha-ghe")]
    public async Task<IActionResult> NhaGhe([FromForm] int maLich, [FromForm] List<int>? maGhes)
    {
        var userKey = GetCurrentUserKey();
        if (maGhes == null || maGhes.Count == 0)
        {
            await _gheService.GiaiPhongGiuTamAsync(maLich, userKey);
        }
        else
        {
            await _gheService.GiaiPhongGheAsync(maLich, maGhes, userKey);
        }
        return Json(new { success = true });
    }

    /// <summary>Trạng thái ghế hiện tại của suất chiếu (client gọi định kỳ để thấy ghế người khác vừa giữ/đặt).</summary>
    [HttpGet("trang-thai-ghe/{maLich:int}")]
    public async Task<IActionResult> TrangThaiGhe(int maLich)
    {
        var seats = await _gheService.GetTrangThaiGheForLichAsync(maLich, GetCurrentUserKey());
        Response.Headers.CacheControl = "no-store";
        return Json(seats.Select(s => new { gheId = s.GheId, trangThai = s.TrangThai }));
    }

    [HttpGet("xac-nhan")]
    public async Task<IActionResult> XacNhan(int maLich, List<int> maGhes, int? maKm)
    {
        var lichChieu = await _lichChieuService.GetByIdAsync(maLich);

        if (maGhes == null || maGhes.Count == 0)
        {
            TempData["errorMessage"] = "Vui lòng chọn ít nhất 1 ghế.";
            return RedirectToAction(nameof(ChonGhe), new { maLich });
        }

        // Kiểm tra lại ghế trước khi cho sang bước thanh toán (suất còn mở bán, ghế chưa bị đặt / người khác giữ)
        // và gia hạn thời gian giữ ghế cho khách đang xác nhận.
        try
        {
            await _gheService.GiuGheTamAsync(maLich, maGhes, GetCurrentUserKey());
        }
        catch (Exception e)
        {
            TempData["errorMessage"] = DatVeXemPhim.Infrastructure.ErrorMessages.From(e);
            return RedirectToAction(nameof(ChonGhe), new { maLich });
        }

        var gheDaChon = new List<GheChonDto>();
        foreach (var gheId in maGhes)
        {
            var ghe = await _gheService.FindByIdAsync(gheId);
            gheDaChon.Add(new GheChonDto
            {
                GheId = ghe.Id,
                HangGhe = ghe.HangGhe,
                SoGhe = ghe.SoGhe,
                LoaiGhe = ghe.LoaiGhe ?? "Thường",
            });
        }

        var khuyenMais = await _khuyenMaiService.FindHieuLucAsync();

        var vm = new XacNhanViewModel
        {
            LichChieu = lichChieu,
            GheDaChon = gheDaChon,
            MaGhes = maGhes,
            KhuyenMais = khuyenMais,
            MaKmChon = maKm,
        };
        ViewData["Title"] = "Xác nhận đặt vé - UTC Cinema";
        return View(vm);
    }

    [HttpPost("thanh-toan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThanhToan(int maLich, List<int> maGhes, int? maKm, string? phuongThucThanhToan)
    {
        var request = new DatVeRequest
        {
            LichChieuId = maLich,
            GheIds = maGhes,
            KhuyenMaiId = maKm,
            PhuongThucThanhToan = phuongThucThanhToan,
        };

        try
        {
            var khachHangId = await GetCurrentKhachHangIdAsync();
            // Đặt online luôn qua cổng thanh toán mô phỏng (xacNhanThanhToanNgay: false):
            // đơn giữ ở "Chờ thanh toán" kèm hạn chót, chưa trừ tiền/xác nhận ngay như trước đây.
            var response = await _datVeService.DatVeAsync(request, khachHangId, null, GetCurrentUserKey(), xacNhanThanhToanNgay: false);
            return RedirectToAction(nameof(ThanhToanGateway), new { maDat = response.MaDat });
        }
        catch (Exception e)
        {
            TempData["errorMessage"] = DatVeXemPhim.Infrastructure.ErrorMessages.From(e);
            return RedirectToAction(nameof(ChonGhe), new { maLich });
        }
    }

    [HttpGet("thanh-toan/{maDat}")]
    public async Task<IActionResult> ThanhToanGateway(string maDat)
    {
        var datVe = await LoadDatVeFullAsync(maDat);
        if (datVe == null)
        {
            return NotFound();
        }

        var khachHangIdHienTai = await GetCurrentKhachHangIdAsync();
        if (!khachHangIdHienTai.HasValue || datVe.KhachHangId != khachHangIdHienTai.Value)
        {
            return RedirectToAction("Forbidden", "Home");
        }

        if (datVe.TrangThai != "Chờ thanh toán")
        {
            // Đơn đã được xử lý (thanh toán xong / đã hủy) — chuyển thẳng sang trang kết quả.
            return RedirectToAction(nameof(KetQua), new { maDat });
        }

        ViewData["Title"] = "Thanh toán - UTC Cinema";
        return View(datVe);
    }

    [HttpPost("thanh-toan/{maDat}/xac-nhan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanThanhToanGateway(string maDat, bool thanhCong, string? phuongThucThanhToan)
    {
        try
        {
            var datVe = await _db.DatVes.FirstOrDefaultAsync(d => d.MaDat == maDat)
                ?? throw new Exception("Không tìm thấy đơn đặt vé");
            var khachHangIdHienTai = await GetCurrentKhachHangIdAsync();
            if (!khachHangIdHienTai.HasValue || datVe.KhachHangId != khachHangIdHienTai.Value)
            {
                return RedirectToAction("Forbidden", "Home");
            }

            // Trang cổng thanh toán mô phỏng: khách bấm "Thanh toán thành công" hoặc "Hủy giao dịch"
            // để tự chọn kết quả, thay cho tích hợp cổng thanh toán thật.
            await _datVeService.XacNhanThanhToanAsync(maDat, thanhCong, phuongThucThanhToan);
        }
        catch (Exception e)
        {
            TempData["errorMessage"] = DatVeXemPhim.Infrastructure.ErrorMessages.From(e);
        }
        return RedirectToAction(nameof(KetQua), new { maDat });
    }

    /// <summary>
    /// Nhân viên chỉ xem được đơn do chính mình bán; Quản lý xem đơn thuộc rạp mình phụ trách; Admin xem tất cả.
    /// (Trước đây mọi nhân viên xem được mọi đơn của mọi rạp, lộ thông tin khách hàng.)
    /// </summary>
    private async Task<bool> NhanVienDuocXemDonAsync(DatVe datVe)
    {
        if (User.IsInRole("Admin")) return true;
        if (!User.IsInRole("QuanLy") && !User.IsInRole("NhanVien")) return false;

        var userId = _userManager.GetUserId(User);
        var nv = await _db.NhanViens.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId);
        if (nv == null) return false;

        if (User.IsInRole("QuanLy"))
        {
            return datVe.Ves.Any(v => v.LichChieu?.PhongChieu?.RapChieuId == nv.RapChieuId);
        }
        return datVe.NhanVienId.HasValue && datVe.NhanVienId.Value == nv.Id;
    }

    [HttpGet("ket-qua/{maDat}")]
    public async Task<IActionResult> KetQua(string maDat)
    {
        var datVe = await LoadDatVeFullAsync(maDat);
        if (datVe == null)
        {
            return NotFound();
        }

        var isStaff = await NhanVienDuocXemDonAsync(datVe);
        var khachHangId = await GetCurrentKhachHangIdAsync();
        var isOwner = khachHangId.HasValue && datVe.KhachHangId == khachHangId.Value;
        if (!isOwner && !isStaff)
        {
            return RedirectToAction("Forbidden", "Home");
        }

        ViewData["Title"] = "Đặt vé thành công - UTC Cinema";
        return View(datVe);
    }

    [HttpGet("lich-su")]
    public async Task<IActionResult> LichSu(string? trangThai)
    {
        var khachHangId = await GetCurrentKhachHangIdAsync();
        var danhSach = khachHangId.HasValue
            ? await _datVeService.FindByKhachHangAsync(khachHangId.Value)
            : new List<DatVe>();

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            danhSach = danhSach.Where(dv => string.Equals(dv.TrangThai, trangThai, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var vm = new LichSuViewModel
        {
            DanhSachDatVe = danhSach,
            TrangThaiFilter = trangThai,
        };
        ViewData["Title"] = "Lịch sử đặt vé - UTC Cinema";
        return View(vm);
    }

    [HttpPost("huy/{maDat}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(string maDat)
    {
        try
        {
            var khachHangId = await GetCurrentKhachHangIdAsync();
            var datVe = await _db.DatVes.FirstOrDefaultAsync(d => d.MaDat == maDat)
                        ?? throw new Exception("Không tìm thấy đơn đặt vé");
            await _datVeService.HuyDatVeAsync(datVe.Id, khachHangId ?? 0);
            TempData["successMessage"] = "Đã hủy đặt vé thành công";
        }
        catch (Exception e)
        {
            TempData["errorMessage"] = DatVeXemPhim.Infrastructure.ErrorMessages.From(e);
        }
        return RedirectToAction(nameof(LichSu));
    }
}
