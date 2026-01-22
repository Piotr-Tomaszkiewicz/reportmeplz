using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MojeApiMariaDB.Migrations
{
    /// <inheritdoc />
    public partial class StoreOriginalFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileRoute",
                table: "Reports",
                newName: "FileName");

            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "Reports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Reports",
                newName: "FileRoute");
        }
    }
}
