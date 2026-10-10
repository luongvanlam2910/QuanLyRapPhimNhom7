using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public interface IKhuyenMaiService
{
    Task<List<KhuyenMai>> FindAllAsync();
    Task<List<KhuyenMai>> FindHieuLucAsync(DateOnly? ngayHienTai = null);
    Task<KhuyenMai?> FindByIdAsync(int id);
    Task<KhuyenMai> SaveAsync(KhuyenMai km);
    Task DeleteAsync(int id);
}
