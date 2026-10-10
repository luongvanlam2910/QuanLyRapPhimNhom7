using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class PhongBanService : IPhongBanService
{
    private readonly ApplicationDbContext _db;

    public PhongBanService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<PhongBan>> FindAllAsync()
        => await _db.PhongBans.Include(p => p.NhanViens).OrderBy(p => p.TenPb).ToListAsync();

    public async Task<PhongBan?> FindByIdAsync(int id)
        => await _db.PhongBans.FindAsync(id);

    public async Task<PhongBan> SaveAsync(PhongBan phongBan)
    {
        if (string.IsNullOrWhiteSpace(phongBan.TenPb))
        {
            throw new BusinessException("Tên phòng ban không được để trống.");
        }

        phongBan.TenPb = phongBan.TenPb.Trim();
        if (await _db.PhongBans.AnyAsync(p => p.TenPb == phongBan.TenPb && p.Id != phongBan.Id))
        {
            throw new BusinessException($"Phòng ban '{phongBan.TenPb}' đã tồn tại.");
        }

        bool isNew = phongBan.Id == 0;
        if (isNew)
        {
            _db.PhongBans.Add(phongBan);
            await _db.SaveChangesAsync();
            if (string.IsNullOrWhiteSpace(phongBan.MaPb))
            {
                phongBan.MaPb = $"PB{phongBan.Id:D3}";
            }
        }
        else
        {
            if (_db.Entry(phongBan).State == EntityState.Detached)
            {
                _db.PhongBans.Update(phongBan);
            }
        }
        await _db.SaveChangesAsync();
        return phongBan;
    }

    public async Task DeleteAsync(int id)
    {
        var pb = await _db.PhongBans
            .Include(p => p.NhanViens)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new ResourceNotFoundException("Phòng ban", "Id", id);

        if (pb.NhanViens.Count > 0)
        {
            throw new BusinessException($"Không thể xóa phòng ban '{pb.TenPb}' vì còn {pb.NhanViens.Count} nhân viên thuộc phòng ban này.");
        }

        _db.PhongBans.Remove(pb);
        await _db.SaveChangesAsync();
    }
}
