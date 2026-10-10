using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class PhimService : IPhimService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public PhimService(ApplicationDbContext db, IWebHostEnvironment env, IConfiguration configuration)
    {
        _db = db;
        _env = env;
        _configuration = configuration;
    }

    public async Task<List<Phim>> FindAllAsync()
        => await _db.Phims.Include(p => p.TheLoais).ToListAsync();

    public async Task<List<Phim>> FindDangChieuAsync()
        => await _db.Phims
            .Include(p => p.TheLoais)
            .Where(p => p.TrangThai == TrangThaiPhim.DangChieu)
            .OrderByDescending(p => p.NgayKhoiChieu)
            .ToListAsync();

    public async Task<List<Phim>> FindSapChieuAsync()
        => await _db.Phims
            .Include(p => p.TheLoais)
            .Where(p => p.TrangThai == TrangThaiPhim.SapChieu)
            .OrderByDescending(p => p.NgayKhoiChieu)
            .ToListAsync();

    public async Task<Phim?> FindByIdAsync(int id)
        => await _db.Phims.Include(p => p.TheLoais).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Phim> GetByIdAsync(int id)
        => await FindByIdAsync(id) ?? throw new ResourceNotFoundException("Phim", "Id", id);

    public async Task<PhimSearchResult> SearchAsync(string? tenPhim, int? theLoaiId, string? trangThai, int page, int size)
    {
        var query = _db.Phims.Include(p => p.TheLoais).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenPhim))
        {
            query = query.Where(p => p.TenPhim.ToLower().Contains(tenPhim.ToLower()));
        }
        if (theLoaiId.HasValue)
        {
            query = query.Where(p => p.TheLoais.Any(tl => tl.Id == theLoaiId.Value));
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(p => p.TrangThai == trangThai);
        }

        query = query.OrderByDescending(p => p.NgayKhoiChieu);

        var total = await query.CountAsync();

        // SQL Server 2008 R2 không hỗ trợ OFFSET/FETCH (chỉ có từ SQL 2012 trở lên) — EF Core 8 vẫn sinh
        // cú pháp đó cho Skip/Take dù đã cấu hình UseCompatibilityLevel(100), nên phân trang trong bộ nhớ.
        var items = (await query.ToListAsync())
            .Skip(page * size)
            .Take(size)
            .ToList();

        return new PhimSearchResult
        {
            Items = items,
            TotalCount = total,
            Page = page,
            Size = size
        };
    }

    public async Task<Phim> SaveAsync(Phim phim, List<int>? theLoaiIds, IFormFile? posterFile)
    {
        bool isNew = phim.Id == 0;

        if (isNew && string.IsNullOrWhiteSpace(phim.MaPhim))
        {
            phim.MaPhim = await GenerateMaPhimAsync();
        }

        if (posterFile is { Length: > 0 })
        {
            phim.AnhPoster = await SavePosterFileAsync(posterFile);
        }

        List<TheLoai> managedTheLoais = theLoaiIds is { Count: > 0 }
            ? await _db.TheLoais.Where(tl => theLoaiIds.Contains(tl.Id)).ToListAsync()
            : new List<TheLoai>();

        if (isNew)
        {
            phim.TheLoais = managedTheLoais;
            _db.Phims.Add(phim);
        }
        else
        {
            var existing = await _db.Phims.Include(p => p.TheLoais).FirstOrDefaultAsync(p => p.Id == phim.Id)
                ?? throw new ResourceNotFoundException("Phim", "Id", phim.Id);

            existing.TenPhim = phim.TenPhim;
            existing.ThoiLuong = phim.ThoiLuong;
            existing.DaoDien = phim.DaoDien;
            existing.DoTuoi = phim.DoTuoi;
            existing.NgayKhoiChieu = phim.NgayKhoiChieu;
            existing.TrangThai = phim.TrangThai;
            existing.MoTa = phim.MoTa;
            if (!string.IsNullOrWhiteSpace(phim.AnhPoster))
            {
                existing.AnhPoster = phim.AnhPoster;
            }

            existing.TheLoais.Clear();
            foreach (var tl in managedTheLoais)
            {
                existing.TheLoais.Add(tl);
            }

            phim = existing;
        }

        await _db.SaveChangesAsync();
        return phim;
    }

    public async Task DeleteAsync(int id)
    {
        var phim = await _db.Phims
            .Include(p => p.TheLoais)
            .Include(p => p.LichChieus)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new ResourceNotFoundException("Phim", "Id", id);

        if (phim.LichChieus.Count > 0)
        {
            throw new BusinessException(
                $"Không thể xóa phim '{phim.TenPhim}' vì đã có {phim.LichChieus.Count} lịch chiếu!");
        }

        phim.TheLoais.Clear();
        _db.Phims.Remove(phim);
        await _db.SaveChangesAsync();
    }

    public async Task<TheLoai?> FindTheLoaiByIdAsync(int id)
        => await _db.TheLoais.FindAsync(id);

    public async Task<List<TheLoai>> FindAllTheLoaiAsync()
        => await _db.TheLoais.Include(tl => tl.Phims).OrderBy(tl => tl.TenTheLoai).ToListAsync();

    public async Task<TheLoai> SaveTheLoaiAsync(TheLoai theLoai)
    {
        if (string.IsNullOrWhiteSpace(theLoai.TenTheLoai))
        {
            throw new BusinessException("Tên thể loại không được để trống.");
        }

        theLoai.TenTheLoai = theLoai.TenTheLoai.Trim();
        if (await _db.TheLoais.AnyAsync(t => t.TenTheLoai == theLoai.TenTheLoai && t.Id != theLoai.Id))
        {
            throw new BusinessException($"Thể loại '{theLoai.TenTheLoai}' đã tồn tại.");
        }

        bool isNew = theLoai.Id == 0;
        if (isNew)
        {
            _db.TheLoais.Add(theLoai);
            await _db.SaveChangesAsync();
            if (string.IsNullOrWhiteSpace(theLoai.MaTheLoai))
            {
                theLoai.MaTheLoai = $"TL{theLoai.Id:D3}";
            }
        }
        else
        {
            if (_db.Entry(theLoai).State == EntityState.Detached)
            {
                _db.TheLoais.Update(theLoai);
            }
        }
        await _db.SaveChangesAsync();
        return theLoai;
    }

    public async Task DeleteTheLoaiAsync(int id)
    {
        var tl = await _db.TheLoais.Include(t => t.Phims).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new ResourceNotFoundException("Thể loại", "Id", id);

        if (tl.Phims.Count > 0)
        {
            throw new BusinessException($"Không thể xóa thể loại '{tl.TenTheLoai}' vì còn {tl.Phims.Count} phim thuộc thể loại này.");
        }

        _db.TheLoais.Remove(tl);
        await _db.SaveChangesAsync();
    }

    private async Task<string> GenerateMaPhimAsync()
    {
        var maxMa = await _db.Phims
            .Where(p => p.MaPhim != null)
            .OrderByDescending(p => p.MaPhim)
            .Select(p => p.MaPhim)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(maxMa))
        {
            return "PH001";
        }

        var digits = new string(maxMa.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var num))
        {
            return $"PH{num + 1:D3}";
        }

        var count = await _db.Phims.CountAsync();
        return $"PH{count + 1:D3}";
    }

    private async Task<string> SavePosterFileAsync(IFormFile file)
    {
        var uploadDir = _configuration["app:UploadDir"];
        string targetDir = !string.IsNullOrWhiteSpace(uploadDir)
            ? Path.Combine(uploadDir, "images", "posters")
            : Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "images", "posters");

        Directory.CreateDirectory(targetDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(targetDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/images/posters/{fileName}";
    }
}
