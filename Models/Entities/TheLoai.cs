using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("THE_LOAI")]
public class TheLoai
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaTheLoai { get; set; }

    [Required]
    [MaxLength(80)]
    [Column(TypeName = "nvarchar(80)")]
    public string TenTheLoai { get; set; } = string.Empty;

    public List<Phim> Phims { get; set; } = new();
}
