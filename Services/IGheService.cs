using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;

namespace DatVeXemPhim.Services;

public interface IGheService
{
    Task<List<Ghe>> FindByPhongAsync(int phongChieuId);
    Task<Ghe> FindByIdAsync(int gheId);
    Task<List<SeatStatusDto>> GetTrangThaiGheForLichAsync(int lichChieuId, string? sessionOrUserKey);
    Task GiuGheTamAsync(int lichChieuId, List<int> gheIds, string sessionOrUserKey);
    Task GiaiPhongGiuTamAsync(int lichChieuId, string sessionOrUserKey);
    Task GiaiPhongGheAsync(int lichChieuId, IEnumerable<int> gheIds, string sessionOrUserKey);
    Task<Ghe> SaveAsync(Ghe ghe);
    Task<int> CountByPhongAsync(int phongChieuId);
    Task GenerateGheForPhongAsync(PhongChieu phongChieu);
}
