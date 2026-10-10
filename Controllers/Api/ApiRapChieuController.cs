using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Api;

/// <summary>Web API RESTful công khai cho rạp chiếu và phòng chiếu.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/rap-chieu")]
[Produces("application/json")]
public class ApiRapChieuController : ControllerBase
{
    private readonly IRapChieuService _rapChieuService;

    public ApiRapChieuController(IRapChieuService rapChieuService)
    {
        _rapChieuService = rapChieuService;
    }

    /// <summary>GET /api/rap-chieu</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RapChieuDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var danhSach = await _rapChieuService.FindAllAsync();
        return Ok(danhSach.Select(r => new RapChieuDto(r.Id, r.MaRap, r.TenRap, r.DiaChi, r.Sdt, r.PhongChieus.Count)));
    }

    /// <summary>GET /api/rap-chieu/{id}</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RapChieuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var rap = await _rapChieuService.FindByIdAsync(id);
        if (rap == null)
        {
            return NotFound(new ApiErrorDto($"Không tìm thấy rạp chiếu với Id = {id}"));
        }
        return Ok(new RapChieuDto(rap.Id, rap.MaRap, rap.TenRap, rap.DiaChi, rap.Sdt, rap.PhongChieus?.Count ?? 0));
    }

    /// <summary>GET /api/rap-chieu/{id}/phong — danh sách phòng chiếu của 1 rạp.</summary>
    [HttpGet("{id:int}/phong")]
    [ProducesResponseType(typeof(IEnumerable<PhongChieuDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPhongByRap(int id)
    {
        var danhSach = await _rapChieuService.FindPhongByRapAsync(id);
        return Ok(danhSach.Select(p => new PhongChieuDto(p.Id, p.MaPhong, p.TenPhong, p.LoaiPhong, p.SucChua, p.RapChieuId)));
    }
}
