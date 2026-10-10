namespace DatVeXemPhim.Models.ViewModels;

// DTO cho các API công khai (RESTful) — tách khỏi entity để tránh lộ dữ liệu nội bộ
// và tránh vòng lặp tham chiếu khi serialize JSON.

public record PhimDto(
    int Id, string? MaPhim, string TenPhim, int ThoiLuong, string? DaoDien,
    string? DoTuoi, DateOnly? NgayKhoiChieu, string? TrangThai, string? AnhPoster,
    string? MoTa, List<string> TheLoais);

public record TheLoaiDto(int Id, string? MaTheLoai, string TenTheLoai);

public record RapChieuDto(int Id, string? MaRap, string TenRap, string DiaChi, string? Sdt, int SoPhong);

public record PhongChieuDto(int Id, string? MaPhong, string TenPhong, string? LoaiPhong, int SucChua, int RapChieuId);

public record LichChieuDto(
    int Id, string? MaLich, int PhimId, string? TenPhim, int PhongChieuId, string? TenPhong,
    int RapChieuId, string? TenRap, DateTime ThoiGianBatDau, DateTime ThoiGianKetThuc,
    decimal GiaVe, string? TrangThai);

public record GheTrangThaiDto(int GheId, string? MaGhe, string HangGhe, int SoGhe, string? LoaiGhe, string TrangThai);

public record ApiErrorDto(string Message);
