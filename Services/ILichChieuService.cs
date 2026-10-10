using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public interface ILichChieuService
{
    Task<List<LichChieu>> FindAllAsync();
    Task<LichChieu?> FindByIdAsync(int id);
    Task<LichChieu> GetByIdAsync(int id);
    Task<List<LichChieu>> FindByPhimRapNgayAsync(int phimId, int rapChieuId, DateTime ngay);
    Task<List<LichChieu>> FindByPhimAsync(int phimId);
    Task<LichChieu> SaveAsync(LichChieu lichChieu);
    Task DeleteAsync(int id);
    Task UpdateTrangThaiAsync(int id, string trangThai);
    Task<LichChieu> SuaAsync(int id, DateTime batDau, DateTime ketThuc, decimal giaVe);
}
