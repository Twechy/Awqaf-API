using Microsoft.EntityFrameworkCore.Migrations;

namespace BlockerLib.Migrations
{
    public partial class userBlocksfirstMig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    UserKey = table.Column<string>(nullable: false),
                    IsBlocked = table.Column<bool>(nullable: false),
                    NumberOFRetry = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => x.UserKey);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBlocks");
        }
    }
}