using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltraStrore.Migrations
{
    /// <inheritdoc />
    public partial class addOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Otp",
                table: "NGUOI_DUNG",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiry",
                table: "NGUOI_DUNG",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NGUOI_DUNG",
                keyColumn: "ma_nguoi_dung",
                keyValue: "KH001",
                columns: new[] { "ngay_sinh", "ngay_tao", "Otp", "OtpExpiry" },
                values: new object[] { new DateTime(2025, 3, 11, 21, 25, 48, 321, DateTimeKind.Local).AddTicks(8374), new DateTime(2025, 3, 11, 21, 25, 48, 322, DateTimeKind.Local).AddTicks(3787), null, null });

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 11));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Otp",
                table: "NGUOI_DUNG");

            migrationBuilder.DropColumn(
                name: "OtpExpiry",
                table: "NGUOI_DUNG");

            migrationBuilder.UpdateData(
                table: "NGUOI_DUNG",
                keyColumn: "ma_nguoi_dung",
                keyValue: "KH001",
                columns: new[] { "ngay_sinh", "ngay_tao" },
                values: new object[] { new DateTime(2025, 3, 9, 17, 12, 33, 30, DateTimeKind.Local).AddTicks(7792), new DateTime(2025, 3, 9, 17, 12, 33, 31, DateTimeKind.Local).AddTicks(5372) });

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 9));
        }
    }
}
