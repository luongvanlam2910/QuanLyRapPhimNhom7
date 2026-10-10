using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatVeXemPhim.Models.Entities;

[Table("KHACH_HANG")]
public class KhachHang
{
    [Key]
    public int Id { get; set; }

    [MaxLength(10)]
    public string? MaKh { get; set; }

    // Links to ASP.NET Core Identity user (1-1), nullable for legacy/offline customers
    public string? UserId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string HoTen { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string Sdt { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    // Dùng để kiểm tra độ tuổi khi đặt vé phim gắn nhãn K/T13/T16/T18.
    public DateOnly? NgaySinh { get; set; }

    public DateOnly? NgayDangKy { get; set; }

    // 'Hoạt động', 'Đã khóa' - khóa thay vì xóa cứng (còn FK từ DAT_VE)
    [MaxLength(30)]
    [Column(TypeName = "nvarchar(30)")]
    public string? TrangThai { get; set; } = "Hoạt động";

    public List<DatVe> DatVes { get; set; } = new();
}
