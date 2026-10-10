using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("PHIM")]
public class Phim
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaPhim { get; set; }

    [Required]
    [MaxLength(150)]
    [Column(TypeName = "nvarchar(150)")]
    public string TenPhim { get; set; } = string.Empty;

    [Required]
    public int ThoiLuong { get; set; }

    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string? DaoDien { get; set; }

    [MaxLength(10)]
    public string? DoTuoi { get; set; }

    public DateOnly? NgayKhoiChieu { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Đang chiếu";

    [MaxLength(200)]
    public string? AnhPoster { get; set; }

    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(2000)")]
    public string? MoTa { get; set; }

    public List<TheLoai> TheLoais { get; set; } = new();
    public List<LichChieu> LichChieus { get; set; } = new();
}
