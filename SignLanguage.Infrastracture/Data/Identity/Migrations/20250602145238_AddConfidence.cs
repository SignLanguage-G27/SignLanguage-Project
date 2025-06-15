using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignLanguage.Infrastracture.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddConfidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Confidence",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "Logs");
        }
    }
}
