using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("KHUYEN_MAI")]
public class KhuyenMai
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaKm { get; set; }

    [Required]
    [MaxLength(120)]
    [Column(TypeName = "nvarchar(120)")]
    public string TenKm { get; set; } = string.Empty;

    // 'PERCENT' or 'AMOUNT'
    [Required]
    [MaxLength(20)]
    public string LoaiGiam { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal GiaTri { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public List<DatVe> DatVes { get; set; } = new();
}
