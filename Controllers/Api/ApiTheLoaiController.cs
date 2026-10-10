using System.ComponentModel.DataAnnotations;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatVeXemPhim.Controllers.Api;

/// <summary>Dữ liệu gửi lên khi tạo/sửa thể loại (validate bằng Data Annotations).</summary>
public class TheLoaiInput
{
    [Required(ErrorMessage = "Tên thể loại là bắt buộc")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Tên thể loại từ 2 đến 80 ký tự")]
    public string TenTheLoai { get; set; } = string.Empty;
}

/// <summary>
/// Web API RESTful đầy đủ CRUD cho thể loại phim:
/// GET (công khai), POST/PUT/DELETE (yêu cầu đăng nhập vai trò Admin/QuanLy).
/// Mã trạng thái: 200 OK, 201 Created, 204 NoContent, 400 BadRequest, 401/403, 404 NotFound, 409 Conflict.
/// </summary>
[ApiController]
[Route("api/the-loai")]
[Produces("application/json")]
public class ApiTheLoaiController : ControllerBase
{
    private readonly IPhimService _phimService;

    public ApiTheLoaiController(IPhimService phimService)
    {
        _phimService = phimService;
    }

    private static TheLoaiDto ToDto(TheLoai t) => new(t.Id, t.MaTheLoai, t.TenTheLoai);

    /// <summary>GET /api/the-loai</summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll()
        => Ok((await _phimService.FindAllTheLoaiAsync()).Select(ToDto));

    /// <summary>GET /api/the-loai/{id}</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var tl = (await _phimService.FindAllTheLoaiAsync()).FirstOrDefault(t => t.Id == id);
        return tl == null ? NotFound(new ApiErrorDto($"Không tìm thấy thể loại Id = {id}")) : Ok(ToDto(tl));
    }

    /// <summary>POST /api/the-loai — tạo mới, trả 201 + header Location.</summary>
    [HttpPost, Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> Create([FromBody] TheLoaiInput input)
    {
        var saved = await _phimService.SaveTheLoaiAsync(new TheLoai { TenTheLoai = input.TenTheLoai.Trim() });
        return CreatedAtAction(nameof(GetById), new { id = saved.Id }, ToDto(saved));
    }

    /// <summary>PUT /api/the-loai/{id} — cập nhật toàn bộ tên thể loại.</summary>
    [HttpPut("{id:int}"), Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] TheLoaiInput input)
    {
        var tl = (await _phimService.FindAllTheLoaiAsync()).FirstOrDefault(t => t.Id == id);
        if (tl == null)
        {
            return NotFound(new ApiErrorDto($"Không tìm thấy thể loại Id = {id}"));
        }
        // tl đã được DbContext theo dõi: sửa trực tiếp rồi lưu (tạo instance mới cùng Id sẽ gây xung đột tracking)
        tl.TenTheLoai = input.TenTheLoai.Trim();
        var saved = await _phimService.SaveTheLoaiAsync(tl);
        return Ok(ToDto(saved));
    }

    /// <summary>DELETE /api/the-loai/{id} — 204 nếu xóa được, 409 nếu còn phim thuộc thể loại.</summary>
    [HttpDelete("{id:int}"), Authorize(Policy = "QuanLyOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _phimService.DeleteTheLoaiAsync(id);
            return NoContent();
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorDto(ex.Message));
        }
        catch (BusinessException ex)
        {
            return Conflict(new ApiErrorDto(ex.Message));
        }
    }
}
