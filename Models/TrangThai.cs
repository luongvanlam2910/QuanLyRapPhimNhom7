namespace DatVeXemPhim.Models;

/// <summary>
/// Hằng số trạng thái dùng chung, thay cho so sánh chuỗi rải rác / LIKE dễ sai dấu.
/// </summary>
public static class TrangThaiPhim
{
    public const string DangChieu = "Đang chiếu";
    public const string SapChieu = "Sắp chiếu";
    public const string NgungChieu = "Ngừng chiếu";
}

/// <summary>
/// Phân loại phim theo độ tuổi (Thông tư 05/2023/TT-BVHTTDL):
/// P = phổ biến mọi lứa tuổi; K = phổ biến với khán giả dưới 13 tuổi (cần người giám hộ đi cùng);
/// T13/T16/T18 = chỉ phổ biến với khán giả từ đủ 13/16/18 tuổi trở lên.
/// </summary>
public static class DoTuoiPhim
{
    public const string P = "P";
    public const string K = "K";
    public const string T13 = "T13";
    public const string T16 = "T16";
    public const string T18 = "T18";

    public static readonly string[] All = { P, K, T13, T16, T18 };

    /// <summary>Độ tuổi tối thiểu bắt buộc, null nếu không giới hạn (P) hoặc chỉ cần giám hộ (K).</summary>
    public static int? DoTuoiToiThieu(string? doTuoi) => doTuoi switch
    {
        T13 => 13,
        T16 => 16,
        T18 => 18,
        _ => null
    };

    public static string MoTa(string? doTuoi) => doTuoi switch
    {
        P => "P - Phổ biến mọi lứa tuổi",
        K => "K - Dưới 13 tuổi cần người giám hộ đi cùng",
        T13 => "T13 - Từ 13 tuổi trở lên",
        T16 => "T16 - Từ 16 tuổi trở lên",
        T18 => "T18 - Từ 18 tuổi trở lên",
        _ => doTuoi ?? ""
    };
}

public static class TrangThaiLichChieu
{
    public const string MoBan = "Mở bán";
    public const string DaHuy = "Đã hủy";
    public const string DaKetThuc = "Đã kết thúc";
}

public static class TrangThaiDatVe
{
    public const string ChoThanhToan = "Chờ thanh toán";
    public const string DaThanhToan = "Đã thanh toán";
    public const string DaHuy = "Đã hủy";
}

public static class TrangThaiVe
{
    public const string DaDat = "Đã đặt";
    public const string DaHuy = "Đã hủy";
    public const string DaSuDung = "Đã sử dụng";
}

public static class TrangThaiGhe
{
    public const string HoatDong = "Hoạt động";
    public const string BaoTri = "Bảo trì";
}

public static class TrangThaiNhanVien
{
    public const string HoatDong = "Hoạt động";
    public const string DaKhoa = "Đã khóa";
}

public static class TrangThaiKhachHang
{
    public const string HoatDong = "Hoạt động";
    public const string DaKhoa = "Đã khóa";
}

public static class TrangThaiThanhToan
{
    public const string ThanhCong = "Thành công";
    public const string ThatBai = "Thất bại";
    public const string HoanTien = "Hoàn tiền";
}

/// <summary>
/// Danh sách role hợp lệ trong hệ thống (whitelist khi tạo/sửa nhân viên).
/// </summary>
public static class AppRoles
{
    public const string KhachHang = "KhachHang";
    public const string NhanVien = "NhanVien";
    public const string QuanLy = "QuanLy";
    // Quản trị toàn hệ thống (mọi rạp) — khác với QuanLy chỉ quản lý rạp mình phụ trách.
    public const string Admin = "Admin";

    public static readonly string[] AllRoles = { KhachHang, NhanVien, QuanLy, Admin };
    public static readonly string[] NhanVienRoles = { NhanVien, QuanLy };

    public static bool IsValidNhanVienRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && NhanVienRoles.Contains(role);
}
