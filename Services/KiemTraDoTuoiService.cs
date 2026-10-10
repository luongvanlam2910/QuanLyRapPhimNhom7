using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Services;

public class KiemTraDoTuoiService : IKiemTraDoTuoiService
{
    public void KiemTra(Phim phim, LichChieu lichChieu, DateOnly? ngaySinhKhachHang)
    {
        var doTuoiToiThieu = DoTuoiPhim.DoTuoiToiThieu(phim.DoTuoi);
        var ngayChieu = DateOnly.FromDateTime(lichChieu.ThoiGianBatDau);

        if (doTuoiToiThieu.HasValue)
        {
            if (ngaySinhKhachHang == null)
            {
                throw new BusinessException(
                    $"Phim '{phim.TenPhim}' giới hạn độ tuổi {phim.DoTuoi} ({DoTuoiPhim.MoTa(phim.DoTuoi)}). " +
                    "Vui lòng khai báo ngày sinh trong trang Tài khoản trước khi đặt vé.");
            }

            var tuoi = TinhTuoi(ngaySinhKhachHang.Value, ngayChieu);
            if (tuoi < doTuoiToiThieu.Value)
            {
                throw new BusinessException(
                    $"Phim '{phim.TenPhim}' chỉ phổ biến cho khán giả từ {doTuoiToiThieu.Value} tuổi trở lên ({phim.DoTuoi}). " +
                    "Bạn chưa đủ điều kiện độ tuổi để đặt vé suất chiếu này.");
            }
        }

        // Giờ chiếu khuya: hạn chế khán giả nhỏ tuổi dù phim không giới hạn tuổi quá cao (K/T13),
        // để phù hợp thực tế rạp chiếu (trẻ dưới 13 tuổi không xem suất kết thúc sau 22h,
        // dưới 16 tuổi không xem suất kết thúc sau 23h).
        if (ngaySinhKhachHang.HasValue)
        {
            var tuoi = TinhTuoi(ngaySinhKhachHang.Value, ngayChieu);
            var gioKetThuc = lichChieu.ThoiGianKetThuc.TimeOfDay;

            if (tuoi < 13 && gioKetThuc > new TimeSpan(22, 0, 0))
            {
                throw new BusinessException("Khán giả dưới 13 tuổi không được xem suất chiếu kết thúc sau 22:00.");
            }
            if (tuoi < 16 && gioKetThuc > new TimeSpan(23, 0, 0))
            {
                throw new BusinessException("Khán giả dưới 16 tuổi không được xem suất chiếu kết thúc sau 23:00.");
            }
        }
    }

    private static int TinhTuoi(DateOnly ngaySinh, DateOnly tinhTaiNgay)
    {
        var tuoi = tinhTaiNgay.Year - ngaySinh.Year;
        if (tinhTaiNgay < ngaySinh.AddYears(tuoi)) tuoi--;
        return tuoi;
    }
}
