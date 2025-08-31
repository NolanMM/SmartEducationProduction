using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityReconmmendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityRecommendations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTimeRequest = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineerConnection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearningObjectives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationStandards = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialLists = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorksheetsAndAttachments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntroductionMotivation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Procedure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Assessments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SafetyIssues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TroubleshootingTips = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityExtensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityScaling = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: true),
                    CompletionTokens = table.Column<int>(type: "int", nullable: true),
                    TotalTokens = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityRecommendations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRecommendations_UserId",
                table: "ActivityRecommendations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityRecommendations");
        }
    }
}
