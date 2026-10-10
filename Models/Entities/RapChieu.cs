using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("RAP_CHIEU")]
public class RapChieu
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaRap { get; set; }

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string TenRap { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string DiaChi { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Sdt { get; set; }

    public List<PhongChieu> PhongChieus { get; set; } = new();
    public List<NhanVien> NhanViens { get; set; } = new();
}
