using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RapChieu> RapChieus => Set<RapChieu>();
    public DbSet<PhongChieu> PhongChieus => Set<PhongChieu>();
    public DbSet<Ghe> Ghes => Set<Ghe>();
    public DbSet<TheLoai> TheLoais => Set<TheLoai>();
    public DbSet<Phim> Phims => Set<Phim>();
    public DbSet<LichChieu> LichChieus => Set<LichChieu>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<PhongBan> PhongBans => Set<PhongBan>();
    public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
    public DbSet<DatVe> DatVes => Set<DatVe>();
    public DbSet<Ve> Ves => Set<Ve>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- RapChieu ----
        builder.Entity<RapChieu>(e =>
        {
            e.HasIndex(x => x.MaRap).IsUnique().HasFilter("[MaRap] IS NOT NULL");
            e.HasIndex(x => x.Sdt).IsUnique().HasFilter("[Sdt] IS NOT NULL");

            e.HasMany(x => x.PhongChieus)
                .WithOne(x => x.RapChieu)
                .HasForeignKey(x => x.RapChieuId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.NhanViens)
                .WithOne(x => x.RapChieu)
                .HasForeignKey(x => x.RapChieuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PhongChieu ----
        builder.Entity<PhongChieu>(e =>
        {
            e.HasIndex(x => x.MaPhong).IsUnique().HasFilter("[MaPhong] IS NOT NULL");

            e.HasMany(x => x.Ghes)
                .WithOne(x => x.PhongChieu)
                .HasForeignKey(x => x.PhongChieuId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.LichChieus)
                .WithOne(x => x.PhongChieu)
                .HasForeignKey(x => x.PhongChieuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Ghe ----
        builder.Entity<Ghe>(e =>
        {
            e.HasIndex(x => x.MaGhe).IsUnique().HasFilter("[MaGhe] IS NOT NULL");
            e.HasIndex(x => new { x.PhongChieuId, x.HangGhe, x.SoGhe }).IsUnique();

            e.HasMany(x => x.Ves)
                .WithOne(x => x.Ghe)
                .HasForeignKey(x => x.GheId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- TheLoai ----
        builder.Entity<TheLoai>(e =>
        {
            e.HasIndex(x => x.MaTheLoai).IsUnique().HasFilter("[MaTheLoai] IS NOT NULL");
            e.HasIndex(x => x.TenTheLoai).IsUnique();
        });

        // ---- Phim (many-to-many with TheLoai, implicit join table) ----
        builder.Entity<Phim>(e =>
        {
            e.HasIndex(x => x.MaPhim).IsUnique().HasFilter("[MaPhim] IS NOT NULL");

            e.HasMany(x => x.TheLoais)
                .WithMany(x => x.Phims)
                .UsingEntity(j => j.ToTable("PHIM_THE_LOAI"));

            e.HasMany(x => x.LichChieus)
                .WithOne(x => x.Phim)
                .HasForeignKey(x => x.PhimId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LichChieu ----
        builder.Entity<LichChieu>(e =>
        {
            e.HasIndex(x => x.MaLich).IsUnique().HasFilter("[MaLich] IS NOT NULL");

            e.HasMany(x => x.Ves)
                .WithOne(x => x.LichChieu)
                .HasForeignKey(x => x.LichChieuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- KhachHang ----
        builder.Entity<KhachHang>(e =>
        {
            e.HasIndex(x => x.MaKh).IsUnique().HasFilter("[MaKh] IS NOT NULL");
            e.HasIndex(x => x.Sdt).IsUnique();
            e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

            e.HasMany(x => x.DatVes)
                .WithOne(x => x.KhachHang)
                .HasForeignKey(x => x.KhachHangId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PhongBan ----
        builder.Entity<PhongBan>(e =>
        {
            e.HasIndex(x => x.MaPb).IsUnique().HasFilter("[MaPb] IS NOT NULL");
            e.HasIndex(x => x.TenPb).IsUnique();
        });

        // ---- NhanVien ----
        builder.Entity<NhanVien>(e =>
        {
            e.HasIndex(x => x.MaNv).IsUnique().HasFilter("[MaNv] IS NOT NULL");
            e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

            e.HasMany(x => x.DatVes)
                .WithOne(x => x.NhanVien)
                .HasForeignKey(x => x.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.PhongBan)
                .WithMany(x => x.NhanViens)
                .HasForeignKey(x => x.PhongBanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- KhuyenMai ----
        builder.Entity<KhuyenMai>(e =>
        {
            e.HasIndex(x => x.MaKm).IsUnique().HasFilter("[MaKm] IS NOT NULL");

            e.HasMany(x => x.DatVes)
                .WithOne(x => x.KhuyenMai)
                .HasForeignKey(x => x.KhuyenMaiId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- DatVe ----
        builder.Entity<DatVe>(e =>
        {
            e.HasIndex(x => x.MaDat).IsUnique().HasFilter("[MaDat] IS NOT NULL");
            e.Property(x => x.TongTien).HasDefaultValue(0m);

            e.HasMany(x => x.Ves)
                .WithOne(x => x.DatVe)
                .HasForeignKey(x => x.DatVeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.ThanhToans)
                .WithOne(x => x.DatVe)
                .HasForeignKey(x => x.DatVeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Ve ----
        builder.Entity<Ve>(e =>
        {
            e.HasIndex(x => x.MaVe).IsUnique().HasFilter("[MaVe] IS NOT NULL");

            // Chỉ chặn trùng ghế cho những vé CHƯA hủy — vé đã hủy không được tính,
            // để ghế hủy có thể đặt lại cho cùng suất chiếu (sửa lỗi "ghế đã hủy không bao giờ đặt lại được").
            e.HasIndex(x => new { x.LichChieuId, x.GheId })
                .IsUnique()
                .HasFilter("[TrangThai] <> N'Đã hủy'");

            e.HasOne(x => x.NhanVienSoatVe)
                .WithMany()
                .HasForeignKey(x => x.NhanVienSoatVeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ThanhToan ----
        builder.Entity<ThanhToan>(e =>
        {
            e.HasIndex(x => x.MaTt).IsUnique().HasFilter("[MaTt] IS NOT NULL");
        });
    }
}
