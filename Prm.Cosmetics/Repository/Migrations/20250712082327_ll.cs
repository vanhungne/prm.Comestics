using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "Phone", "address" },
                values: new object[] { new DateTime(2025, 7, 12, 15, 23, 26, 255, DateTimeKind.Local).AddTicks(6877), "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash", "Phone", "address" },
                values: new object[] { new DateTime(2025, 7, 12, 15, 23, 26, 255, DateTimeKind.Local).AddTicks(6880), "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash", "Phone", "address" },
                values: new object[] { new DateTime(2025, 7, 12, 15, 23, 26, 255, DateTimeKind.Local).AddTicks(6881), "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "address",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 6, 23, 14, 56, 19, 45, DateTimeKind.Local).AddTicks(715), "passwordAdmin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 6, 23, 14, 56, 19, 45, DateTimeKind.Local).AddTicks(717), "passwordCustomer1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 6, 23, 14, 56, 19, 45, DateTimeKind.Local).AddTicks(718), "passwordCustomer2" });
        }
    }
}
