using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("VE")]
public class Ve
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaVe { get; set; }

    [Required]
    public int DatVeId { get; set; }

    [ForeignKey(nameof(DatVeId))]
    public DatVe? DatVe { get; set; }

    [Required]
    public int LichChieuId { get; set; }

    [ForeignKey(nameof(LichChieuId))]
    public LichChieu? LichChieu { get; set; }

    [Required]
    public int GheId { get; set; }

    [ForeignKey(nameof(GheId))]
    public Ghe? Ghe { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal GiaVe { get; set; }

    // 'Đã đặt', 'Đã hủy', 'Đã sử dụng'
    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Đã đặt";

    public DateTime? ThoiGianSoatVe { get; set; }

    public int? NhanVienSoatVeId { get; set; }

    [ForeignKey(nameof(NhanVienSoatVeId))]
    public NhanVien? NhanVienSoatVe { get; set; }
}
