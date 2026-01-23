using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MojeApiMariaDB.Migrations
{
    /// <inheritdoc />
    public partial class AddDatesToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateReported",
                table: "Reports",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateResolved",
                table: "Reports",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateReported",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DateResolved",
                table: "Reports");
        }
    }
}
