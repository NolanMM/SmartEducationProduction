using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Migrations
{
    /// <inheritdoc />
    public partial class AddNGSSandGradeStandards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NGSS_Detailed_Standard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title_NGSS_Standard = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Matter_Interactions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Science_Engineering_Practices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Disciplinary_Core_Ideas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Crosscutting_Concepts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Connections_To_Other_DCI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Common_Core_State_Standards_Connections = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Articulation_of_DCIs_across_grade_levels = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NGSS_Detailed_Standard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NGSS_Standard",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title_Grade_Standard = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NGSS_Standard", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NGSS_Detailed_Standard");

            migrationBuilder.DropTable(
                name: "NGSS_Standard");
        }
    }
}
