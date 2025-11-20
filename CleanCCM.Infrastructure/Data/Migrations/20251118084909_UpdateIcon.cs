using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanCCM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Tags");
        }
    }
}
