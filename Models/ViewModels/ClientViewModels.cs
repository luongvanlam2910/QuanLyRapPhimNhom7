using System.ComponentModel.DataAnnotations;
using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Models.ViewModels;

public class HomeIndexViewModel
{
    public List<Phim> PhimDangChieu { get; set; } = new();
    public List<Phim> PhimSapChieu { get; set; } = new();
    public List<RapChieu> DanhSachRap { get; set; } = new();
    public List<TheLoai> DanhSachTheLoai { get; set; } = new();
}

public class TimKiemViewModel
{
    public PhimSearchResult? KetQua { get; set; }
    public List<TheLoai> DanhSachTheLoai { get; set; } = new();
    public string? TenPhimSearch { get; set; }
    public int? MaTheLoaiSearch { get; set; }
    public string? TrangThaiSearch { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Sdt { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
    [DataType(DataType.Date)]
    public DateOnly NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [DataType(DataType.Password)]
    public string XacNhanMatKhau { get; set; } = string.Empty;
}

public class TaiKhoanViewModel
{
    public KhachHang? KhachHang { get; set; }
}

public class PhimDetailViewModel
{
    public Phim Phim { get; set; } = null!;
    public List<RapChieu> DanhSachRap { get; set; } = new();
}

public class ChonGheViewModel
{
    public LichChieu LichChieu { get; set; } = null!;
    public List<SeatStatusDto> DanhSachGhe { get; set; } = new();
    public List<KhuyenMai> KhuyenMais { get; set; } = new();
    public int SeatHoldMinutes { get; set; } = 10;
}

public class GheChonDto
{
    public int GheId { get; set; }
    public string HangGhe { get; set; } = string.Empty;
    public int SoGhe { get; set; }
    public string? LoaiGhe { get; set; }
}

public class XacNhanViewModel
{
    public LichChieu LichChieu { get; set; } = null!;
    public List<GheChonDto> GheDaChon { get; set; } = new();
    public List<int> MaGhes { get; set; } = new();
    public List<KhuyenMai> KhuyenMais { get; set; } = new();
    public int? MaKmChon { get; set; }
}

public class LichSuViewModel
{
    public List<DatVe> DanhSachDatVe { get; set; } = new();
    public string? TrangThaiFilter { get; set; }
}
