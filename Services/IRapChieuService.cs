using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public interface IRapChieuService
{
    Task<List<RapChieu>> FindAllAsync();
    Task<RapChieu?> FindByIdAsync(int id);
    Task<RapChieu> GetByIdAsync(int id);
    Task<RapChieu> SaveAsync(RapChieu rapChieu);
    Task DeleteAsync(int id);

    Task<List<PhongChieu>> FindPhongByRapAsync(int rapChieuId);
    Task<PhongChieu> SavePhongAsync(PhongChieu phong);
    Task<PhongChieu?> FindPhongByIdAsync(int id);
    Task DeletePhongAsync(int id);
}
