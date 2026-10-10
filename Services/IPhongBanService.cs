using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public interface IPhongBanService
{
    Task<List<PhongBan>> FindAllAsync();
    Task<PhongBan?> FindByIdAsync(int id);
    Task<PhongBan> SaveAsync(PhongBan phongBan);
    Task DeleteAsync(int id);
}
