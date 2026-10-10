using DatVeXemPhim.Models.Entities;

namespace DatVeXemPhim.Models.ViewModels;

public class SeatStatusDto
{
    public int GheId { get; set; }
    public string? MaGhe { get; set; }
    public string HangGhe { get; set; } = string.Empty;
    public int SoGhe { get; set; }
    public string? LoaiGhe { get; set; }
    // "TRONG", "DA_DAT", "GIU_TAM", "BAO_TRI"
    public string TrangThai { get; set; } = "TRONG";
}

public class DatVeRequest
{
    public int LichChieuId { get; set; }
    public List<int> GheIds { get; set; } = new();
    public int? KhuyenMaiId { get; set; }
    public string? PhuongThucThanhToan { get; set; }

    // Dùng khi nhân viên bán vé tại quầy cho khách không có tài khoản.
    public string? TenKhachVangLai { get; set; }
    public string? SdtVangLai { get; set; }
}

public class GheInfoDto
{
    public string? MaVe { get; set; }
    public string HangGhe { get; set; } = string.Empty;
    public int SoGhe { get; set; }
    public string? LoaiGhe { get; set; }
    public decimal GiaVe { get; set; }
}

public class DatVeResponse
{
    public int DatVeId { get; set; }
    public string? MaDat { get; set; }
    public string TenPhim { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public string TenRap { get; set; } = string.Empty;
    public DateTime ThoiGianChieu { get; set; }
    public List<GheInfoDto> GheDaDat { get; set; } = new();
    public decimal TongTienGoc { get; set; }
    public decimal SoTienGiam { get; set; }
    public decimal TongTienSauGiam { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? MaTt { get; set; }
}

public class PhimSearchResult
{
    public List<Phim> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages => Size == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Size);
}

public class DoanhThuPhimDto
{
    public int PhimId { get; set; }
    public string TenPhim { get; set; } = string.Empty;
    public decimal TongDoanhThu { get; set; }
    public int SoVeBan { get; set; }
}

public class DoanhThuRapDto
{
    public int RapChieuId { get; set; }
    public string TenRap { get; set; } = string.Empty;
    public decimal TongDoanhThu { get; set; }
    public int SoVeBan { get; set; }
}

public class DoanhThuPhuongThucDto
{
    public string PhuongThuc { get; set; } = string.Empty;
    public decimal TongTien { get; set; }
    public int SoGiaoDich { get; set; }
}

public class SoatVeResultDto
{
    public bool HopLe { get; set; }
    public string ThongBao { get; set; } = string.Empty;

    public string? MaVe { get; set; }
    public string? TenPhim { get; set; }
    public string? TenRap { get; set; }
    public string? TenPhong { get; set; }
    public string? HangGhe { get; set; }
    public int? SoGhe { get; set; }
    public string? LoaiGhe { get; set; }
    public string? DoTuoiPhim { get; set; }
    public DateTime? ThoiGianChieu { get; set; }
    public string? TrangThaiVe { get; set; }
    public DateTime? ThoiGianSoatVe { get; set; }
}

public class DashboardStatsDto
{
    public long TongVeBan { get; set; }
    public decimal TongDoanhThu { get; set; }
    public long SoSuatMoBan { get; set; }
    public long DaDatVe { get; set; }
    public long ChoThanhToan { get; set; }
    public long TongPhim { get; set; }
    public long TongKhachHang { get; set; }
}
