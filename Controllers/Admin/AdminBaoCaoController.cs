using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Admin;

[Authorize(Policy = "QuanLyOnly")]
[Route("Admin/BaoCao")]
public class AdminBaoCaoController : Controller
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly ICurrentStaffService _currentStaffService;

    public AdminBaoCaoController(IBaoCaoService baoCaoService, ICurrentStaffService currentStaffService)
    {
        _baoCaoService = baoCaoService;
        _currentStaffService = currentStaffService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var rapChieuId = await _currentStaffService.GetRapChieuIdAsync();

        ViewBag.DoanhThuPhim = await _baoCaoService.GetDoanhThuTheoPhimAsync(rapChieuId);
        ViewBag.DoanhThuRap = await _baoCaoService.GetDoanhThuTheoRapAsync(rapChieuId);
        ViewBag.DoanhThuPhuongThuc = await _baoCaoService.GetDoanhThuTheoPhuongThucAsync(rapChieuId);
        ViewBag.Stats = await _baoCaoService.GetDashboardStatsAsync(rapChieuId);
        ViewBag.PageTitle = "Báo cáo doanh thu";
        return View();
    }
}
