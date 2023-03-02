using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Domain.Migrations
{
    public partial class addedMemorizationRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Memorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentSurah = table.Column<int>(type: "int", nullable: false),
                    SurahMemorized = table.Column<int>(type: "int", nullable: false),
                    TotalGrade = table.Column<double>(type: "float", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memorizations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "GuardianMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemorizationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SurahId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    MemorizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemorizationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemorizationRecords_Memorizations_MemorizationId",
                        column: x => x.MemorizationId,
                        principalTable: "Memorizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemorizationRecords_MemorizationId",
                table: "MemorizationRecords",
                column: "MemorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorizationRecords");

            migrationBuilder.DropTable(
                name: "Memorizations");
        }
    }
}
