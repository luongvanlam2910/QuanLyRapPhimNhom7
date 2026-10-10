using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("PHONG_CHIEU")]
public class PhongChieu
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaPhong { get; set; }

    [Required]
    public int RapChieuId { get; set; }

    [ForeignKey(nameof(RapChieuId))]
    public RapChieu? RapChieu { get; set; }

    [Required]
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string TenPhong { get; set; } = string.Empty;

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? LoaiPhong { get; set; }

    [Required]
    public int SucChua { get; set; }

    public List<Ghe> Ghes { get; set; } = new();
    public List<LichChieu> LichChieus { get; set; } = new();
}
