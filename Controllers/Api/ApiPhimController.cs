using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Api;

/// <summary>
/// Web API RESTful công khai cho phim: GET danh sách/chi tiết, tìm kiếm có phân trang/lọc.
/// Quy ước REST: danh từ số nhiều "phim" là resource, id trên đường dẫn xác định 1 bản ghi,
/// mã trạng thái HTTP phản ánh đúng kết quả (200/404).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/phim")]
[Produces("application/json")]
public class ApiPhimController : ControllerBase
{
    private readonly IPhimService _phimService;

    public ApiPhimController(IPhimService phimService)
    {
        _phimService = phimService;
    }

    private static PhimDto ToDto(Models.Entities.Phim p) => new(
        p.Id, p.MaPhim, p.TenPhim, p.ThoiLuong, p.DaoDien, p.DoTuoi,
        p.NgayKhoiChieu, p.TrangThai, p.AnhPoster, p.MoTa,
        p.TheLoais.Select(t => t.TenTheLoai).ToList());

    /// <summary>GET /api/phim?tenPhim=&maTheLoai=&trangThai=&page=0&size=12</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PhimDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string? tenPhim, int? maTheLoai, string? trangThai, int page = 0, int size = 12)
    {
        var ketQua = await _phimService.SearchAsync(tenPhim, maTheLoai, trangThai, page, size);
        return Ok(new
        {
            items = ketQua.Items.Select(ToDto),
            totalCount = ketQua.TotalCount,
            page = ketQua.Page,
            size = ketQua.Size,
            totalPages = ketQua.TotalPages
        });
    }

    /// <summary>GET /api/phim/{id}</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PhimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var phim = await _phimService.FindByIdAsync(id);
        if (phim == null)
        {
            return NotFound(new ApiErrorDto($"Không tìm thấy phim với Id = {id}"));
        }
        return Ok(ToDto(phim));
    }

    /// <summary>GET /api/phim/the-loai — danh mục thể loại dùng cho bộ lọc.</summary>
    [HttpGet("the-loai")]
    [ProducesResponseType(typeof(IEnumerable<TheLoaiDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTheLoai()
    {
        var theLoais = await _phimService.FindAllTheLoaiAsync();
        return Ok(theLoais.Select(t => new TheLoaiDto(t.Id, t.MaTheLoai, t.TenTheLoai)));
    }
}
