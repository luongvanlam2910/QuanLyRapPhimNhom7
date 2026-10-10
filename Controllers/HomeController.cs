using System.Diagnostics;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IPhimService _phimService;
    private readonly IRapChieuService _rapChieuService;

    public HomeController(IPhimService phimService, IRapChieuService rapChieuService)
    {
        _phimService = phimService;
        _rapChieuService = rapChieuService;
    }

    public const string SessionPhimDaXem = "PhimDaXem";

    public async Task<IActionResult> Index()
    {
        var vm = new HomeIndexViewModel
        {
            PhimDangChieu = await _phimService.FindDangChieuAsync(),
            PhimSapChieu = await _phimService.FindSapChieuAsync(),
            DanhSachRap = await _rapChieuService.FindAllAsync(),
            DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync(),
        };
        // Đọc từ Session các phim khách đã xem gần đây
        var daXem = (HttpContext.Session.GetString(SessionPhimDaXem) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        var tatCa = vm.PhimDangChieu.Concat(vm.PhimSapChieu).ToList();
        ViewBag.PhimDaXem = daXem.Select(i => tatCa.FirstOrDefault(p => p.Id == i)).Where(p => p != null).ToList();
        ViewData["Title"] = "UTC Cinema - Đặt Vé Xem Phim Online";
        ViewData["danhSachRap"] = vm.DanhSachRap;
        return View(vm);
    }

    [HttpGet("/tim-kiem")]
    public async Task<IActionResult> TimKiem(string? tenPhim, int? maTheLoai, string? trangThai, int page = 0)
    {
        var ketQua = await _phimService.SearchAsync(tenPhim, maTheLoai, trangThai, page, 12);
        var vm = new TimKiemViewModel
        {
            KetQua = ketQua,
            DanhSachTheLoai = await _phimService.FindAllTheLoaiAsync(),
            TenPhimSearch = tenPhim,
            MaTheLoaiSearch = maTheLoai,
            TrangThaiSearch = trangThai,
        };
        ViewData["Title"] = "Tìm kiếm phim - UTC Cinema";
        return View(vm);
    }

    [HttpGet("/403")]
    public IActionResult Forbidden()
    {
        ViewData["Title"] = "Truy cập bị từ chối - UTC Cinema";
        return View("~/Views/Shared/_403.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
