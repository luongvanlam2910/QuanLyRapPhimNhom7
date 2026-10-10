using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("THANH_TOAN")]
public class ThanhToan
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaTt { get; set; }

    [Required]
    public int DatVeId { get; set; }

    [ForeignKey(nameof(DatVeId))]
    public DatVe? DatVe { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal SoTien { get; set; }

    public DateTime NgayTt { get; set; } = DateTime.Now;

    // 'Ví điện tử', 'Tiền mặt', 'Thẻ ngân hàng', 'Chuyển khoản'
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string? PhuongThuc { get; set; }

    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Thành công";
}
