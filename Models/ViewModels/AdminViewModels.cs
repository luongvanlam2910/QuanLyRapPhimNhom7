using System.ComponentModel.DataAnnotations;

namespace DatVeXemPhim.Models.ViewModels;

public class PhimFormDTO
{
    public int? MaPhim { get; set; }

    [Required(ErrorMessage = "Tên phim không được để trống")]
    [MaxLength(150, ErrorMessage = "Tên phim không được vượt quá 150 ký tự")]
    public string TenPhim { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập thời lượng phim")]
    [Range(1, 1000, ErrorMessage = "Thời lượng không hợp lệ")]
    public int ThoiLuong { get; set; }

    [MaxLength(100, ErrorMessage = "Tên đạo diễn không được vượt quá 100 ký tự")]
    public string? DaoDien { get; set; }

    public string? DoTuoi { get; set; }

    public string? NgayKhoiChieu { get; set; }

    public string? TrangThai { get; set; } = "dang-chieu";

    public string? AnhPoster { get; set; }

    [MaxLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    public string? MoTa { get; set; }

    public List<string> MaTheLoais { get; set; } = new();
}

public class LichChieuFormDTO
{
    public int MaPhim { get; set; }
    public int MaPhong { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public decimal GiaVe { get; set; }
    public string TrangThai { get; set; } = "Mở bán";
}

public class PhongChieuJsonDto
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public string? LoaiPhong { get; set; }
    public int SucChua { get; set; }
}

public class NhanVienFormDTO
{
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? Sdt { get; set; }
    public string? ChucVu { get; set; }
    public int MaRap { get; set; }
    public string? MatKhau { get; set; }
}

public class NhanVienEditDTO
{
    public int MaNv { get; set; }
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? Sdt { get; set; }
    public string? ChucVu { get; set; }
    public int MaRap { get; set; }
    public string? MatKhauMoi { get; set; }
}

public class RapChieuFormDTO
{
    public string TenRap { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? Sdt { get; set; }
}

public class PhongChieuFormDTO
{
    public int MaRap { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public string? LoaiPhong { get; set; }
    public int SucChua { get; set; }
}

