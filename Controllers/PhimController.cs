using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers;

[AllowAnonymous]
[Route("phim")]
public class PhimController : Controller
{
    private readonly IPhimService _phimService;
    private readonly ILichChieuService _lichChieuService;
    private readonly IRapChieuService _rapChieuService;

    public PhimController(IPhimService phimService, ILichChieuService lichChieuService, IRapChieuService rapChieuService)
    {
        _phimService = phimService;
        _lichChieuService = lichChieuService;
        _rapChieuService = rapChieuService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var phim = await _phimService.FindByIdAsync(id);
        if (phim == null)
        {
            return NotFound();
        }

        GhiNhoPhimDaXem(id);

        var vm = new PhimDetailViewModel
        {
            Phim = phim,
            DanhSachRap = await _rapChieuService.FindAllAsync(),
        };
        ViewData["Title"] = phim.TenPhim + " - UTC Cinema";
        return View(vm);
    }

    /// <summary>Lưu tối đa 6 phim khách vừa xem vào Session (mới nhất đứng đầu).</summary>
    private void GhiNhoPhimDaXem(int id)
    {
        var ids = (HttpContext.Session.GetString(HomeController.SessionPhimDaXem) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse).Where(x => x != id).ToList();
        ids.Insert(0, id);
        HttpContext.Session.SetString(HomeController.SessionPhimDaXem, string.Join(',', ids.Take(6)));
    }

    [HttpGet("lich-chieu")]
    public async Task<IActionResult> LichChieu(int maPhim, int maRap, DateOnly ngay)
    {
        var ngayDt = ngay.ToDateTime(TimeOnly.MinValue);
        var lichChieus = await _lichChieuService.FindByPhimRapNgayAsync(maPhim, maRap, ngayDt);

        var result = lichChieus.Select(lc => new
        {
            maLich = lc.Id,
            maPhim = lc.PhimId,
            tenPhim = lc.Phim?.TenPhim,
            maPhong = lc.PhongChieuId,
            tenPhong = lc.PhongChieu?.TenPhong,
            maRap = lc.PhongChieu?.RapChieuId,
            tenRap = lc.PhongChieu?.RapChieu?.TenRap,
            thoiGianBatDau = lc.ThoiGianBatDau,
            thoiGianKetThuc = lc.ThoiGianKetThuc,
            giaVe = lc.GiaVe,
            trangThai = lc.TrangThai,
        }).ToList();

        return Json(result);
    }
}
