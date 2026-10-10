using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;

namespace DatVeXemPhim.Services;

public interface IDatVeService
{
    Task<List<DatVe>> FindByKhachHangAsync(int khachHangId);
    Task<DatVe?> FindByIdAsync(int id);
    Task<DatVeResponse> DatVeAsync(DatVeRequest request, int? khachHangId, int? nhanVienId, string? userKey = null, bool xacNhanThanhToanNgay = true);
    Task<DatVeResponse> XacNhanThanhToanAsync(string maDat, bool thanhCong, string? phuongThucThanhToan);
    Task<int> HuyCacDonQuaHanAsync();
    Task HuyDatVeAsync(int datVeId, int khachHangId);
    Task HuyDatVeByStaffAsync(int datVeId);
    Task DoiTrangThaiByStaffAsync(int datVeId, string trangThaiMoi);
}
