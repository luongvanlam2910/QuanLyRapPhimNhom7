/* =====================================================================
   CreateDatabase.sql — Script tạo toàn bộ bảng cho dự án DatVeXemPhim
   =====================================================================
   File này được SINH TỰ ĐỘNG từ Entity Framework Core Migrations bằng lệnh:
       dotnet ef migrations script -o CreateDatabase.sql --idempotent
   (không viết tay) nên luôn khớp 100% với cấu trúc entity hiện tại của dự án.

   LƯU Ý QUAN TRỌNG:
   - Bình thường KHÔNG CẦN chạy file này thủ công: chỉ cần chạy
     `dotnet ef database update` (hoặc chạy `dotnet run` — SeedData.cs tự
     gọi Database.MigrateAsync() khi khởi động) là bảng sẽ được tạo tự động.
   - File này chỉ dùng khi muốn xem trước cấu trúc CSDL, hoặc đưa cho người
     không cài .NET/EF Tools tự chạy bằng tay qua sqlcmd/SSMS.
   - Script có tính "idempotent": chạy lại nhiều lần vẫn an toàn — nó tự
     kiểm tra bảng __EFMigrationsHistory để biết migration nào đã áp dụng,
     không tạo trùng bảng.
   - File KHÔNG chứa dữ liệu mẫu/dữ liệu ảo nào — chỉ tạo bảng + ràng buộc
     (index, khóa ngoại, unique...). Vai trò hệ thống (Admin/QuanLy/NhanVien/
     KhachHang) và 1 tài khoản Admin đầu tiên sẽ do SeedData.cs tự tạo khi
     chạy ứng dụng lần đầu (đọc cấu hình app:AdminEmail / app:AdminPassword).

   Cách chạy thủ công bằng sqlcmd:
       sqlcmd -S <tên_server> -d <tên_database> -i CreateDatabase.sql
   (nếu database đã tồn tại nhưng chưa có bảng nào; sqlcmd cần quyền tạo bảng
   trên database đó.)
   ===================================================================== */

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [HoTen] nvarchar(max) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [KHACH_HANG] (
        [Id] int NOT NULL IDENTITY,
        [MaKh] nvarchar(10) NULL,
        [UserId] nvarchar(max) NULL,
        [HoTen] nvarchar(100) NOT NULL,
        [Sdt] nvarchar(15) NOT NULL,
        [Email] nvarchar(100) NULL,
        [MatKhau] nvarchar(200) NULL,
        [NgayDangKy] date NULL,
        CONSTRAINT [PK_KHACH_HANG] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [KHUYEN_MAI] (
        [Id] int NOT NULL IDENTITY,
        [MaKm] nvarchar(10) NULL,
        [TenKm] nvarchar(120) NOT NULL,
        [LoaiGiam] nvarchar(20) NOT NULL,
        [GiaTri] decimal(12,2) NOT NULL,
        [NgayBatDau] date NULL,
        [NgayKetThuc] date NULL,
        CONSTRAINT [PK_KHUYEN_MAI] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [PHIM] (
        [Id] int NOT NULL IDENTITY,
        [MaPhim] nvarchar(10) NULL,
        [TenPhim] nvarchar(150) NOT NULL,
        [ThoiLuong] int NOT NULL,
        [DaoDien] nvarchar(100) NULL,
        [DoTuoi] nvarchar(10) NULL,
        [NgayKhoiChieu] date NULL,
        [TrangThai] nvarchar(30) NULL,
        [AnhPoster] nvarchar(200) NULL,
        [MoTa] nvarchar(2000) NULL,
        CONSTRAINT [PK_PHIM] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [RAP_CHIEU] (
        [Id] int NOT NULL IDENTITY,
        [MaRap] nvarchar(10) NULL,
        [TenRap] nvarchar(100) NOT NULL,
        [DiaChi] nvarchar(200) NOT NULL,
        [Sdt] nvarchar(15) NULL,
        CONSTRAINT [PK_RAP_CHIEU] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [THE_LOAI] (
        [Id] int NOT NULL IDENTITY,
        [MaTheLoai] nvarchar(10) NULL,
        [TenTheLoai] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_THE_LOAI] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [NHAN_VIEN] (
        [Id] int NOT NULL IDENTITY,
        [MaNv] nvarchar(10) NULL,
        [UserId] nvarchar(max) NULL,
        [RapChieuId] int NOT NULL,
        [HoTen] nvarchar(100) NOT NULL,
        [ChucVu] nvarchar(50) NULL,
        [Sdt] nvarchar(15) NULL,
        [Email] nvarchar(100) NULL,
        [MatKhau] nvarchar(200) NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_NHAN_VIEN] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NHAN_VIEN_RAP_CHIEU_RapChieuId] FOREIGN KEY ([RapChieuId]) REFERENCES [RAP_CHIEU] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [PHONG_CHIEU] (
        [Id] int NOT NULL IDENTITY,
        [MaPhong] nvarchar(10) NULL,
        [RapChieuId] int NOT NULL,
        [TenPhong] nvarchar(50) NOT NULL,
        [LoaiPhong] nvarchar(30) NULL,
        [SucChua] int NOT NULL,
        CONSTRAINT [PK_PHONG_CHIEU] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PHONG_CHIEU_RAP_CHIEU_RapChieuId] FOREIGN KEY ([RapChieuId]) REFERENCES [RAP_CHIEU] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [PHIM_THE_LOAI] (
        [PhimsId] int NOT NULL,
        [TheLoaisId] int NOT NULL,
        CONSTRAINT [PK_PHIM_THE_LOAI] PRIMARY KEY ([PhimsId], [TheLoaisId]),
        CONSTRAINT [FK_PHIM_THE_LOAI_PHIM_PhimsId] FOREIGN KEY ([PhimsId]) REFERENCES [PHIM] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PHIM_THE_LOAI_THE_LOAI_TheLoaisId] FOREIGN KEY ([TheLoaisId]) REFERENCES [THE_LOAI] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [DAT_VE] (
        [Id] int NOT NULL IDENTITY,
        [MaDat] nvarchar(10) NULL,
        [KhachHangId] int NOT NULL,
        [NhanVienId] int NULL,
        [KhuyenMaiId] int NULL,
        [NgayDat] datetime2 NOT NULL,
        [TongTien] decimal(12,2) NOT NULL DEFAULT 0.0,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_DAT_VE] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DAT_VE_KHACH_HANG_KhachHangId] FOREIGN KEY ([KhachHangId]) REFERENCES [KHACH_HANG] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DAT_VE_KHUYEN_MAI_KhuyenMaiId] FOREIGN KEY ([KhuyenMaiId]) REFERENCES [KHUYEN_MAI] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DAT_VE_NHAN_VIEN_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [NHAN_VIEN] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [GHE] (
        [Id] int NOT NULL IDENTITY,
        [MaGhe] nvarchar(10) NULL,
        [PhongChieuId] int NOT NULL,
        [HangGhe] nvarchar(5) NOT NULL,
        [SoGhe] int NOT NULL,
        [LoaiGhe] nvarchar(30) NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_GHE] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GHE_PHONG_CHIEU_PhongChieuId] FOREIGN KEY ([PhongChieuId]) REFERENCES [PHONG_CHIEU] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [LICH_CHIEU] (
        [Id] int NOT NULL IDENTITY,
        [MaLich] nvarchar(10) NULL,
        [PhimId] int NOT NULL,
        [PhongChieuId] int NOT NULL,
        [ThoiGianBatDau] datetime2 NOT NULL,
        [ThoiGianKetThuc] datetime2 NOT NULL,
        [GiaVe] decimal(12,2) NOT NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_LICH_CHIEU] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LICH_CHIEU_PHIM_PhimId] FOREIGN KEY ([PhimId]) REFERENCES [PHIM] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LICH_CHIEU_PHONG_CHIEU_PhongChieuId] FOREIGN KEY ([PhongChieuId]) REFERENCES [PHONG_CHIEU] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [THANH_TOAN] (
        [Id] int NOT NULL IDENTITY,
        [MaTt] nvarchar(10) NULL,
        [DatVeId] int NOT NULL,
        [SoTien] decimal(12,2) NOT NULL,
        [NgayTt] datetime2 NOT NULL,
        [PhuongThuc] nvarchar(50) NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_THANH_TOAN] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_THANH_TOAN_DAT_VE_DatVeId] FOREIGN KEY ([DatVeId]) REFERENCES [DAT_VE] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE TABLE [VE] (
        [Id] int NOT NULL IDENTITY,
        [MaVe] nvarchar(10) NULL,
        [DatVeId] int NOT NULL,
        [LichChieuId] int NOT NULL,
        [GheId] int NOT NULL,
        [GiaVe] decimal(12,2) NOT NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_VE] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VE_DAT_VE_DatVeId] FOREIGN KEY ([DatVeId]) REFERENCES [DAT_VE] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_VE_GHE_GheId] FOREIGN KEY ([GheId]) REFERENCES [GHE] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VE_LICH_CHIEU_LichChieuId] FOREIGN KEY ([LichChieuId]) REFERENCES [LICH_CHIEU] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DAT_VE_KhachHangId] ON [DAT_VE] ([KhachHangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DAT_VE_KhuyenMaiId] ON [DAT_VE] ([KhuyenMaiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DAT_VE_MaDat] ON [DAT_VE] ([MaDat]) WHERE [MaDat] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DAT_VE_NhanVienId] ON [DAT_VE] ([NhanVienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GHE_MaGhe] ON [GHE] ([MaGhe]) WHERE [MaGhe] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GHE_PhongChieuId_HangGhe_SoGhe] ON [GHE] ([PhongChieuId], [HangGhe], [SoGhe]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_KHACH_HANG_Email] ON [KHACH_HANG] ([Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_KHACH_HANG_MaKh] ON [KHACH_HANG] ([MaKh]) WHERE [MaKh] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KHACH_HANG_Sdt] ON [KHACH_HANG] ([Sdt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_KHUYEN_MAI_MaKm] ON [KHUYEN_MAI] ([MaKm]) WHERE [MaKm] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LICH_CHIEU_MaLich] ON [LICH_CHIEU] ([MaLich]) WHERE [MaLich] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LICH_CHIEU_PhimId] ON [LICH_CHIEU] ([PhimId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LICH_CHIEU_PhongChieuId] ON [LICH_CHIEU] ([PhongChieuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NHAN_VIEN_Email] ON [NHAN_VIEN] ([Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NHAN_VIEN_MaNv] ON [NHAN_VIEN] ([MaNv]) WHERE [MaNv] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NHAN_VIEN_RapChieuId] ON [NHAN_VIEN] ([RapChieuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PHIM_MaPhim] ON [PHIM] ([MaPhim]) WHERE [MaPhim] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PHIM_THE_LOAI_TheLoaisId] ON [PHIM_THE_LOAI] ([TheLoaisId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PHONG_CHIEU_MaPhong] ON [PHONG_CHIEU] ([MaPhong]) WHERE [MaPhong] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PHONG_CHIEU_RapChieuId] ON [PHONG_CHIEU] ([RapChieuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RAP_CHIEU_MaRap] ON [RAP_CHIEU] ([MaRap]) WHERE [MaRap] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RAP_CHIEU_Sdt] ON [RAP_CHIEU] ([Sdt]) WHERE [Sdt] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_THANH_TOAN_DatVeId] ON [THANH_TOAN] ([DatVeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_THANH_TOAN_MaTt] ON [THANH_TOAN] ([MaTt]) WHERE [MaTt] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_THE_LOAI_MaTheLoai] ON [THE_LOAI] ([MaTheLoai]) WHERE [MaTheLoai] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_THE_LOAI_TenTheLoai] ON [THE_LOAI] ([TenTheLoai]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VE_DatVeId] ON [VE] ([DatVeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VE_GheId] ON [VE] ([GheId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VE_LichChieuId_GheId] ON [VE] ([LichChieuId], [GheId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VE_MaVe] ON [VE] ([MaVe]) WHERE [MaVe] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005170031_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005170031_InitialCreate', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    DROP INDEX [IX_VE_LichChieuId_GheId] ON [VE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NHAN_VIEN]') AND [c].[name] = N'MatKhau');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [NHAN_VIEN] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [NHAN_VIEN] DROP COLUMN [MatKhau];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KHACH_HANG]') AND [c].[name] = N'MatKhau');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [KHACH_HANG] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [KHACH_HANG] DROP COLUMN [MatKhau];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    ALTER TABLE [KHACH_HANG] ADD [TrangThai] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DAT_VE]') AND [c].[name] = N'KhachHangId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DAT_VE] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [DAT_VE] ALTER COLUMN [KhachHangId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    ALTER TABLE [DAT_VE] ADD [SdtVangLai] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    ALTER TABLE [DAT_VE] ADD [TenKhachVangLai] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VE_LichChieuId_GheId] ON [VE] ([LichChieuId], [GheId]) WHERE [TrangThai] <> N''Đã hủy''');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006073617_Phase0_SuaLoiNghiepVu'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006073617_Phase0_SuaLoiNghiepVu', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    ALTER TABLE [NHAN_VIEN] ADD [Cccd] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    ALTER TABLE [NHAN_VIEN] ADD [NgaySinh] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    ALTER TABLE [NHAN_VIEN] ADD [PhongBanId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    CREATE TABLE [PHONG_BAN] (
        [Id] int NOT NULL IDENTITY,
        [MaPb] nvarchar(10) NULL,
        [TenPb] nvarchar(100) NOT NULL,
        [MoTa] nvarchar(300) NULL,
        [TrangThai] nvarchar(30) NULL,
        CONSTRAINT [PK_PHONG_BAN] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    CREATE INDEX [IX_NHAN_VIEN_PhongBanId] ON [NHAN_VIEN] ([PhongBanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PHONG_BAN_MaPb] ON [PHONG_BAN] ([MaPb]) WHERE [MaPb] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PHONG_BAN_TenPb] ON [PHONG_BAN] ([TenPb]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    ALTER TABLE [NHAN_VIEN] ADD CONSTRAINT [FK_NHAN_VIEN_PHONG_BAN_PhongBanId] FOREIGN KEY ([PhongBanId]) REFERENCES [PHONG_BAN] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006074756_Phase1_PhanQuyenPhongBan'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006074756_Phase1_PhanQuyenPhongBan', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075301_Phase2_DoTuoiTheLoai'
)
BEGIN
    ALTER TABLE [KHACH_HANG] ADD [NgaySinh] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075301_Phase2_DoTuoiTheLoai'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006075301_Phase2_DoTuoiTheLoai', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075526_Phase6_SoatVe'
)
BEGIN
    ALTER TABLE [VE] ADD [NhanVienSoatVeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075526_Phase6_SoatVe'
)
BEGIN
    ALTER TABLE [VE] ADD [ThoiGianSoatVe] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075526_Phase6_SoatVe'
)
BEGIN
    CREATE INDEX [IX_VE_NhanVienSoatVeId] ON [VE] ([NhanVienSoatVeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075526_Phase6_SoatVe'
)
BEGIN
    ALTER TABLE [VE] ADD CONSTRAINT [FK_VE_NHAN_VIEN_NhanVienSoatVeId] FOREIGN KEY ([NhanVienSoatVeId]) REFERENCES [NHAN_VIEN] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006075526_Phase6_SoatVe'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006075526_Phase6_SoatVe', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006080158_Phase5_ThanhToanGateway'
)
BEGIN
    ALTER TABLE [DAT_VE] ADD [HanThanhToan] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006080158_Phase5_ThanhToanGateway'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006080158_Phase5_ThanhToanGateway', N'8.0.10');
END;
GO

COMMIT;
GO

