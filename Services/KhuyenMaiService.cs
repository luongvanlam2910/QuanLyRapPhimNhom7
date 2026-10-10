using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class KhuyenMaiService : IKhuyenMaiService
{
    private readonly ApplicationDbContext _db;

    public KhuyenMaiService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<KhuyenMai>> FindAllAsync()
        => await _db.KhuyenMais.ToListAsync();

    public async Task<List<KhuyenMai>> FindHieuLucAsync(DateOnly? ngayHienTai = null)
    {
        var today = ngayHienTai ?? DateOnly.FromDateTime(DateTime.Now);
        return await _db.KhuyenMais
            .Where(k => (k.NgayBatDau == null || k.NgayBatDau <= today)
                && (k.NgayKetThuc == null || k.NgayKetThuc >= today))
            .ToListAsync();
    }

    public async Task<KhuyenMai?> FindByIdAsync(int id)
        => await _db.KhuyenMais.FindAsync(id);

    public async Task<KhuyenMai> SaveAsync(KhuyenMai km)
    {
        if (string.IsNullOrWhiteSpace(km.TenKm))
        {
            throw new BusinessException("Tên khuyến mãi không được để trống.");
        }
        if (km.LoaiGiam != "PERCENT" && km.LoaiGiam != "AMOUNT")
        {
            throw new BusinessException("Loại giảm giá không hợp lệ (chỉ chấp nhận theo % hoặc theo số tiền).");
        }
        if (km.GiaTri <= 0)
        {
            throw new BusinessException("Giá trị khuyến mãi phải lớn hơn 0.");
        }
        if (km.LoaiGiam == "AMOUNT" && km.GiaTri > 10_000_000)
        {
            throw new BusinessException("Số tiền giảm quá lớn (tối đa 10.000.000 ₫).");
        }
        if (km.LoaiGiam == "PERCENT" && km.GiaTri is < 0 or > 100)
        {
            throw new BusinessException("Khuyến mãi theo % phải trong khoảng 0-100.");
        }
        if (km.NgayBatDau.HasValue && km.NgayKetThuc.HasValue && km.NgayKetThuc < km.NgayBatDau)
        {
            throw new BusinessException("Ngày kết thúc phải sau ngày bắt đầu.");
        }

        bool isNew = km.Id == 0;
        if (isNew)
        {
            _db.KhuyenMais.Add(km);
            await _db.SaveChangesAsync();
            if (string.IsNullOrWhiteSpace(km.MaKm))
            {
                km.MaKm = $"KM{km.Id:D3}"; // sinh từ Id sau khi lưu (fix #13), tránh đụng mã khi đồng thời
            }
        }
        else
        {
            _db.KhuyenMais.Update(km);
        }
        await _db.SaveChangesAsync();
        return km;
    }

    public async Task DeleteAsync(int id)
    {
        var km = await _db.KhuyenMais.FindAsync(id)
            ?? throw new ResourceNotFoundException("Khuyến mãi", "Id", id);

        var soDon = await _db.DatVes.CountAsync(d => d.KhuyenMaiId == id);
        if (soDon > 0)
        {
            throw new BusinessException($"Không thể xóa khuyến mãi '{km.TenKm}' vì đã được áp dụng cho {soDon} đơn đặt vé. Hãy sửa ngày kết thúc để ngừng áp dụng.");
        }

        _db.KhuyenMais.Remove(km);
        await _db.SaveChangesAsync();
    }
}
