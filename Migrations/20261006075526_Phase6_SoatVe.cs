using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatVeXemPhim.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_SoatVe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NhanVienSoatVeId",
                table: "VE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianSoatVe",
                table: "VE",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VE_NhanVienSoatVeId",
                table: "VE",
                column: "NhanVienSoatVeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VE_NHAN_VIEN_NhanVienSoatVeId",
                table: "VE",
                column: "NhanVienSoatVeId",
                principalTable: "NHAN_VIEN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VE_NHAN_VIEN_NhanVienSoatVeId",
                table: "VE");

            migrationBuilder.DropIndex(
                name: "IX_VE_NhanVienSoatVeId",
                table: "VE");

            migrationBuilder.DropColumn(
                name: "NhanVienSoatVeId",
                table: "VE");

            migrationBuilder.DropColumn(
                name: "ThoiGianSoatVe",
                table: "VE");
        }
    }
}
