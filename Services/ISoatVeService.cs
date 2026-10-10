using DatVeXemPhim.Models.ViewModels;

namespace DatVeXemPhim.Services;

public interface ISoatVeService
{
    /// <summary>
    /// Tra cứu vé theo mã vé, phục vụ hiển thị cho nhân viên soát vé trước khi xác nhận.
    /// </summary>
    Task<SoatVeResultDto> TraCuuAsync(string maVe, int? rapChieuIdGioiHan);

    /// <summary>
    /// Xác nhận soát vé (khách đã vào rạp). Kiểm tra lại toàn bộ điều kiện trước khi đổi trạng thái.
    /// </summary>
    Task<SoatVeResultDto> XacNhanSoatAsync(string maVe, int nhanVienId, int? rapChieuIdGioiHan);
}
