namespace DatVeXemPhim.Services;

/// <summary>
/// Xác định phạm vi rạp của người dùng admin hiện tại.
/// Trả về null = không giới hạn (role Admin, xem/quản lý được mọi rạp).
/// Trả về giá trị = chỉ được thao tác trên rạp đó (role QuanLy/NhanVien gắn với 1 rạp cụ thể).
/// </summary>
public interface ICurrentStaffService
{
    Task<int?> GetRapChieuIdAsync();

    /// <summary>Id hồ sơ nhân viên của người đăng nhập (null nếu là Admin không gắn hồ sơ hoặc chưa có hồ sơ).</summary>
    Task<int?> GetNhanVienIdAsync();

    /// <summary>true nếu là Quản lý hoặc Admin. Nhân viên thường (NhanVien) chỉ được xem/làm việc của chính mình.</summary>
    bool LaQuanLyTroLen { get; }
}
