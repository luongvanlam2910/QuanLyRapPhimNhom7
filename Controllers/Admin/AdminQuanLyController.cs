using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using DatVeXemPhim.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "NhanVienHoacQuanLy")]
[Route("Admin")]
public class AdminQuanLyController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IRapChieuService _rapChieuService;
    private readonly IGheService _gheService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICurrentStaffService _currentStaffService;

    public AdminQuanLyController(
        ApplicationDbContext db,
        IRapChieuService rapChieuService,
        IGheService gheService,
        IKhuyenMaiService khuyenMaiService,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICurrentStaffService currentStaffService)
    {
        _db = db;
        _rapChieuService = rapChieuService;
        _gheService = gheService;
        _khuyenMaiService = khuyenMaiService;
        _userManager = userManager;
        _roleManager = roleManager;
        _currentStaffService = currentStaffService;
    }

    // ===================== KHACH HANG =====================

    [HttpGet("KhachHang")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> KhachHang()
    {
        ViewBag.DanhSachKH = await _db.KhachHangs.OrderBy(k => k.Id).ToListAsync();
        ViewBag.PageTitle = "Quản lý khách hàng";
        return View();
    }

    // Khóa/mở khóa thay vì xóa cứng: khách hàng đã có đơn đặt vé (FK từ DAT_VE) không thể xóa,
    // và xóa cứng cũng không vô hiệu hóa được tài khoản đăng nhập Identity liên kết (fix #11).
    [HttpPost("KhachHang/KhoaMoKhoa/{maKh:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> KhoaMoKhoaKhachHang(int maKh)
    {
        try
        {
            var kh = await _db.KhachHangs.FindAsync(maKh);
            if (kh == null) throw new BusinessException("Không tìm thấy khách hàng.");

            var daKhoa = kh.TrangThai == TrangThaiKhachHang.DaKhoa;
            kh.TrangThai = daKhoa ? TrangThaiKhachHang.HoatDong : TrangThaiKhachHang.DaKhoa;

            if (!string.IsNullOrEmpty(kh.UserId))
            {
                var user = await _userManager.FindByIdAsync(kh.UserId);
                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = daKhoa ? null : DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                    // Buộc đăng xuất phiên hiện tại trong tối đa 1 phút (SecurityStampValidatorOptions).
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = daKhoa ? "Đã mở khóa khách hàng." : "Đã khóa khách hàng.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(KhachHang));
    }

    // ===================== NHAN VIEN =====================

    [HttpGet("NhanVien")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> NhanVien()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var query = _db.NhanViens.Include(n => n.RapChieu).Include(n => n.PhongBan).AsQueryable();
        if (rapChieuId.HasValue)
        {
            // QuanLy chỉ thấy nhân viên thuộc rạp mình phụ trách; Admin thấy tất cả.
            query = query.Where(n => n.RapChieuId == rapChieuId.Value);
        }

        ViewBag.DanhSachNV = await query.OrderBy(n => n.Id).ToListAsync();
        ViewBag.DanhSachRap = rapChieuId.HasValue
            ? (await _rapChieuService.FindAllAsync()).Where(r => r.Id == rapChieuId.Value).ToList()
            : await _rapChieuService.FindAllAsync();
        ViewBag.DanhSachPhongBan = await _db.PhongBans.OrderBy(p => p.TenPb).ToListAsync();
        ViewBag.PageTitle = "Quản lý nhân viên";
        return View();
    }

    // ---------- Hàm dùng chung cho thêm/sửa nhân viên ----------

    /// <summary>Kiểm tra mật khẩu theo chính sách của Identity (độ dài, chữ hoa, số, ký tự đặc biệt) TRƯỚC khi thay đổi bất cứ dữ liệu nào.</summary>
    private async Task KiemTraMatKhauAsync(ApplicationUser user, string matKhau)
    {
        var loi = new List<string>();
        foreach (var validator in _userManager.PasswordValidators)
        {
            var kq = await validator.ValidateAsync(_userManager, user, matKhau);
            if (!kq.Succeeded) loi.AddRange(kq.Errors.Select(e => e.Description));
        }
        if (loi.Count > 0)
        {
            throw new BusinessException("Mật khẩu không hợp lệ: " + string.Join(" ", loi));
        }
    }

    /// <summary>Mọi thao tác Identity phải được kiểm tra kết quả (trước đây bị bỏ qua nên báo thành công dù thất bại).</summary>
    private static void PhaiThanhCong(IdentityResult kq)
    {
        if (!kq.Succeeded)
        {
            throw new BusinessException(string.Join(" ", kq.Errors.Select(e => e.Description)));
        }
    }

    private static void KiemTraEmail(string email)
    {
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            throw new BusinessException("Email không hợp lệ.");
        }
    }

    [HttpPost("NhanVien/Them")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> ThemNhanVien(string hoTen, string email, string sdt, string chucVu, int maRap, string matKhau, int? phongBanId, DateOnly? ngaySinh)
    {
        try
        {
            hoTen = (hoTen ?? "").Trim();
            email = (email ?? "").Trim();
            sdt = (sdt ?? "").Trim();

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) || maRap <= 0)
            {
                throw new BusinessException("Vui lòng điền đầy đủ thông tin bắt buộc.");
            }
            KiemTraEmail(email);

            // Whitelist role hợp lệ (fix #9): trước đây nhận "chucVu" trực tiếp từ form,
            // cho phép tạo ra role tùy ý (kể cả gán nhầm/giả mạo role không mong muốn).
            if (!AppRoles.IsValidNhanVienRole(chucVu))
            {
                throw new BusinessException("Chức vụ không hợp lệ.");
            }

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != maRap)
            {
                throw new BusinessException("Bạn chỉ có thể thêm nhân viên cho rạp mình phụ trách.");
            }

            var rap = await _rapChieuService.FindByIdAsync(maRap);
            if (rap == null) throw new BusinessException("Không tìm thấy rạp chiếu.");

            if (await _userManager.FindByEmailAsync(email) != null || await _db.NhanViens.AnyAsync(n => n.Email == email))
            {
                throw new BusinessException($"Email '{email}' đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(sdt) && await _db.NhanViens.AnyAsync(n => n.Sdt == sdt))
            {
                throw new BusinessException($"Số điện thoại '{sdt}' đã thuộc về nhân viên khác.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                HoTen = hoTen,
                PhoneNumber = sdt,
                EmailConfirmed = true
            };

            // Kiểm tra mật khẩu trước để báo lỗi rõ ràng (độ dài, chữ hoa, số, ký tự đặc biệt).
            if (string.IsNullOrWhiteSpace(matKhau))
            {
                throw new BusinessException("Vui lòng nhập mật khẩu.");
            }
            await KiemTraMatKhauAsync(user, matKhau);

            if (!await _roleManager.RoleExistsAsync(chucVu))
            {
                await _roleManager.CreateAsync(new IdentityRole(chucVu));
            }

            // Tạo tài khoản + hồ sơ nhân viên trong 1 giao dịch: lỗi ở bước nào cũng không để lại tài khoản mồ côi.
            await using var tx = await _db.Database.BeginTransactionAsync();
            PhaiThanhCong(await _userManager.CreateAsync(user, matKhau));
            PhaiThanhCong(await _userManager.AddToRoleAsync(user, chucVu));

            var nv = new NhanVien
            {
                UserId = user.Id,
                RapChieuId = maRap,
                PhongBanId = phongBanId,
                NgaySinh = ngaySinh,
                HoTen = hoTen,
                ChucVu = chucVu,
                Sdt = sdt,
                Email = email,
                TrangThai = "Hoạt động"
            };

            _db.NhanViens.Add(nv);
            await _db.SaveChangesAsync();
            // Sinh mã từ Id sau khi lưu (fix #13): tránh đụng mã khi thêm nhân viên đồng thời.
            nv.MaNv = $"NV{nv.Id:000}";
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["SuccessMessage"] = $"Thêm nhân viên thành công. Mã: {nv.MaNv}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(NhanVien));
    }

    [HttpPost("NhanVien/Xoa/{maNv:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> XoaNhanVien(int maNv)
    {
        try
        {
            var nv = await _db.NhanViens.FindAsync(maNv);
            if (nv == null) throw new BusinessException("Không tìm thấy nhân viên.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != nv.RapChieuId)
            {
                throw new BusinessException("Bạn không có quyền thao tác trên nhân viên của rạp khác.");
            }

            if (!string.IsNullOrEmpty(nv.UserId) && nv.UserId == _userManager.GetUserId(User))
            {
                throw new BusinessException("Bạn không thể tự xóa tài khoản của chính mình.");
            }

            // Kiểm tra lịch sử TRƯỚC khi xóa bất cứ thứ gì (trước đây xóa tài khoản đăng nhập trước,
            // rồi mới lỗi khóa ngoại khi xóa hồ sơ → còn hồ sơ nhưng mất tài khoản).
            var soDon = await _db.DatVes.CountAsync(d => d.NhanVienId == nv.Id);
            var soVeSoat = await _db.Ves.CountAsync(v => v.NhanVienSoatVeId == nv.Id);
            if (soDon > 0 || soVeSoat > 0)
            {
                throw new BusinessException(
                    $"Không thể xóa nhân viên '{nv.HoTen}' vì đã có lịch sử công việc ({soDon} đơn bán vé, {soVeSoat} vé đã soát). " +
                    "Hãy dùng chức năng Khóa tài khoản để ngừng cho nhân viên này làm việc.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            _db.NhanViens.Remove(nv);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(nv.UserId))
            {
                var user = await _userManager.FindByIdAsync(nv.UserId);
                if (user != null) PhaiThanhCong(await _userManager.DeleteAsync(user));
            }
            await tx.CommitAsync();
            TempData["SuccessMessage"] = "Xóa nhân viên thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(NhanVien));
    }

    [HttpPost("NhanVien/Sua")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> SuaNhanVien(int maNv, string hoTen, string email, string sdt, string chucVu, int maRap, string? matKhauMoi, int? phongBanId, DateOnly? ngaySinh)
    {
        try
        {
            var nv = await _db.NhanViens.FindAsync(maNv);
            if (nv == null) throw new BusinessException("Không tìm thấy nhân viên.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && (rapChieuIdHienTai.Value != nv.RapChieuId || rapChieuIdHienTai.Value != maRap))
            {
                throw new BusinessException("Bạn chỉ có thể quản lý nhân viên của rạp mình phụ trách.");
            }

            hoTen = (hoTen ?? "").Trim();
            email = (email ?? "").Trim();
            sdt = (sdt ?? "").Trim();
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email))
            {
                throw new BusinessException("Họ tên và email không được để trống.");
            }
            KiemTraEmail(email);

            if (!AppRoles.IsValidNhanVienRole(chucVu))
            {
                throw new BusinessException("Chức vụ không hợp lệ.");
            }

            var rap = await _rapChieuService.FindByIdAsync(maRap);
            if (rap == null) throw new BusinessException("Không tìm thấy rạp chiếu.");

            var user = string.IsNullOrEmpty(nv.UserId) ? null : await _userManager.FindByIdAsync(nv.UserId);
            var laChinhMinh = user != null && user.Id == _userManager.GetUserId(User);
            if (laChinhMinh && !string.Equals(nv.ChucVu, chucVu, StringComparison.Ordinal))
            {
                throw new BusinessException("Bạn không thể tự đổi chức vụ của chính mình.");
            }

            // Email/SĐT không được trùng người khác.
            var khac = await _userManager.FindByEmailAsync(email);
            if ((khac != null && khac.Id != nv.UserId) || await _db.NhanViens.AnyAsync(n => n.Id != nv.Id && n.Email == email))
            {
                throw new BusinessException($"Email '{email}' đã được sử dụng bởi tài khoản khác.");
            }
            if (!string.IsNullOrEmpty(sdt) && await _db.NhanViens.AnyAsync(n => n.Id != nv.Id && n.Sdt == sdt))
            {
                throw new BusinessException($"Số điện thoại '{sdt}' đã thuộc về nhân viên khác.");
            }

            // Mật khẩu mới (nếu có): kiểm tra theo chính sách TRƯỚC khi sửa bất cứ thứ gì.
            var doiMatKhau = !string.IsNullOrWhiteSpace(matKhauMoi);
            if (doiMatKhau)
            {
                if (user == null) throw new BusinessException("Nhân viên này chưa có tài khoản đăng nhập nên không thể đặt mật khẩu.");
                await KiemTraMatKhauAsync(user, matKhauMoi!);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var emailDoi = user != null && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
            var chucVuDoi = !string.Equals(nv.ChucVu, chucVu, StringComparison.Ordinal);

            nv.HoTen = hoTen;
            nv.Email = email;
            nv.Sdt = sdt;
            nv.ChucVu = chucVu;
            nv.RapChieuId = maRap;
            nv.PhongBanId = phongBanId;
            nv.NgaySinh = ngaySinh;

            if (user != null)
            {
                user.Email = email;
                user.UserName = email;
                user.HoTen = hoTen;
                user.PhoneNumber = sdt;
                PhaiThanhCong(await _userManager.UpdateAsync(user));

                if (chucVuDoi || !await _userManager.IsInRoleAsync(user, chucVu))
                {
                    if (!await _roleManager.RoleExistsAsync(chucVu))
                    {
                        PhaiThanhCong(await _roleManager.CreateAsync(new IdentityRole(chucVu)));
                    }
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Count > 0) PhaiThanhCong(await _userManager.RemoveFromRolesAsync(user, currentRoles));
                    PhaiThanhCong(await _userManager.AddToRoleAsync(user, chucVu));
                }

                if (doiMatKhau)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    PhaiThanhCong(await _userManager.ResetPasswordAsync(user, token, matKhauMoi!));
                }

                // Đổi quyền/mật khẩu/email cần vô hiệu phiên đăng nhập cũ sớm (fix #10).
                if (chucVuDoi || doiMatKhau || emailDoi)
                {
                    PhaiThanhCong(await _userManager.UpdateSecurityStampAsync(user));
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["SuccessMessage"] = doiMatKhau
                ? "Cập nhật nhân viên và đổi mật khẩu thành công."
                : "Cập nhật nhân viên thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(NhanVien));
    }

    [HttpPost("NhanVien/Khoa/{maNv:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> KhoaMoTaiKhoanNhanVien(int maNv)
    {
        try
        {
            var nv = await _db.NhanViens.FindAsync(maNv);
            if (nv == null) throw new BusinessException("Không tìm thấy nhân viên.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != nv.RapChieuId)
            {
                throw new BusinessException("Bạn không có quyền thao tác trên nhân viên của rạp khác.");
            }

            if (!string.IsNullOrEmpty(nv.UserId) && nv.UserId == _userManager.GetUserId(User))
            {
                throw new BusinessException("Bạn không thể tự khóa tài khoản của chính mình.");
            }

            var daKhoa = nv.TrangThai == "Đã khóa";
            nv.TrangThai = daKhoa ? "Hoạt động" : "Đã khóa";

            if (!string.IsNullOrEmpty(nv.UserId))
            {
                var user = await _userManager.FindByIdAsync(nv.UserId);
                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = daKhoa ? null : DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                    // Buộc đăng xuất phiên hiện tại trong tối đa 1 phút thay vì chờ cookie hết hạn (fix #10).
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = daKhoa ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(NhanVien));
    }

    // ===================== RAP CHIEU / PHONG CHIEU =====================

    [HttpGet("RapChieu")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> RapChieu()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var query = _db.RapChieus.Include(r => r.PhongChieus).AsQueryable();
        if (rapChieuId.HasValue)
        {
            query = query.Where(r => r.Id == rapChieuId.Value);
        }
        ViewBag.DanhSachRap = await query.OrderBy(r => r.Id).ToListAsync();
        ViewBag.PageTitle = "Quản lý rạp chiếu";
        return View();
    }

    [HttpGet("RapChieu/{maRap:int}/Phong")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> GetPhongByRap(int maRap)
    {
        var danhSach = await _rapChieuService.FindPhongByRapAsync(maRap);
        var result = danhSach.Select(p => new PhongChieuJsonDto
        {
            MaPhong = p.Id,
            TenPhong = p.TenPhong,
            LoaiPhong = p.LoaiPhong,
            SucChua = p.SucChua
        }).ToList();
        return Json(result);
    }

    // Chỉ Admin toàn hệ thống được mở thêm rạp mới; QuanLy chỉ quản lý rạp đã được giao.
    [HttpPost("RapChieu/Them")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ThemRap(string tenRap, string diaChi, string? sdt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenRap) || string.IsNullOrWhiteSpace(diaChi))
            {
                throw new BusinessException("Tên rạp và địa chỉ không được để trống.");
            }

            var rap = new RapChieu
            {
                TenRap = tenRap,
                DiaChi = diaChi,
                Sdt = string.IsNullOrWhiteSpace(sdt) ? null : sdt
            };

            await _rapChieuService.SaveAsync(rap);
            // Sinh mã từ Id sau khi lưu (fix #13): tránh đụng mã khi 2 request cùng thêm rạp đồng thời.
            rap.MaRap = $"RAP{rap.Id:00}";
            await _rapChieuService.SaveAsync(rap);
            TempData["SuccessMessage"] = $"Thêm rạp chiếu thành công. Mã: {rap.MaRap}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    [HttpPost("RapChieu/Xoa/{maRap:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> XoaRap(int maRap)
    {
        try
        {
            await _rapChieuService.DeleteAsync(maRap);
            TempData["SuccessMessage"] = "Xóa rạp chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    [HttpPost("RapChieu/Sua")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> SuaRap(int maRap, string tenRap, string diaChi, string? sdt)
    {
        try
        {
            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != maRap)
            {
                throw new BusinessException("Bạn chỉ có thể sửa thông tin rạp mình phụ trách.");
            }

            var rap = await _rapChieuService.FindByIdAsync(maRap)
                ?? throw new BusinessException("Không tìm thấy rạp chiếu.");
            rap.TenRap = tenRap;
            rap.DiaChi = diaChi;
            rap.Sdt = string.IsNullOrWhiteSpace(sdt) ? null : sdt.Trim();

            await _rapChieuService.SaveAsync(rap);
            TempData["SuccessMessage"] = "Cập nhật rạp chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    [HttpPost("PhongChieu/Sua")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> SuaPhong(int maPhong, string tenPhong, string loaiPhong)
    {
        try
        {
            var phong = await _rapChieuService.FindPhongByIdAsync(maPhong)
                ?? throw new BusinessException("Không tìm thấy phòng chiếu.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != phong.RapChieuId)
            {
                throw new BusinessException("Bạn chỉ có thể sửa phòng chiếu của rạp mình phụ trách.");
            }

            // Sức chứa gắn với sơ đồ ghế đã sinh nên không đổi ở đây; chỉ sửa tên và loại phòng.
            phong.TenPhong = tenPhong;
            phong.LoaiPhong = loaiPhong;
            await _rapChieuService.SavePhongAsync(phong);
            TempData["SuccessMessage"] = "Cập nhật phòng chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    [HttpPost("PhongChieu/Xoa/{maPhong:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> XoaPhong(int maPhong)
    {
        try
        {
            var phong = await _rapChieuService.FindPhongByIdAsync(maPhong)
                ?? throw new BusinessException("Không tìm thấy phòng chiếu.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != phong.RapChieuId)
            {
                throw new BusinessException("Bạn chỉ có thể xóa phòng chiếu của rạp mình phụ trách.");
            }

            await _rapChieuService.DeletePhongAsync(maPhong);
            TempData["SuccessMessage"] = "Xóa phòng chiếu thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    [HttpPost("PhongChieu/Them")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> ThemPhong(int maRap, string tenPhong, string loaiPhong, int sucChua)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenPhong)) throw new BusinessException("Tên phòng không được để trống.");
            if (sucChua < 1) throw new BusinessException("Sức chứa phải lớn hơn 0.");

            var rapChieuIdHienTai = await _currentStaffService.GetRapChieuIdAsync();
            if (rapChieuIdHienTai.HasValue && rapChieuIdHienTai.Value != maRap)
            {
                throw new BusinessException("Bạn chỉ có thể thêm phòng chiếu cho rạp mình phụ trách.");
            }

            var rap = await _rapChieuService.FindByIdAsync(maRap);
            if (rap == null) throw new BusinessException("Không tìm thấy rạp chiếu.");

            var phong = new PhongChieu
            {
                RapChieuId = maRap,
                TenPhong = tenPhong,
                LoaiPhong = loaiPhong,
                SucChua = sucChua
            };

            // SavePhongAsync đã tự sinh ghế theo SucChua khi phòng mới (RapChieuService.SavePhongAsync),
            // không cần gọi GenerateGheForPhongAsync thêm lần nữa ở đây (trước đây gọi trùng 2 lần).
            var saved = await _rapChieuService.SavePhongAsync(phong);
            var soGheDaTao = await _gheService.CountByPhongAsync(saved.Id);

            TempData["SuccessMessage"] = $"Thêm phòng chiếu thành công, đã tạo tự động {soGheDaTao} ghế.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(RapChieu));
    }

    // ===================== KHUYEN MAI =====================

    [HttpGet("KhuyenMai")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> KhuyenMai()
    {
        ViewBag.DanhSachKM = await _khuyenMaiService.FindAllAsync();
        ViewBag.PageTitle = "Quản lý khuyến mãi";
        return View();
    }

    [HttpPost("KhuyenMai/Them")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> ThemKhuyenMai(string tenKm, string loaiGiam, decimal giaTri, DateOnly? ngayBatDau, DateOnly? ngayKetThuc)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenKm)) throw new BusinessException("Tên khuyến mãi không được để trống.");

            var km = new KhuyenMai
            {
                TenKm = tenKm,
                LoaiGiam = loaiGiam,
                GiaTri = giaTri,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc
            };

            await _khuyenMaiService.SaveAsync(km);
            TempData["SuccessMessage"] = "Thêm khuyến mãi thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(KhuyenMai));
    }

    [HttpPost("KhuyenMai/Sua")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> SuaKhuyenMai(int maKm, string tenKm, string loaiGiam, decimal giaTri, DateOnly? ngayBatDau, DateOnly? ngayKetThuc)
    {
        try
        {
            var km = await _khuyenMaiService.FindByIdAsync(maKm)
                ?? throw new ResourceNotFoundException("Khuyến mãi", "Id", maKm);

            if (string.IsNullOrWhiteSpace(tenKm)) throw new BusinessException("Tên khuyến mãi không được để trống.");

            km.TenKm = tenKm;
            km.LoaiGiam = loaiGiam;
            km.GiaTri = giaTri;
            km.NgayBatDau = ngayBatDau;
            km.NgayKetThuc = ngayKetThuc;

            await _khuyenMaiService.SaveAsync(km);
            TempData["SuccessMessage"] = "Cập nhật khuyến mãi thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(KhuyenMai));
    }

    [HttpPost("KhuyenMai/Xoa/{maKm:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> XoaKhuyenMai(int maKm)
    {
        try
        {
            await _khuyenMaiService.DeleteAsync(maKm);
            TempData["SuccessMessage"] = "Xóa khuyến mãi thành công.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
        }

        return RedirectToAction(nameof(KhuyenMai));
    }

    // ===================== THANH TOAN =====================

    [HttpGet("ThanhToan")]
    [Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> ThanhToan()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();
        var queryTT = _db.ThanhToans.Include(t => t.DatVe).AsQueryable();
        if (rapChieuId.HasValue)
        {
            // Quản lý chỉ thấy giao dịch của đơn thuộc rạp mình phụ trách (trước đây thấy toàn bộ các rạp).
            queryTT = queryTT.Where(t => t.DatVe!.Ves.Any(v => v.LichChieu!.PhongChieu!.RapChieuId == rapChieuId.Value));
        }
        ViewBag.DanhSachTT = await queryTT.OrderByDescending(t => t.NgayTt).ToListAsync();
        ViewBag.PageTitle = "Quản lý thanh toán";
        return View();
    }
}
