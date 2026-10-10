using DatVeXemPhim.Models.ViewModels;

namespace DatVeXemPhim.Services;

public interface IBaoCaoService
{
    /// <param name="rapChieuId">null = toàn hệ thống (Admin); có giá trị = chỉ tính riêng rạp đó (QuanLy).</param>
    Task<DashboardStatsDto> GetDashboardStatsAsync(int? rapChieuId = null);
    Task<List<DoanhThuPhimDto>> GetDoanhThuTheoPhimAsync(int? rapChieuId = null);
    Task<List<DoanhThuRapDto>> GetDoanhThuTheoRapAsync(int? rapChieuId = null);
    Task<List<DoanhThuPhuongThucDto>> GetDoanhThuTheoPhuongThucAsync(int? rapChieuId = null);
    Task<decimal> GetDoanhThuSuatAsync(int lichChieuId);
}
