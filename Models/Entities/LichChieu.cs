using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("LICH_CHIEU")]
public class LichChieu
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaLich { get; set; }

    [Required]
    public int PhimId { get; set; }

    [ForeignKey(nameof(PhimId))]
    public Phim? Phim { get; set; }

    [Required]
    public int PhongChieuId { get; set; }

    [ForeignKey(nameof(PhongChieuId))]
    public PhongChieu? PhongChieu { get; set; }

    [Required]
    public DateTime ThoiGianBatDau { get; set; }

    [Required]
    public DateTime ThoiGianKetThuc { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal GiaVe { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Mở bán";

    public List<Ve> Ves { get; set; } = new();
}
