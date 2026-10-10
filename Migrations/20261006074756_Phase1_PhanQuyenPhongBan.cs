using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatVeXemPhim.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_PhanQuyenPhongBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cccd",
                table: "NHAN_VIEN",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgaySinh",
                table: "NHAN_VIEN",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhongBanId",
                table: "NHAN_VIEN",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PHONG_BAN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPb = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenPb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHONG_BAN", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NHAN_VIEN_PhongBanId",
                table: "NHAN_VIEN",
                column: "PhongBanId");

            migrationBuilder.CreateIndex(
                name: "IX_PHONG_BAN_MaPb",
                table: "PHONG_BAN",
                column: "MaPb",
                unique: true,
                filter: "[MaPb] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PHONG_BAN_TenPb",
                table: "PHONG_BAN",
                column: "TenPb",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NHAN_VIEN_PHONG_BAN_PhongBanId",
                table: "NHAN_VIEN",
                column: "PhongBanId",
                principalTable: "PHONG_BAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NHAN_VIEN_PHONG_BAN_PhongBanId",
                table: "NHAN_VIEN");

            migrationBuilder.DropTable(
                name: "PHONG_BAN");

            migrationBuilder.DropIndex(
                name: "IX_NHAN_VIEN_PhongBanId",
                table: "NHAN_VIEN");

            migrationBuilder.DropColumn(
                name: "Cccd",
                table: "NHAN_VIEN");

            migrationBuilder.DropColumn(
                name: "NgaySinh",
                table: "NHAN_VIEN");

            migrationBuilder.DropColumn(
                name: "PhongBanId",
                table: "NHAN_VIEN");
        }
    }
}
