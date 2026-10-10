using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("GHE")]
public class Ghe
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaGhe { get; set; }

    [Required]
    public int PhongChieuId { get; set; }

    [ForeignKey(nameof(PhongChieuId))]
    public PhongChieu? PhongChieu { get; set; }

    [Required]
    [MaxLength(5)]
    public string HangGhe { get; set; } = string.Empty;

    [Required]
    public int SoGhe { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? LoaiGhe { get; set; } = "Thường";

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Hoạt động";

    public List<Ve> Ves { get; set; } = new();
}
