using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("DAT_VE")]
public class DatVe
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaDat { get; set; }

    // Null khi là khách vãng lai do nhân viên bán tại quầy (xem TenKhachVangLai/SdtVangLai)
    public int? KhachHangId { get; set; }

    [ForeignKey(nameof(KhachHangId))]
    public KhachHang? KhachHang { get; set; }

    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string? TenKhachVangLai { get; set; }

    [MaxLength(15)]
    public string? SdtVangLai { get; set; }

    // Online booking may have no staff
    public int? NhanVienId { get; set; }

    [ForeignKey(nameof(NhanVienId))]
    public NhanVien? NhanVien { get; set; }

    public int? KhuyenMaiId { get; set; }

    [ForeignKey(nameof(KhuyenMaiId))]
    public KhuyenMai? KhuyenMai { get; set; }

    public DateTime NgayDat { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(12,2)")]
    public decimal TongTien { get; set; } = 0;

    // 'Chờ thanh toán', 'Đã thanh toán', 'Đã hủy'
    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Chờ thanh toán";

    // Đơn "Chờ thanh toán" quá hạn này sẽ bị hệ thống tự hủy (xem HuyDonQuaHanService).
    public DateTime? HanThanhToan { get; set; }

    public List<Ve> Ves { get; set; } = new();
    public List<ThanhToan> ThanhToans { get; set; } = new();
}
