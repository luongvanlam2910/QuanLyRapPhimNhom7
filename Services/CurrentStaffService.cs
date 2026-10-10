using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class CurrentStaffService : ICurrentStaffService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    private bool _loaded;
    private int? _rapChieuId;
    private int? _nhanVienId;

    public CurrentStaffService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _db = db;
    }

    public async Task<int?> GetRapChieuIdAsync()
    {
        if (_loaded) return _rapChieuId;
        _loaded = true;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.IsInRole(AppRoles.Admin))
        {
            _rapChieuId = null; // Admin: không giới hạn
            return _rapChieuId;
        }

        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            _rapChieuId = null;
            return _rapChieuId;
        }

        var nv = await _db.NhanViens.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId);
        _rapChieuId = nv?.RapChieuId;
        _nhanVienId = nv?.Id;
        return _rapChieuId;
    }

    public async Task<int?> GetNhanVienIdAsync()
    {
        await GetRapChieuIdAsync(); // nạp cache (kể cả _nhanVienId)
        if (_nhanVienId.HasValue) return _nhanVienId;

        // Admin: GetRapChieuIdAsync trả sớm; tra riêng hồ sơ nhân viên nếu Admin có hồ sơ.
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user == null ? null : _userManager.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return null;
        _nhanVienId = await _db.NhanViens.AsNoTracking().Where(n => n.UserId == userId).Select(n => (int?)n.Id).FirstOrDefaultAsync();
        return _nhanVienId;
    }

    public bool LaQuanLyTroLen
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user != null && (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.QuanLy));
        }
    }
}
