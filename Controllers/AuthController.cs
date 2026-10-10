using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Đăng nhập - UTC Cinema";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        ViewData["Title"] = "Đăng nhập - UTC Cinema";
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        // lockoutOnFailure: true — khóa tạm 15' sau 5 lần sai (xem Program.cs Lockout), chặn brute-force (fix #10).
        var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.MatKhau, isPersistent: true, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            {
                return Redirect(vm.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút.");
            return View(vm);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng. Vui lòng thử lại.");
        return View(vm);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "Đăng ký tài khoản - UTC Cinema";
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        ViewData["Title"] = "Đăng ký tài khoản - UTC Cinema";
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var existing = await _userManager.FindByEmailAsync(vm.Email);
        if (existing != null)
        {
            ViewData["errorMessage"] = "Email này đã được đăng ký. Vui lòng sử dụng email khác.";
            return View(vm);
        }

        var sdtDaTonTai = await _db.KhachHangs.AnyAsync(k => k.Sdt == vm.Sdt);
        if (sdtDaTonTai)
        {
            ViewData["errorMessage"] = "Số điện thoại này đã được đăng ký. Vui lòng sử dụng số khác.";
            return View(vm);
        }

        if (vm.NgaySinh > DateOnly.FromDateTime(DateTime.Now) || vm.NgaySinh < new DateOnly(1900, 1, 1))
        {
            ViewData["errorMessage"] = "Ngày sinh không hợp lệ.";
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            HoTen = vm.HoTen,
            PhoneNumber = vm.Sdt,
        };

        if (!await _roleManager.RoleExistsAsync("KhachHang"))
        {
            await _roleManager.CreateAsync(new IdentityRole("KhachHang"));
        }

        // Tài khoản + vai trò + hồ sơ khách hàng trong 1 giao dịch: lỗi ở bước nào cũng không để lại tài khoản mồ côi.
        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var createResult = await _userManager.CreateAsync(user, vm.MatKhau);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(vm);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "KhachHang");
            if (!roleResult.Succeeded)
            {
                throw new DatVeXemPhim.Exceptions.BusinessException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
            }

            var khachHang = new KhachHang
            {
                UserId = user.Id,
                HoTen = vm.HoTen,
                Sdt = vm.Sdt,
                Email = vm.Email,
                NgaySinh = vm.NgaySinh,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
            };
            _db.KhachHangs.Add(khachHang);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            ViewData["errorMessage"] = "Không thể đăng ký: " + DatVeXemPhim.Infrastructure.ErrorMessages.From(ex);
            return View(vm);
        }

        TempData["successMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Truy cập bị từ chối";
        return View();
    }

    [HttpGet("/tai-khoan")]
    [Authorize]
    public async Task<IActionResult> TaiKhoan()
    {
        var userId = _userManager.GetUserId(User);
        var kh = await _db.KhachHangs.FirstOrDefaultAsync(k => k.UserId == userId);
        ViewData["Title"] = "Tài khoản của tôi - UTC Cinema";
        return View(new TaiKhoanViewModel { KhachHang = kh });
    }

    [HttpPost("/tai-khoan/cap-nhat")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatTaiKhoan(string hoTen, string sdt, DateOnly? ngaySinh)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            var kh = await _db.KhachHangs.FirstOrDefaultAsync(k => k.UserId == userId)
                      ?? throw new Exception("Không tìm thấy tài khoản");
            hoTen = (hoTen ?? "").Trim();
            sdt = (sdt ?? "").Trim();
            if (string.IsNullOrWhiteSpace(hoTen))
            {
                throw new DatVeXemPhim.Exceptions.BusinessException("Họ tên không được để trống.");
            }
            if (!new System.ComponentModel.DataAnnotations.PhoneAttribute().IsValid(sdt) || sdt.Length < 8)
            {
                throw new DatVeXemPhim.Exceptions.BusinessException("Số điện thoại không hợp lệ.");
            }
            if (await _db.KhachHangs.AnyAsync(k => k.Sdt == sdt && k.Id != kh.Id))
            {
                throw new DatVeXemPhim.Exceptions.BusinessException("Số điện thoại này đã được tài khoản khác sử dụng.");
            }

            kh.HoTen = hoTen;
            kh.Sdt = sdt;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                user.HoTen = hoTen;
                user.PhoneNumber = sdt;
                await _userManager.UpdateAsync(user);
            }

            // Chỉ cho phép khai báo ngày sinh một lần khi đang trống (chống sửa đi sửa lại để lách kiểm tra độ tuổi).
            if (kh.NgaySinh == null && ngaySinh.HasValue
                && ngaySinh.Value <= DateOnly.FromDateTime(DateTime.Now) && ngaySinh.Value >= new DateOnly(1900, 1, 1))
            {
                kh.NgaySinh = ngaySinh.Value;
            }

            await _db.SaveChangesAsync();
            TempData["successMessage"] = "Cập nhật thông tin thành công";
        }
        catch (Exception e)
        {
            TempData["errorMessage"] = DatVeXemPhim.Infrastructure.ErrorMessages.From(e);
        }
        return RedirectToAction(nameof(TaiKhoan));
    }
}
