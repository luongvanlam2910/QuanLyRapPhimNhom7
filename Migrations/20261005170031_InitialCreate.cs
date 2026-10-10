using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatVeXemPhim.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KHACH_HANG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayDangKy = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHACH_HANG", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KHUYEN_MAI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenKm = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LoaiGiam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHUYEN_MAI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PHIM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhim = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenPhim = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ThoiLuong = table.Column<int>(type: "int", nullable: false),
                    DaoDien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoTuoi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NgayKhoiChieu = table.Column<DateOnly>(type: "date", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AnhPoster = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RAP_CHIEU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaRap = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenRap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RAP_CHIEU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "THE_LOAI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTheLoai = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenTheLoai = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THE_LOAI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NHAN_VIEN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNv = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RapChieuId = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHAN_VIEN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NHAN_VIEN_RAP_CHIEU_RapChieuId",
                        column: x => x.RapChieuId,
                        principalTable: "RAP_CHIEU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PHONG_CHIEU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RapChieuId = table.Column<int>(type: "int", nullable: false),
                    TenPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoaiPhong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SucChua = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHONG_CHIEU", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PHONG_CHIEU_RAP_CHIEU_RapChieuId",
                        column: x => x.RapChieuId,
                        principalTable: "RAP_CHIEU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PHIM_THE_LOAI",
                columns: table => new
                {
                    PhimsId = table.Column<int>(type: "int", nullable: false),
                    TheLoaisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIM_THE_LOAI", x => new { x.PhimsId, x.TheLoaisId });
                    table.ForeignKey(
                        name: "FK_PHIM_THE_LOAI_PHIM_PhimsId",
                        column: x => x.PhimsId,
                        principalTable: "PHIM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PHIM_THE_LOAI_THE_LOAI_TheLoaisId",
                        column: x => x.TheLoaisId,
                        principalTable: "THE_LOAI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DAT_VE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    KhachHangId = table.Column<int>(type: "int", nullable: false),
                    NhanVienId = table.Column<int>(type: "int", nullable: true),
                    KhuyenMaiId = table.Column<int>(type: "int", nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DAT_VE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DAT_VE_KHACH_HANG_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KHACH_HANG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DAT_VE_KHUYEN_MAI_KhuyenMaiId",
                        column: x => x.KhuyenMaiId,
                        principalTable: "KHUYEN_MAI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DAT_VE_NHAN_VIEN_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NHAN_VIEN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GHE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhongChieuId = table.Column<int>(type: "int", nullable: false),
                    HangGhe = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SoGhe = table.Column<int>(type: "int", nullable: false),
                    LoaiGhe = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GHE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GHE_PHONG_CHIEU_PhongChieuId",
                        column: x => x.PhongChieuId,
                        principalTable: "PHONG_CHIEU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LICH_CHIEU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLich = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhimId = table.Column<int>(type: "int", nullable: false),
                    PhongChieuId = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiaVe = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LICH_CHIEU", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LICH_CHIEU_PHIM_PhimId",
                        column: x => x.PhimId,
                        principalTable: "PHIM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LICH_CHIEU_PHONG_CHIEU_PhongChieuId",
                        column: x => x.PhongChieuId,
                        principalTable: "PHONG_CHIEU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "THANH_TOAN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DatVeId = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NgayTt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THANH_TOAN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_THANH_TOAN_DAT_VE_DatVeId",
                        column: x => x.DatVeId,
                        principalTable: "DAT_VE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DatVeId = table.Column<int>(type: "int", nullable: false),
                    LichChieuId = table.Column<int>(type: "int", nullable: false),
                    GheId = table.Column<int>(type: "int", nullable: false),
                    GiaVe = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VE_DAT_VE_DatVeId",
                        column: x => x.DatVeId,
                        principalTable: "DAT_VE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VE_GHE_GheId",
                        column: x => x.GheId,
                        principalTable: "GHE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VE_LICH_CHIEU_LichChieuId",
                        column: x => x.LichChieuId,
                        principalTable: "LICH_CHIEU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DAT_VE_KhachHangId",
                table: "DAT_VE",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DAT_VE_KhuyenMaiId",
                table: "DAT_VE",
                column: "KhuyenMaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DAT_VE_MaDat",
                table: "DAT_VE",
                column: "MaDat",
                unique: true,
                filter: "[MaDat] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DAT_VE_NhanVienId",
                table: "DAT_VE",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_GHE_MaGhe",
                table: "GHE",
                column: "MaGhe",
                unique: true,
                filter: "[MaGhe] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GHE_PhongChieuId_HangGhe_SoGhe",
                table: "GHE",
                columns: new[] { "PhongChieuId", "HangGhe", "SoGhe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KHACH_HANG_Email",
                table: "KHACH_HANG",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KHACH_HANG_MaKh",
                table: "KHACH_HANG",
                column: "MaKh",
                unique: true,
                filter: "[MaKh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KHACH_HANG_Sdt",
                table: "KHACH_HANG",
                column: "Sdt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KHUYEN_MAI_MaKm",
                table: "KHUYEN_MAI",
                column: "MaKm",
                unique: true,
                filter: "[MaKm] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LICH_CHIEU_MaLich",
                table: "LICH_CHIEU",
                column: "MaLich",
                unique: true,
                filter: "[MaLich] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LICH_CHIEU_PhimId",
                table: "LICH_CHIEU",
                column: "PhimId");

            migrationBuilder.CreateIndex(
                name: "IX_LICH_CHIEU_PhongChieuId",
                table: "LICH_CHIEU",
                column: "PhongChieuId");

            migrationBuilder.CreateIndex(
                name: "IX_NHAN_VIEN_Email",
                table: "NHAN_VIEN",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NHAN_VIEN_MaNv",
                table: "NHAN_VIEN",
                column: "MaNv",
                unique: true,
                filter: "[MaNv] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NHAN_VIEN_RapChieuId",
                table: "NHAN_VIEN",
                column: "RapChieuId");

            migrationBuilder.CreateIndex(
                name: "IX_PHIM_MaPhim",
                table: "PHIM",
                column: "MaPhim",
                unique: true,
                filter: "[MaPhim] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PHIM_THE_LOAI_TheLoaisId",
                table: "PHIM_THE_LOAI",
                column: "TheLoaisId");

            migrationBuilder.CreateIndex(
                name: "IX_PHONG_CHIEU_MaPhong",
                table: "PHONG_CHIEU",
                column: "MaPhong",
                unique: true,
                filter: "[MaPhong] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PHONG_CHIEU_RapChieuId",
                table: "PHONG_CHIEU",
                column: "RapChieuId");

            migrationBuilder.CreateIndex(
                name: "IX_RAP_CHIEU_MaRap",
                table: "RAP_CHIEU",
                column: "MaRap",
                unique: true,
                filter: "[MaRap] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RAP_CHIEU_Sdt",
                table: "RAP_CHIEU",
                column: "Sdt",
                unique: true,
                filter: "[Sdt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_THANH_TOAN_DatVeId",
                table: "THANH_TOAN",
                column: "DatVeId");

            migrationBuilder.CreateIndex(
                name: "IX_THANH_TOAN_MaTt",
                table: "THANH_TOAN",
                column: "MaTt",
                unique: true,
                filter: "[MaTt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_THE_LOAI_MaTheLoai",
                table: "THE_LOAI",
                column: "MaTheLoai",
                unique: true,
                filter: "[MaTheLoai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_THE_LOAI_TenTheLoai",
                table: "THE_LOAI",
                column: "TenTheLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VE_DatVeId",
                table: "VE",
                column: "DatVeId");

            migrationBuilder.CreateIndex(
                name: "IX_VE_GheId",
                table: "VE",
                column: "GheId");

            migrationBuilder.CreateIndex(
                name: "IX_VE_LichChieuId_GheId",
                table: "VE",
                columns: new[] { "LichChieuId", "GheId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VE_MaVe",
                table: "VE",
                column: "MaVe",
                unique: true,
                filter: "[MaVe] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "PHIM_THE_LOAI");

            migrationBuilder.DropTable(
                name: "THANH_TOAN");

            migrationBuilder.DropTable(
                name: "VE");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "THE_LOAI");

            migrationBuilder.DropTable(
                name: "DAT_VE");

            migrationBuilder.DropTable(
                name: "GHE");

            migrationBuilder.DropTable(
                name: "LICH_CHIEU");

            migrationBuilder.DropTable(
                name: "KHACH_HANG");

            migrationBuilder.DropTable(
                name: "KHUYEN_MAI");

            migrationBuilder.DropTable(
                name: "NHAN_VIEN");

            migrationBuilder.DropTable(
                name: "PHIM");

            migrationBuilder.DropTable(
                name: "PHONG_CHIEU");

            migrationBuilder.DropTable(
                name: "RAP_CHIEU");
        }
    }
}
