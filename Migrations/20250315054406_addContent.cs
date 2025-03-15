using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltraStrore.Migrations
{
    /// <inheritdoc />
    public partial class addContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NGUOI_DUNG",
                keyColumn: "ma_nguoi_dung",
                keyValue: "KH001",
                columns: new[] { "ngay_sinh", "ngay_tao" },
                values: new object[] { new DateTime(2025, 3, 15, 12, 44, 2, 909, DateTimeKind.Local).AddTicks(940), new DateTime(2025, 3, 15, 12, 44, 2, 909, DateTimeKind.Local).AddTicks(9071) });

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00001_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff0000_XXL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_M",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));

            migrationBuilder.UpdateData(
                table: "SAN_PHAM",
                keyColumn: "ma_san_pham",
                keyValue: "A00002_ff00ff_XL",
                column: "ngay_tao",
                value: new DateOnly(2025, 3, 15));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NGUOI_DUNG",
                keyColumn: "ma_nguoi_dung",
                keyValue: "KH001",
                columns: new[] { "ngay_sinh", "ngay_tao" },
                values: new object[] { new DateTime(2025, 3, 11, 21, 25, 48, 321, DateTimeKind.Local).AddTicks(8374), new DateTime(2025, 3, 11, 21, 25, 48, 322, DateTimeKind.Local).AddTicks(3787) });

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
    }
}
