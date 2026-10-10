using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public interface IKiemTraDoTuoiService
{
    /// <summary>
    /// Kiểm tra khách hàng có đủ điều kiện độ tuổi để đặt vé cho một suất chiếu hay không.
    /// Ném BusinessException với thông báo cụ thể nếu không đủ điều kiện (phim hạn chế tuổi, chưa khai ngày sinh, suất quá khuya cho trẻ em...).
    /// </summary>
    void KiemTra(Phim phim, LichChieu lichChieu, DateOnly? ngaySinhKhachHang);
}
