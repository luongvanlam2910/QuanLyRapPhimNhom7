using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatVeXemPhim.Migrations
{
    /// <inheritdoc />
    public partial class Phase0_SuaLoiNghiepVu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VE_LichChieuId_GheId",
                table: "VE");

            migrationBuilder.DropColumn(
                name: "MatKhau",
                table: "NHAN_VIEN");

            migrationBuilder.DropColumn(
                name: "MatKhau",
                table: "KHACH_HANG");

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "KHACH_HANG",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "KhachHangId",
                table: "DAT_VE",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "SdtVangLai",
                table: "DAT_VE",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenKhachVangLai",
                table: "DAT_VE",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VE_LichChieuId_GheId",
                table: "VE",
                columns: new[] { "LichChieuId", "GheId" },
                unique: true,
                filter: "[TrangThai] <> N'Đã hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VE_LichChieuId_GheId",
                table: "VE");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "KHACH_HANG");

            migrationBuilder.DropColumn(
                name: "SdtVangLai",
                table: "DAT_VE");

            migrationBuilder.DropColumn(
                name: "TenKhachVangLai",
                table: "DAT_VE");

            migrationBuilder.AddColumn<string>(
                name: "MatKhau",
                table: "NHAN_VIEN",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatKhau",
                table: "KHACH_HANG",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "KhachHangId",
                table: "DAT_VE",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VE_LichChieuId_GheId",
                table: "VE",
                columns: new[] { "LichChieuId", "GheId" },
                unique: true);
        }
    }
}
