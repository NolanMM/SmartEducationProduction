using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Migrations
{
    /// <inheritdoc />
    public partial class addnameofactivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameOfActivity",
                table: "ActivityRecommendations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameOfActivity",
                table: "ActivityRecommendations");
        }
    }
}
