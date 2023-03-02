using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Domain.Migrations
{
    public partial class addedMemorizationRecords1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations");

            migrationBuilder.AddColumn<Guid>(
                name: "MemorizationId",
                table: "GuardianMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations",
                column: "StudentId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations");

            migrationBuilder.DropColumn(
                name: "MemorizationId",
                table: "GuardianMembers");

            migrationBuilder.CreateIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations",
                column: "StudentId");
        }
    }
}
