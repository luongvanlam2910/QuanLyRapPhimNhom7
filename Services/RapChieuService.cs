using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class RapChieuService : IRapChieuService
{
    private readonly ApplicationDbContext _db;
    private readonly IGheService _gheService;

    /// <summary>Mỗi hàng 10 ghế, tên hàng A–Z → tối đa 260 ghế; giới hạn 200 cho hợp lý.</summary>
    public const int MaxSucChua = 200;

    public RapChieuService(ApplicationDbContext db, IGheService gheService)
    {
        _db = db;
        _gheService = gheService;
    }

    public async Task<List<RapChieu>> FindAllAsync()
        => await _db.RapChieus.OrderBy(x => x.TenRap).ToListAsync();

    public async Task<RapChieu?> FindByIdAsync(int id)
        => await _db.RapChieus.FindAsync(id);

    public async Task<RapChieu> GetByIdAsync(int id)
        => await FindByIdAsync(id)
            ?? throw new ResourceNotFoundException("Rạp chiếu", "Id", id);

    public async Task<RapChieu> SaveAsync(RapChieu rapChieu)
    {
        rapChieu.TenRap = (rapChieu.TenRap ?? "").Trim();
        rapChieu.DiaChi = (rapChieu.DiaChi ?? "").Trim();
        if (string.IsNullOrWhiteSpace(rapChieu.TenRap) || string.IsNullOrWhiteSpace(rapChieu.DiaChi))
        {
            throw new BusinessException("Tên rạp và địa chỉ không được để trống.");
        }
        if (await _db.RapChieus.AnyAsync(r => r.TenRap == rapChieu.TenRap && r.Id != rapChieu.Id))
        {
            throw new BusinessException($"Rạp '{rapChieu.TenRap}' đã tồn tại.");
        }
        if (!string.IsNullOrWhiteSpace(rapChieu.Sdt) &&
            await _db.RapChieus.AnyAsync(r => r.Sdt == rapChieu.Sdt && r.Id != rapChieu.Id))
        {
            throw new BusinessException($"Số điện thoại '{rapChieu.Sdt}' đã được dùng bởi rạp khác.");
        }

        if (rapChieu.Id == 0)
        {
            _db.RapChieus.Add(rapChieu);
        }
        else if (_db.Entry(rapChieu).State == EntityState.Detached)
        {
            _db.RapChieus.Update(rapChieu);
        }
        await _db.SaveChangesAsync();
        return rapChieu;
    }

    public async Task DeleteAsync(int id)
    {
        var rap = await GetByIdAsync(id);

        var soNhanVien = await _db.NhanViens.CountAsync(n => n.RapChieuId == id);
        if (soNhanVien > 0)
        {
            throw new BusinessException($"Không thể xóa rạp '{rap.TenRap}' vì còn {soNhanVien} nhân viên thuộc rạp này. Hãy chuyển hoặc xóa nhân viên trước.");
        }
        var soSuat = await _db.LichChieus.CountAsync(l => l.PhongChieu!.RapChieuId == id);
        if (soSuat > 0)
        {
            throw new BusinessException($"Không thể xóa rạp '{rap.TenRap}' vì đã có {soSuat} suất chiếu (và vé đã bán) gắn với rạp này.");
        }

        _db.RapChieus.Remove(rap);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PhongChieu>> FindPhongByRapAsync(int rapChieuId)
        => await _db.PhongChieus
            .Where(x => x.RapChieuId == rapChieuId)
            .OrderBy(x => x.TenPhong)
            .ToListAsync();

    public async Task<PhongChieu?> FindPhongByIdAsync(int id)
        => await _db.PhongChieus.FindAsync(id);

    public async Task DeletePhongAsync(int id)
    {
        var phong = await _db.PhongChieus.FindAsync(id)
            ?? throw new ResourceNotFoundException("Phòng chiếu", "Id", id);

        var soSuat = await _db.LichChieus.CountAsync(l => l.PhongChieuId == id);
        if (soSuat > 0)
        {
            throw new BusinessException($"Không thể xóa phòng '{phong.TenPhong}' vì đã có {soSuat} suất chiếu (và vé đã bán) trong phòng này.");
        }

        _db.PhongChieus.Remove(phong); // ghế bị xóa theo (cascade)
        await _db.SaveChangesAsync();
    }

    public async Task<PhongChieu> SavePhongAsync(PhongChieu phong)
    {
        phong.TenPhong = (phong.TenPhong ?? "").Trim();
        if (string.IsNullOrWhiteSpace(phong.TenPhong))
        {
            throw new BusinessException("Tên phòng không được để trống.");
        }
        if (phong.SucChua < 1 || phong.SucChua > MaxSucChua)
        {
            throw new BusinessException($"Sức chứa phải từ 1 đến {MaxSucChua} ghế.");
        }
        if (await _db.PhongChieus.AnyAsync(p => p.RapChieuId == phong.RapChieuId && p.TenPhong == phong.TenPhong && p.Id != phong.Id))
        {
            throw new BusinessException($"Rạp này đã có phòng tên '{phong.TenPhong}'.");
        }

        bool isNew = phong.Id == 0;

        if (isNew)
        {
            _db.PhongChieus.Add(phong);
            await _db.SaveChangesAsync();
            // Sinh mã từ Id sau khi lưu (fix #13): tránh đụng mã khi 2 request thêm phòng đồng thời
            // cùng đọc được Count() cũ trước khi cái nào SaveChanges trước.
            if (string.IsNullOrWhiteSpace(phong.MaPhong))
            {
                phong.MaPhong = $"P{phong.Id:D4}";
            }
        }
        else if (_db.Entry(phong).State == EntityState.Detached)
        {
            _db.PhongChieus.Update(phong);
        }
        await _db.SaveChangesAsync();

        // Tự động sinh ghế cho phòng mới (giống GheService.generateGheForPhong trong Java)
        if (isNew)
        {
            await _gheService.GenerateGheForPhongAsync(phong);
        }

        return phong;
    }
}
