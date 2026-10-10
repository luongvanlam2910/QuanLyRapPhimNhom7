using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace DatVeXemPhim.Services;

public interface IPhimService
{
    Task<List<Phim>> FindAllAsync();
    Task<List<Phim>> FindDangChieuAsync();
    Task<List<Phim>> FindSapChieuAsync();
    Task<Phim?> FindByIdAsync(int id);
    Task<Phim> GetByIdAsync(int id);
    Task<PhimSearchResult> SearchAsync(string? tenPhim, int? theLoaiId, string? trangThai, int page, int size);
    Task<Phim> SaveAsync(Phim phim, List<int>? theLoaiIds, IFormFile? posterFile);
    Task DeleteAsync(int id);
    Task<List<TheLoai>> FindAllTheLoaiAsync();
    Task<TheLoai?> FindTheLoaiByIdAsync(int id);
    Task<TheLoai> SaveTheLoaiAsync(TheLoai theLoai);
    Task DeleteTheLoaiAsync(int id);
}
