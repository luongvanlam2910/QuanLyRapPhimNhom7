using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("NHAN_VIEN")]
public class NhanVien
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaNv { get; set; }

    // Links to ASP.NET Core Identity user (1-1)
    public string? UserId { get; set; }

    [Required]
    public int RapChieuId { get; set; }

    [ForeignKey(nameof(RapChieuId))]
    public RapChieu? RapChieu { get; set; }

    public int? PhongBanId { get; set; }

    [ForeignKey(nameof(PhongBanId))]
    public PhongBan? PhongBan { get; set; }

    public DateOnly? NgaySinh { get; set; }

    [MaxLength(20)]
    public string? Cccd { get; set; }

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string HoTen { get; set; } = string.Empty;

    // 'NHAN_VIEN' or 'QUAN_LY' - maps to Identity role
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string? ChucVu { get; set; }

    [MaxLength(15)]
    public string? Sdt { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Hoạt động";

    public List<DatVe> DatVes { get; set; } = new();
}
