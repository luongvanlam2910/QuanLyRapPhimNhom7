using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Api;

/// <summary>Web API RESTful công khai cho lịch chiếu và tình trạng ghế theo suất.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/lich-chieu")]
[Produces("application/json")]
public class ApiLichChieuController : ControllerBase
{
    private readonly ILichChieuService _lichChieuService;
    private readonly IGheService _gheService;

    public ApiLichChieuController(ILichChieuService lichChieuService, IGheService gheService)
    {
        _lichChieuService = lichChieuService;
        _gheService = gheService;
    }

    private static LichChieuDto ToDto(Models.Entities.LichChieu l) => new(
        l.Id, l.MaLich, l.PhimId, l.Phim?.TenPhim, l.PhongChieuId, l.PhongChieu?.TenPhong,
        l.PhongChieu?.RapChieuId ?? 0, l.PhongChieu?.RapChieu?.TenRap,
        l.ThoiGianBatDau, l.ThoiGianKetThuc, l.GiaVe, l.TrangThai);

    /// <summary>GET /api/lich-chieu?phimId=&rapId=&ngay=yyyy-MM-dd — lịch chiếu đang mở bán của 1 phim tại 1 rạp, theo ngày.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LichChieuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(int? phimId, int? rapId, DateOnly? ngay)
    {
        if (!phimId.HasValue || !rapId.HasValue)
        {
            return BadRequest(new ApiErrorDto("Cần truyền đủ tham số phimId và rapId."));
        }

        var ngayXem = (ngay ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
        var danhSach = await _lichChieuService.FindByPhimRapNgayAsync(phimId.Value, rapId.Value, ngayXem);
        return Ok(danhSach.Select(ToDto));
    }

    /// <summary>GET /api/lich-chieu/{id}</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LichChieuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var lc = await _lichChieuService.FindByIdAsync(id);
        if (lc == null)
        {
            return NotFound(new ApiErrorDto($"Không tìm thấy lịch chiếu với Id = {id}"));
        }
        return Ok(ToDto(lc));
    }

    /// <summary>GET /api/lich-chieu/{id}/ghe — sơ đồ ghế và tình trạng (trống/đã đặt/giữ tạm/bảo trì) của 1 suất chiếu.</summary>
    [HttpGet("{id:int}/ghe")]
    [ProducesResponseType(typeof(IEnumerable<GheTrangThaiDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGhe(int id)
    {
        var lc = await _lichChieuService.FindByIdAsync(id);
        if (lc == null)
        {
            return NotFound(new ApiErrorDto($"Không tìm thấy lịch chiếu với Id = {id}"));
        }

        var ghes = await _gheService.GetTrangThaiGheForLichAsync(id, null);
        return Ok(ghes.Select(g => new GheTrangThaiDto(g.GheId, g.MaGhe, g.HangGhe, g.SoGhe, g.LoaiGhe, g.TrangThai)));
    }
}
