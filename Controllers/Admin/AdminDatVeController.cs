using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "NhanVienHoacQuanLy")]
[Route("Admin/DatVe")]
public class AdminDatVeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IDatVeService _datVeService;
    private readonly ICurrentStaffService _currentStaffService;

    public AdminDatVeController(ApplicationDbContext db, IDatVeService datVeService, ICurrentStaffService currentStaffService)
    {
        _db = db;
        _datVeService = datVeService;
        _currentStaffService = currentStaffService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, string? trangThai)
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();

        var query = _db.DatVes
            .Include(d => d.KhachHang)
            .Include(d => d.Ves).ThenInclude(v => v.Ghe)
            .Include(d => d.Ves).ThenInclude(v => v.LichChieu).ThenInclude(lc => lc!.Phim)
            .Include(d => d.Ves).ThenInclude(v => v.LichChieu).ThenInclude(lc => lc!.PhongChieu)
            .AsQueryable();

        var laQuanLy = _currentStaffService.LaQuanLyTroLen;
        var nhanVienId = laQuanLy ? null : await _currentStaffService.GetNhanVienIdAsync();

        if (!laQuanLy)
        {
            // Nhân viên thường: chỉ thấy các đơn do CHÍNH MÌNH bán tại quầy.
            query = query.Where(d => d.NhanVienId == nhanVienId && nhanVienId != null);
        }
        else if (rapChieuId.HasValue)
        {
            // QuanLy chỉ thấy đơn đặt vé cho suất chiếu tại rạp mình phụ trách.
            query = query.Where(d => d.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value));
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(d => d.TrangThai == trangThai);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(d =>
                (d.MaDat != null && d.MaDat.ToLower().Contains(kw)) ||
                (d.KhachHang != null && d.KhachHang.HoTen.ToLower().Contains(kw)) ||
                (d.KhachHang != null && d.KhachHang.Email != null && d.KhachHang.Email.ToLower().Contains(kw)));
        }

        var danhSachDatVe = await query
            .OrderByDescending(d => d.NgayDat)
            .ToListAsync();

        // Cùng điều kiện phạm vi rạp với query chi tiết ở trên, để thống kê khớp danh sách QuanLy đang xem.
        var statsQuery = _db.DatVes.Include(d => d.Ves).ThenInclude(v => v.LichChieu).ThenInclude(lc => lc!.PhongChieu).AsQueryable();
        var tatCaDonTrongPhamVi = rapChieuId.HasValue
            ? await statsQuery.Where(d => d.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value)).ToListAsync()
            : null;

        ViewBag.DanhSachDatVe = danhSachDatVe;
        ViewBag.Keyword = keyword;
        ViewBag.TrangThaiFilter = trangThai;
        ViewBag.LaQuanLy = laQuanLy;
        if (!laQuanLy)
        {
            // Nhân viên thường: chỉ đếm số đơn của mình, KHÔNG hiển thị tiền/doanh thu.
            var donCuaToi = await _db.DatVes.Where(d => d.NhanVienId == nhanVienId && nhanVienId != null).Select(d => d.TrangThai).ToListAsync();
            ViewBag.SoThanhToan = donCuaToi.Count(t => t == TrangThaiDatVe.DaThanhToan);
            ViewBag.SoChoPhanHoi = donCuaToi.Count(t => t == TrangThaiDatVe.ChoThanhToan);
            ViewBag.SoHuy = donCuaToi.Count(t => t == TrangThaiDatVe.DaHuy);
            ViewBag.TongDoanhThu = 0m;
        }
        else if (tatCaDonTrongPhamVi != null)
        {
            ViewBag.SoThanhToan = tatCaDonTrongPhamVi.Count(d => d.TrangThai == TrangThaiDatVe.DaThanhToan);
            ViewBag.SoChoPhanHoi = tatCaDonTrongPhamVi.Count(d => d.TrangThai == TrangThaiDatVe.ChoThanhToan);
            ViewBag.SoHuy = tatCaDonTrongPhamVi.Count(d => d.TrangThai == TrangThaiDatVe.DaHuy);
            ViewBag.TongDoanhThu = tatCaDonTrongPhamVi.Where(d => d.TrangThai == TrangThaiDatVe.DaThanhToan).Sum(d => d.TongTien);
        }
        else
        {
            ViewBag.SoThanhToan = await _db.DatVes.CountAsync(d => d.TrangThai == TrangThaiDatVe.DaThanhToan);
            ViewBag.SoChoPhanHoi = await _db.DatVes.CountAsync(d => d.TrangThai == TrangThaiDatVe.ChoThanhToan);
            ViewBag.SoHuy = await _db.DatVes.CountAsync(d => d.TrangThai == TrangThaiDatVe.DaHuy);
            ViewBag.TongDoanhThu = await _db.DatVes.Where(d => d.TrangThai == TrangThaiDatVe.DaThanhToan).SumAsync(d => (decimal?)d.TongTien) ?? 0m;
        }
        ViewBag.PageTitle = laQuanLy ? "Quản lý đặt vé" : "Vé tôi đã bán";
        return View();
    }

    // Đổi trạng thái đơn (hủy/hoàn tiền/xác nhận thanh toán) chỉ dành cho Quản lý trở lên.
    [HttpPost("TrangThai/{maDat:int}")]
    [Authorize(Policy = "QuanLyOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatTrangThai(int maDat, string trangThai)
    {
        try
        {
            var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuId.HasValue)
            {
                var thuocRap = await _db.DatVes
                    .Where(d => d.Id == maDat)
                    .AnyAsync(d => d.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value));
                if (!thuocRap)
                {
                    throw new BusinessException("Bạn không có quyền thao tác trên đơn đặt vé của rạp khác.");
                }
            }

            // Mọi thay đổi trạng thái đi qua service để kiểm tra bước chuyển hợp lệ và đồng bộ Vé/Thanh toán.
            await _datVeService.DoiTrangThaiByStaffAsync(maDat, trangThai);

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(Index));
    }
}
