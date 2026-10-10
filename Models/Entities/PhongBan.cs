using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("PHONG_BAN")]
public class PhongBan
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaPb { get; set; }

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string TenPb { get; set; } = string.Empty;

    [MaxLength(300)]
    [Column(TypeName = "nvarchar(300)")]
    public string? MoTa { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Hoạt động";

    public List<NhanVien> NhanViens { get; set; } = new();
}
